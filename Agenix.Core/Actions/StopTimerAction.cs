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

using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Actions;

/// The StopTimerAction class is responsible for executing the logic to stop timers,
/// either a specific timer or all timers within a given context.
public class StopTimerAction(StopTimerAction.Builder builder) : AbstractTestAction("stop-timer", builder)
{
    private static readonly ILogger Log = LogManager.GetLogger(typeof(StopTimerAction));

    public string TimerId { get; } = builder.TimerId;

    /// Executes the logic to stop a single timer if the timer ID is specified,
    /// otherwise stops all timers if no timer ID is provided.
    /// <param name="context">The context within which the timer(s) are managed.</param>
    public override void DoExecute(TestContext context)
    {
        if (TimerId != null)
        {
            var success = context.StopTimer(TimerId);
            Log.LogInformation("Stopping timer {S} - stop successful: {Success}", TimerId, success);
        }
        else
        {
            context.StopTimers();
            Log.LogInformation("Stopping all timers");
        }
    }

    /// The Builder class is used to build the StopTimerAction instances, which are actions
    /// that manage the stopping of timers in a given context.
    public class Builder : AbstractTestActionBuilder<ITestAction, dynamic>
    {
        public string TimerId { get; private set; }

        /// Stops a timer specified by the given ID within the provided context.
        /// If no ID is provided, all timers in the context are stopped.
        public static Builder StopTimer()
        {
            return new Builder();
        }

        /// Stops a timer specified by the given ID within the provided context.
        /// If no ID is provided, all timers in the context are stopped.
        /// <param name="context">The context within which the timer(s) are managed.</param>
        public static Builder StopTimer(string timerId)
        {
            var builder = new Builder();
            builder.Id(timerId);
            return builder;
        }

        /// Sets the ID of the timer to be stopped by the StopTimerAction.
        /// <param name="timerId">The ID of the timer to be stopped.</param>
        /// <return>Returns the current Builder instance, allowing for method chaining.</return>
        public Builder Id(string timerId)
        {
            TimerId = timerId;
            return this;
        }

        /// Builds and returns an instance of the StopTimerAction.
        /// The StopTimerAction is used to manage the stopping of timers within a test context.
        /// <return>An instance of StopTimerAction built using the current state of the Builder.</return>
        public override StopTimerAction Build()
        {
            return new StopTimerAction(this);
        }
    }
}
