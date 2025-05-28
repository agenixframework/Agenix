#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System.Threading;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Container;

/// Represents a looping test container that continues to iterate over nested test actions
/// if an error occurs during execution. The iteration persists until a specified aborting
/// condition evaluates to true.
/// Each iteration increments an index variable which can be accessed by the nested test actions
/// as a standard test variable. This index helps in tracking the number of iterations performed.
/// The container allows for an optional sleep interval between iterations, which can be configured
/// to pause the process for a specified amount of time.
/// /
public class RepeatOnErrorUntilTrue(RepeatOnErrorUntilTrue.Builder builder) : AbstractIteratingActionContainer(
    builder.GetName() ?? "repeat-on-error",
    builder.GetDescription(), builder.GetActions(),
    builder.GetCondition(), builder.GetConditionExpression(), builder.GetIndexName(), builder.GetIndex(),
    builder.GetStart())
{
    /// Static logger instance for the RepeatOnErrorUntilTrue class.
    /// /
    private static readonly ILogger Log = LogManager.GetLogger(typeof(RepeatOnErrorUntilTrue));

    /// Specifies the auto sleep duration in milliseconds to wait between iterations.
    private readonly int _autoSleep = builder.AutoSlp;

    /// Specifies the auto sleep duration in milliseconds to wait between iterations.
    public int AutoSleep => _autoSleep;

    /// Executes the current iteration of actions within the context provided.
    /// The method will repeatedly attempt to execute its defined set of actions until a specified condition is met,
    /// or an exception is encountered. If an exception of type CoreSystemException is thrown during the execution
    /// of actions, the exception is logged, and the iteration is retried after a sleep delay.
    /// The retry process continues until the condition is satisfied. If all retries fail, the last encountered
    /// exception has been thrown.
    /// <param name="context">The execution context in which actions are performed and conditions are evaluated.</param>
    protected override void ExecuteIteration(TestContext context)
    {
        AgenixSystemException exception = null;

        while (!CheckCondition(context))
            try
            {
                exception = null;
                ExecuteActions(context);
                break;
            }
            catch (AgenixSystemException e)
            {
                exception = e;

                Log.LogInformation(
                    $"Caught exception of type {e.GetType().Name} '{e.Message}' - performing retry #{index}");

                DoAutoSleep();
                index++;
            }

        if (exception == null) return;
        Log.LogInformation($"All retries failed - raising exception {exception.GetType().Name}");
        throw exception;
    }

    /// Pauses the execution for a duration specified by the auto sleep configuration.
    /// Logs the duration of the sleep before pausing and after resuming.
    /// If the auto sleep duration is non-positive, no sleep action is performed.
    /// Handles and logs any ThreadInterruptedException that occurs during the sleep.
    /// /
    private void DoAutoSleep()
    {
        if (_autoSleep <= 0) return;
        Log.LogInformation($"Sleeping {_autoSleep} milliseconds");

        try
        {
            Thread.Sleep(_autoSleep);
        }
        catch (ThreadInterruptedException e)
        {
            Log.LogError(e, "Error during doc generation");
        }

        Log.LogInformation("Returning after {I} milliseconds", _autoSleep);
    }

    /// Builder class for constructing instances of RepeatOnErrorUntilTrue.
    /// /
    public class Builder : AbstractIteratingContainerBuilder<RepeatOnErrorUntilTrue, Builder>
    {
        public int AutoSlp { get; private set; } = 1000;

        /// Fluent API action building entry method used in C# DSL.
        /// @return
        /// /
        public static Builder RepeatOnError()
        {
            return new Builder();
        }

        /// Adds a condition expression to this iterate container.
        /// <param name="cnd">The condition expression that determines when to stop iterating.</param>
        /// <return>The builder instance allowing for fluent method chaining.</return>
        public Builder Until(string cnd)
        {
            Condition(cnd);
            return this;
        }

        /// Adds a condition expression to this iterate container.
        /// @param condition The condition expression that determines when to stop iterating.
        /// @return The builder instance allowing for fluent method chaining.
        /// /
        public Builder Until(IterationEvaluator.IteratingConditionExpression expression)
        {
            Condition(expression);
            return this;
        }

        /// Sets the auto sleep time in between repeats in milliseconds.
        /// @param autoSleepInMillis The duration in milliseconds to sleep between execution retries.
        /// @return The updated Builder instance for method chaining.
        /// /
        public Builder AutoSleep(int autoSleepInMillis)
        {
            AutoSlp = autoSleepInMillis;
            return this;
        }

        protected override RepeatOnErrorUntilTrue DoBuild()
        {
            return new RepeatOnErrorUntilTrue(this);
        }
    }
}
