using System;
using System.Threading;
using Agenix.Core.Exceptions;
using Agenix.Core.Util;
using log4net;

namespace Agenix.Core.Actions;

/// <summary>
///     Stop the test execution for a given amount of time.
/// </summary>
/// <param name="builder"></param>
public class SleepAction(SleepAction.Builder builder) : AbstractTestAction("sleep", builder)
{
    /// Logger for SleepAction.
    /// /
    private static readonly ILog Log = LogManager.GetLogger(typeof(SleepAction));

    public string Time { get; } = builder.time;

    public ScheduledExecutor.TimeUnit TimeUnit { get; } = builder.timeUnit;

    /// <summary>
    ///     Executes the sleep action, causing the current thread to pause execution for the specified duration.
    /// </summary>
    /// <param name="context">The test context that provides the dynamic resolution of the sleep duration value.</param>
    public override void DoExecute(TestContext context)
    {
        var duration = context.ResolveDynamicValue(Time);

        try
        {
            Log.Info($"Sleeping {duration} {TimeUnit}");

            if (duration.IndexOf('.') > 0)
                switch (TimeUnit)
                {
                    case ScheduledExecutor.TimeUnit.MILLISECONDS:
                        Thread.Sleep((int)Math.Round(double.Parse(duration)));
                        break;
                    case ScheduledExecutor.TimeUnit.SECONDS:
                        Thread.Sleep((int)Math.Round(double.Parse(duration) * 1000));
                        break;
                    case ScheduledExecutor.TimeUnit.MINUTES:
                        Thread.Sleep((int)Math.Round(double.Parse(duration) * 60 * 1000));
                        break;
                    default:
                        throw new CoreSystemException("Unsupported time expression for sleep action - " +
                                                      "please use one of milliseconds, seconds, minutes");
                }
            else
                Thread.Sleep(int.Parse(duration));

            Log.Info($"Returning after {duration} {TimeUnit}");
        }
        catch (ThreadInterruptedException e)
        {
            throw new CoreSystemException(e.Message);
        }
    }

    public sealed class Builder : AbstractTestActionBuilder<ITestAction, dynamic>
    {
        internal string time = "5000";
        internal ScheduledExecutor.TimeUnit timeUnit = ScheduledExecutor.TimeUnit.MILLISECONDS;

        public static Builder Delay()
        {
            return new Builder();
        }

        public static Builder Sleep()
        {
            return new Builder();
        }

        public Builder Milliseconds(int milliseconds)
        {
            return Time(milliseconds.ToString(), ScheduledExecutor.TimeUnit.MILLISECONDS);
        }

        public Builder Milliseconds(long milliseconds)
        {
            return Time(milliseconds.ToString(), ScheduledExecutor.TimeUnit.MILLISECONDS);
        }

        public Builder Milliseconds(string expression)
        {
            Time(expression, ScheduledExecutor.TimeUnit.MILLISECONDS);
            return this;
        }

        public Builder Seconds(double seconds)
        {
            Milliseconds((int)Math.Round(seconds * 1000));
            return this;
        }

        public Builder Seconds(int seconds)
        {
            return Time((seconds * 1000L).ToString(), ScheduledExecutor.TimeUnit.MILLISECONDS);
        }

        public Builder Seconds(long seconds)
        {
            return Time((seconds * 1000L).ToString(), ScheduledExecutor.TimeUnit.MILLISECONDS);
        }

        public Builder Time(TimeSpan duration)
        {
            Milliseconds(duration.Milliseconds);
            return this;
        }

        public Builder Time(string expression, ScheduledExecutor.TimeUnit timeUnit)
        {
            time = expression;
            this.timeUnit = timeUnit;
            return this;
        }

        public override SleepAction Build()
        {
            return new SleepAction(this);
        }
    }
}