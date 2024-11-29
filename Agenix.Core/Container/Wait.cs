using System;
using System.Threading;
using System.Threading.Tasks;
using Agenix.Core.Actions;
using Agenix.Core.Condition;
using Agenix.Core.Exceptions;
using log4net;

namespace Agenix.Core.Container;

/// <summary>
///     Pause the test execution until the condition is met or the wait time has been exceeded.
/// </summary>
/// <param name="builder"></param>
public class Wait(Wait.Builder<ICondition> builder)
    : AbstractTestAction(builder.GetName() ?? "wait", builder.GetDescription())
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(Wait));

    public string Time { get; } = builder.time;

    public ICondition Condition { get; } = builder.condition;

    public string Interval { get; } = builder.interval;

    /// Executes the waiting process for a specified condition within the test context.
    /// <param name="context">
    ///     The test context containing dynamic configurations and conditions to be evaluated during
    ///     execution.
    /// </param>
    public override void DoExecute(TestContext context)
    {
        bool? conditionSatisfied = null;
        var timeLeft = GetWaitTimeMs(context);
        var intervalMs = GetIntervalMs(context);

        if (intervalMs > timeLeft) intervalMs = timeLeft;

        var callable = async () => await Task.FromResult(Condition.IsSatisfied(context));

        while (timeLeft > 0)
        {
            timeLeft -= intervalMs;

            if (Log.IsDebugEnabled) Log.Debug($"Waiting for condition {Condition.GetName()}");

            var task = Task.Run(callable);
            var checkStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            try
            {
                conditionSatisfied = task.Wait(TimeSpan.FromMilliseconds(intervalMs)) && task.Result;
            }
            catch (AggregateException e) when (e.InnerException is TimeoutException ||
                                               e.InnerException is AggregateException)
            {
                Log.Warn($"Condition check interrupted with '{e.InnerException.GetType().Name}'");
            }

            if (conditionSatisfied == true)
            {
                Log.Info(Condition.GetSuccessMessage(context));
                return;
            }

            var sleepTime = intervalMs - (DateTimeOffset.Now.ToUnixTimeMilliseconds() - checkStartTime);

            if (sleepTime > 0)
                try
                {
                    Thread.Sleep(Convert.ToInt32(sleepTime));
                }
                catch (ThreadInterruptedException e)
                {
                    Log.Warn("Interrupted during wait!", e);
                }
        }

        throw new CoreSystemException(Condition.GetErrorMessage(context));
    }


    private long GetWaitTimeMs(TestContext context)
    {
        return long.Parse(context.ReplaceDynamicContentInString(Time));
    }

    private long GetIntervalMs(TestContext context)
    {
        return long.Parse(context.ReplaceDynamicContentInString(Interval));
    }

    public class Builder<C> : AbstractTestActionBuilder<Wait, Builder<C>>,
        ITestActionBuilder<Wait>.IDelegatingTestActionBuilder<Wait> where C : ICondition
    {
        internal C condition;

        protected ITestActionBuilder<ITestAction> dlg;
        internal string interval = "1000";
        internal string time = "5000";

        public C GetCondition => condition;

        public override Wait Build()
        {
            return new Wait(new Builder<ICondition> { condition = condition, interval = interval, time = time });
        }

        public ITestActionBuilder<Wait> Delegate { get; }

        /// Fluent API action building entry method used in C# DSL.
        /// <return>A builder for creating Wait actions with specified conditions.</return>
        /// /
        public static Builder<B> WaitFor<B>() where B : ICondition
        {
            return new Builder<B>();
        }

        /// Sets the condition for the wait action.
        /// <param name="cnd">The condition to be applied to the wait action. It must implement the ICondition interface.</param>
        /// <returns>The builder instance for chaining further configurations on the wait action.</returns>
        public Builder<C> Condition(C cnd)
        {
            condition = cnd;
            dlg = this;
            return this;
        }

        /// Sets the condition for the wait action.
        /// <param name="conditionBuilder">
        ///     A builder that specifies the wait condition to be set. This builder must be of type
        ///     WaitConditionBuilder.
        /// </param>
        /// <returns>The instance of the condition builder for further configuration and chaining.</returns>
        public T Condition<T>(T conditionBuilder) where T : WaitConditionBuilder<C, T>
        {
            condition = conditionBuilder.GetCondition();
            dlg = conditionBuilder;
            return conditionBuilder;
        }

        /// Initiates the construction of a wait condition based on a message within the Agenix framework.
        /// This method is part of the fluent API provided by the Wait class to facilitate the creation
        /// and configuration of conditions that rely on message detection during a wait operation.
        /// <returns>
        ///     Returns an instance of WaitMessageConditionBuilder, enabling further configuration
        ///     of a message-based wait condition within the context of the Agenix framework.
        /// </returns>
        public virtual WaitMessageConditionBuilder Message()
        {
            return new MessageConditionBuilder<MessageCondition>().Message();
        }

        /// Provides a mechanism to build a wait action condition based on a specified action.
        /// The method sets up the condition to be an instance of ActionCondition and initializes
        /// a WaitActionConditionBuilder to define the specific action to be waited on.
        /// <returns>
        ///     A WaitActionConditionBuilder associated with the current Wait builder.
        /// </returns>
        public virtual WaitActionConditionBuilder Execution()
        {
            return new ActionConditionBuilder<ActionCondition>().Execution();
        }

        /// Starts the configuration of an HTTP wait condition builder. This method initializes an
        /// HTTP-specific condition within the wait action, enabling further customization of HTTP
        /// parameters such as URL, timeout, status, and method for checks involving HTTP requests.
        /// <returns>
        ///     A builder instance specifically for configuring HTTP wait conditions.
        /// </returns>
        public virtual WaitHttpConditionBuilder Http()
        {
            return new HttpConditionBuilder<HttpCondition>().Http();
        }

        /// Configures a wait condition for file operations within the specified test action.
        /// This method creates a new FileCondition and an appropriate builder
        /// for setting up wait conditions related to file interactions.
        /// <returns>
        ///     An instance of WaitFileConditionBuilder configured for file operations,
        ///     enabling further customization of the wait condition such as specifying the file path.
        /// </returns>
        public virtual WaitFileConditionBuilder File()
        {
            return new FileConditionBuilder<FileCondition>().File();
        }

        /// Sets the interval for the wait condition evaluation process.
        /// <param name="newInterval">
        ///     The new interval value, in milliseconds, to set for the condition evaluation.
        ///     This value determines the frequency at which the condition is checked.
        /// </param>
        /// <return>
        ///     Returns the current instance of the builder to support method chaining.
        /// </return>
        public Builder<C> Interval(long newInterval)
        {
            return Interval(newInterval.ToString());
        }

        /// Sets the interval for the wait condition evaluation process.
        /// <param name="newInterval">
        ///     The new interval value, in milliseconds, to set for the condition evaluation.
        ///     This value determines the frequency at which the condition is checked.
        /// </param>
        /// <return>
        ///     Returns the current instance of the builder to support method chaining.
        /// </return>
        public Builder<C> Interval(string newInterval)
        {
            interval = newInterval;
            return this;
        }

        /// Sets the wait duration in milliseconds.
        /// <param name="milliseconds">
        ///     The duration to be set for the wait condition in milliseconds.
        /// </param>
        /// <return>
        ///     Returns the builder instance for method chaining.
        /// </return>
        public Builder<C> Milliseconds(long milliseconds)
        {
            return Milliseconds(milliseconds.ToString());
        }

        /// Sets the maximum wait time in milliseconds for a specified condition within the builder context.
        /// <param name="milliseconds">
        ///     A string representing the number of milliseconds to wait before the condition times out.
        /// </param>
        /// <return>
        ///     The current builder instance with the updated wait time setting.
        /// </return>
        public Builder<C> Milliseconds(string milliseconds)
        {
            time = milliseconds;
            return this;
        }

        /// Sets the interval in seconds, converting to milliseconds before updating the internal timer configuration.
        /// <param name="seconds">
        ///     The time interval in seconds that the waiting process should be set to.
        /// </param>
        /// <return>
        ///     Returns the updated builder instance for fluent interface chaining.
        /// </return>
        public Builder<C> Seconds(double seconds)
        {
            Milliseconds(Convert.ToInt64(seconds * 1000));
            return this;
        }

        /// Sets the maximum duration for which the wait action should continue checking the specified condition.
        /// <param name="duration">
        ///     A TimeSpan representing the maximum allowed time for waiting until the condition is met.
        ///     The duration is converted to milliseconds to configure the wait time limit.
        /// </param>
        /// <return>
        ///     Returns the current builder instance with updated time settings, enabling method chaining of additional builder
        ///     configurations.
        /// </return>
        public Builder<C> Time(TimeSpan duration)
        {
            Milliseconds(Convert.ToInt64(duration.TotalMilliseconds));
            return this;
        }

        private class MessageConditionBuilder<MCB> : Builder<MCB> where MCB : MessageCondition
        {
            public override WaitMessageConditionBuilder Message()
            {
                condition = (MCB)new MessageCondition();
                var builder = new WaitMessageConditionBuilder((Builder<MessageCondition>)(object)this);
                dlg = builder;
                return builder;
            }
        }

        private class ActionConditionBuilder<ACB> : Builder<ACB> where ACB : ActionCondition
        {
            public override WaitActionConditionBuilder Execution()
            {
                condition = (ACB)new ActionCondition();
                var builder = new WaitActionConditionBuilder((Builder<ActionCondition>)(object)this);
                dlg = builder;
                return builder;
            }
        }

        private class HttpConditionBuilder<HCB> : Builder<HCB> where HCB : HttpCondition
        {
            public override WaitHttpConditionBuilder Http()
            {
                condition = (HCB)new HttpCondition();
                var builder = new WaitHttpConditionBuilder((Builder<HttpCondition>)(object)this);
                dlg = builder;
                return builder;
            }
        }

        private class FileConditionBuilder<FCB> : Builder<FCB> where FCB : FileCondition
        {
            public override WaitFileConditionBuilder File()
            {
                condition = (FCB)new FileCondition();
                var builder = new WaitFileConditionBuilder((Builder<FileCondition>)(object)this);
                dlg = builder;
                return builder;
            }
        }
    }
}