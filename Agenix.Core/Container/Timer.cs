using System;
using System.Threading;
using System.Threading.Tasks;
using Agenix.Core.Exceptions;
using Agenix.Core.Util;
using log4net;

namespace Agenix.Core.Container;

/// Represents a configurable timer that can execute a set of actions periodically and is capable of running asynchronously or synchronously.
/// The Timer class allows setting parameters such as delay, interval, and repeat count through its builder, and provides functionality to stop the timer.
public class Timer(Timer.Builder builder)
    : AbstractActionContainer(builder.GetName() ?? "timer", builder.GetDescription(), builder.GetActions()), IStopTimer
{
    public const string IndexSuffix = "-index";

    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(Timer));

    private static readonly AtomicLong nextSerialNumber = new();
    private System.Timers.Timer _timer;

    protected bool timerComplete;
    protected CoreSystemException timerException;

    /// <summary>
    ///     Determines whether the timer should run in a separate task.
    /// </summary>
    public bool Fork { get; } = builder.fork;

    /// <summary>
    ///     Represents the interval in milliseconds for the timer execution cycle.
    /// </summary>
    public long Interval { get; } = builder.interval;

    /// <summary>
    ///     Represents the initial delay before the timer starts executing its actions.
    /// </summary>
    public long Delay { get; } = builder.delay;

    /// <summary>
    ///     Specifies the number of times the timer will repeat its execution cycle.
    /// </summary>
    public int RepeatCount { get; } = builder.repeatCount;

    /// <summary>
    ///     Gets the unique identifier for this timer instance.
    /// </summary>
    public string TimerId { get; } = builder.timerId;

    public CoreSystemException TimerException => timerException;

    /// Stops the timer, ensuring it is disposed of properly, and marks the timer as complete.
    public void StopTimer()
    {
        _timer.Stop();
        _timer.Dispose();
        timerComplete = true;
    }

    /// Executes the timer in the given test context, determining whether to run the timer asynchronously or synchronously based on the configuration.
    /// <param name="context">The test context within which the timer executes.</param>
    public override void DoExecute(TestContext context)
    {
        if (Fork)
            Task.Run(() => ConfigureAndRunTimer(context));
        else
            ConfigureAndRunTimer(context);
    }

    /// Configures and runs the timer associated with the given test context.
    /// <param name="context">The test context within which the timer operates.</param>
    private void ConfigureAndRunTimer(TestContext context)
    {
        // Register the timer with the test context using its ID
        context.RegisterTimer(TimerId, this);

        // Use a lock for thread safety if multiple threads might access indexCount
        var lockObj = new object();
        var indexCount = 0;
        var timerComplete = false;

        _timer = new System.Timers.Timer(Interval);
        _timer.Elapsed += (sender, args) =>
        {
            lock (lockObj)
            {
                if (indexCount >= RepeatCount)
                {
                    if (!timerComplete)
                    {
                        Log.Debug($"Timer complete: {RepeatCount} iterations reached");
                        StopTimer();
                        timerComplete = true;
                    }

                    return;
                }

                try
                {
                    indexCount++;
                    UpdateIndexCountInTestContext(context, indexCount);
                    Log.Debug($"Timer event fired #{indexCount} - executing nested actions");

                    foreach (var actionBuilder in actions) ExecuteAction(actionBuilder.Build(), context);
                }
                catch (Exception e)
                {
                    HandleException(e, context);

                    // Make sure the loop exits by setting timerComplete to true
                    lock (lockObj) // Ensure thread-safety when setting the flag
                    {
                        timerComplete = true;
                    }
                }
            }
        };

        _timer.AutoReset = true;
        _timer.Start();
        Thread.Sleep((int)Delay);

        while (!timerComplete)
            try
            {
                Thread.Sleep((int)Interval);
            }
            catch (ThreadInterruptedException e)
            {
                Log.Warn("Interrupted while waiting for timer to complete", e);
            }

        if (TimerException != null) throw TimerException;
    }

    /// Updates the current index count in the given test context.
    /// <param name="context">The test context where the index count should be updated.</param>
    /// <param name="indexCount">The current index count to be updated in the context.</param>
    private void UpdateIndexCountInTestContext(TestContext context, int indexCount)
    {
        context.SetVariable(TimerId + IndexSuffix, indexCount.ToString());
    }

    /// Handles exceptions that occur during the execution of a Timer instance.
    /// <param name="e">The exception that was raised during timer execution.</param>
    /// <param name="context">The context of the test where the exception occurred.</param>
    private void HandleException(Exception e, TestContext context)
    {
        if (e is CoreSystemException coreSystemException)
            timerException = coreSystemException;
        else
            timerException = new CoreSystemException(e.Message, e);
        Log.Error($"Timer stopped as a result of nested action error ({e.Message})");
        StopTimer();

        if (Fork) context.AddException(TimerException);
    }


    /// Generates and returns a unique serial number as an integer.
    /// return A unique serial number as an integer.
    private static int SerialNumber()
    {
        return (int)nextSerialNumber.IncrementAndGet();
    }

    /// Builder class for constructing Timer objects with customizable properties such as delay, interval, and repeat count.
    /// /
    public class Builder : AbstractTestContainerBuilder<Timer, Builder>
    {
        internal int delay;
        internal bool fork;
        internal long interval = 1000L;
        internal int repeatCount = int.MaxValue;
        internal string timerId;

        /// Fluent API entry point for creating and configuring Timer objects in a C# DSL.
        /// @return A new instance of Timer.Builder for chaining configuration methods.
        /// /
        public static Builder Timer()
        {
            return new Builder();
        }

        /// Sets the initial delay in milliseconds before the first timer event should fire.
        /// <param name="newDelay">The delay in milliseconds.</param>
        /// <return>The current instance of Builder for chaining configuration methods.</return>
        public Builder Delay(int newDelay)
        {
            delay = newDelay;
            return this;
        }

        /// Sets the interval in milliseconds between each timer event. Once the interval has elapsed, the next timer event is fired.
        /// <param name="newInterval">The interval in milliseconds between each timer event.</param>
        /// <return>The current instance of Builder for chaining configuration methods.</return>
        public Builder Interval(long newInterval)
        {
            interval = newInterval;
            return this;
        }

        /// Sets the maximum number of times the timer event will be fired before the timer is stopped.
        /// <param name="newRepeatCount">The maximum number of times the timer should fire.</param>
        /// <return>The current instance of Builder for chaining configuration methods.</return>
        public Builder RepeatCount(int newRepeatCount)
        {
            repeatCount = newRepeatCount;
            return this;
        }

        /// Configures the timer to allow it to run in parallel with other actions.
        /// <param name="newFork">A boolean value indicating whether the timer should be forked.</param>
        /// <return>The current instance of Builder for chaining configuration methods.</return>
        public Builder Fork(bool newFork)
        {
            fork = newFork;
            return this;
        }

        /// Sets the timer's identifier, allowing it to be referenced by other test actions such as stop-timer.
        /// <param name="newTimerId">A unique identifier for the timer within the test context.</param>
        /// <return>The current instance of Builder for chaining configuration methods.</return>
        public Builder Id(string newTimerId)
        {
            timerId = newTimerId;
            return this;
        }

        /// Sets the timer's unique identifier, enabling reference from other test actions like stop-timer.
        /// <param name="newTimerId">A unique identifier for the timer within the test context.</param>
        /// <return>The current instance of Builder for chaining configuration methods.</return>
        public Builder TimerId(string newTimerId)
        {
            timerId = newTimerId;
            return this;
        }

        protected override Timer DoBuild()
        {
            if (string.IsNullOrEmpty(timerId)) timerId = "agenix-timer-" + SerialNumber();

            return new Timer(this);
        }
    }
}