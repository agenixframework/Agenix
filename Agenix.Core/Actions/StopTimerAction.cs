#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
// 
// Copyright (c) 2025 Agenix
// 
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

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
