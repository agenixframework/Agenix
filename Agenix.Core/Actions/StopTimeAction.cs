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

using System;
using System.Threading;
using System.Threading.Tasks;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Actions;

/// <summary>
///     Action used for time measurement during test. User can define a timeline that is followed during the test case.
///     Action can print out the watched time to the console/ logger.
/// </summary>
public class StopTimeAction(StopTimeAction.Builder builder) : AbstractTestActionAsync("stop-time", builder)
{
    /// <summary>
    ///     Represents the default identifier for a timeline in the StopTimeAction class.
    ///     This identifier is used to track and manage time measurement timelines during test cases.
    /// </summary>
    public const string DefaultTimeLineId = "AGENIX_TIMELINE";

    /// <summary>
    ///     Represents the suffix appended to the default timeline identifier to create a unique key
    ///     for storing timeline-related values. This suffix is used in conjunction with
    ///     the <c>DefaultTimeLineId</c> to manage timeline value information during test executions.
    /// </summary>
    public const string DefaultTimeLineValueSuffix = "_VALUE";

    private static readonly ILogger Log = LogManager.GetLogger(typeof(StopTimerAction));

    /// <summary>
    ///     Retrieves the ID associated with the StopTimeAction.
    /// </summary>
    /// <returns>The ID string.</returns>
    public string Id { get; } = builder.Id;

    /// <summary>
    ///     Retrieves the suffix associated with the StopTimeAction.
    /// </summary>
    /// <returns>The suffix string.</returns>
    public string Suffix { get; } = builder.Suffix;

    /// <summary>
    ///     Executes the stop-time action within the specified test context.
    ///     This involves either starting a time watcher if it is not already active,
    ///     or recording the elapsed time since the watcher was initiated.
    /// </summary>
    /// <param name="context">The test context in which the action is executed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override Task DoExecute(TestContext context, CancellationToken cancellationToken = default)
    {
        var timeLineId = context.ReplaceDynamicContentInString(Id);
        var timeLineSuffix = context.ReplaceDynamicContentInString(Suffix);

        try
        {
            if (context.GetVariables().ContainsKey(timeLineId))
            {
                var time = DateTime.Now.Ticks - context.GetVariable<long>(timeLineId);
                context.SetVariable(timeLineId + timeLineSuffix, time);

                var infoMessage = Description != null
                    ? $"TimeWatcher {timeLineId} after {time} ms ({Description})"
                    : $"TimeWatcher {timeLineId} after {time} ms";
                Log.LogInformation(infoMessage);
            }
            else
            {
                Log.LogInformation("Starting TimeWatcher: {TimeLineId}", timeLineId);
                context.SetVariable(timeLineId, DateTime.Now.Ticks);
                context.SetVariable(timeLineId + timeLineSuffix, 0L);
            }
        }
        catch (Exception e)
        {
            throw new AgenixSystemException(e.Message);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Builder class for configuring and creating instances of StopTimeAction.
    /// </summary>
    public sealed class Builder : AbstractAsyncTestActionBuilder<IAsyncTestAction, dynamic>
    {
        /// <summary>
        ///     Represents the unique identifier associated with the StopTimeAction instance.
        /// </summary>
        /// <remarks>
        ///     This identifier is used internally to define or modify the context or timeline
        ///     for which the StopTimeAction is applied. It may be explicitly defined during
        ///     the instantiation of the builder or defaults to a predefined value.
        /// </remarks>
        internal string Id = DefaultTimeLineId;

        /// <summary>
        ///     Represents the suffix value used as part of the identifier or context
        ///     within the StopTimeAction instance.
        /// </summary>
        /// <remarks>
        ///     This value is utilized to form or refine the timeline or context
        ///     for the StopTimeAction. It may be provided explicitly during the
        ///     construction of the builder or defaults to a predefined value.
        /// </remarks>
        internal string Suffix = DefaultTimeLineValueSuffix;

        /// <summary>
        ///     Stops the time for a specified timeline.
        /// </summary>
        /// <param name="context">The context in which the test action is executed.</param>
        /// <returns>A builder object that facilitates the configuration of the StopTime action.</returns>
        public static Builder StopTime()
        {
            return new Builder();
        }

        /// <summary>
        ///     Stops the time for a specified timeline as part of a test action.
        /// </summary>
        /// <param name="context">The context in which this test action is executed.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous stop-time operation.</returns>
        public static Builder StopTime(string newId)
        {
            var builder = new Builder { Id = newId };
            return builder;
        }

        /// <summary>
        ///     Stops the time tracking for a specified timeline or context.
        ///     This action is typically used to halt an active timer or capture the duration
        ///     of a specific operation in a testing scenario.
        /// </summary>
        /// <param name="context">The context in which the stop-time action is performed.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests during the operation.</param>
        /// <returns>A task that represents the asynchronous completion of the action.</returns>
        public static Builder StopTime(string newId, string newSuffix)
        {
            var builder = new Builder { Id = newId, Suffix = newSuffix };
            return builder;
        }

        /// <summary>
        ///     Sets the ID for the current builder instance associated with the StopTimeAction.
        /// </summary>
        /// <param name="newId">The ID to set for the StopTimeAction.</param>
        /// <returns>The current instance of the Builder, allowing method chaining.</returns>
        public Builder WithId(string newId)
        {
            Id = newId;
            return this;
        }

        /// <summary>
        ///     Sets the suffix to be used in the StopTimeAction.
        /// </summary>
        /// <param name="timeSuffix">The suffix to be set for the StopTimeAction.</param>
        /// <returns>The current instance of the Builder, enabling method chaining.</returns>
        public Builder WithSuffix(string timeSuffix)
        {
            Suffix = timeSuffix;
            return this;
        }

        /// <summary>
        ///     Constructs and returns an instance of StopTimeAction configured with the specified parameters.
        /// </summary>
        /// <returns>A StopTimeAction instance.</returns>
        public override StopTimeAction Build()
        {
            return new StopTimeAction(this);
        }
    }
}
