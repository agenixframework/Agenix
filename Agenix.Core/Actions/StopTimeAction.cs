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
public class StopTimeAction(StopTimeAction.Builder builder) : AbstractTestAction("stop-time", builder)
{
    public const string DefaultTimeLineId = "AGENIX_TIMELINE";
    public const string DefaultTimeLineValueSuffix = "_VALUE";

    private static readonly ILogger Log = LogManager.GetLogger(typeof(StopTimerAction));
    private readonly string _id = builder._id;
    private readonly string _suffix = builder._suffix;

    /// <summary>
    ///     Executes the stop-time action within the given test context.
    ///     This includes starting a time watcher if not already present,
    ///     or recording the elapsed time since the watcher was started.
    /// </summary>
    /// <param name="context">The test context within which the action is executed.</param>
    public override void DoExecute(TestContext context)
    {
        var timeLineId = context.ReplaceDynamicContentInString(_id);
        var timeLineSuffix = context.ReplaceDynamicContentInString(_suffix);

        try
        {
            if (context.GetVariables().ContainsKey(timeLineId))
            {
                var time = DateTime.Now.Ticks - context.GetVariable<long>(timeLineId);
                context.SetVariable(timeLineId + timeLineSuffix, time);

                Log.LogInformation(description != null
                    ? $"TimeWatcher {timeLineId} after {time} ms ({description})"
                    : $"TimeWatcher {timeLineId} after {time} ms");
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
    }

    /// <summary>
    ///     Retrieves the ID associated with the StopTimeAction.
    /// </summary>
    /// <returns>The ID string.</returns>
    public string GetId()
    {
        return _id;
    }

    /// <summary>
    ///     Retrieves the suffix associated with the StopTimeAction.
    /// </summary>
    /// <returns>The suffix string.</returns>
    public string GetSuffix()
    {
        return _suffix;
    }

    /// <summary>
    ///     Builder class for configuring and creating instances of StopTimeAction.
    /// </summary>
    public sealed class Builder : AbstractTestActionBuilder<ITestAction, dynamic>
    {
        public string _id = DefaultTimeLineId;
        public string _suffix = DefaultTimeLineValueSuffix;

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
        /// <param name="context">The context in which the test action is executed.</param>
        public static Builder StopTime(string id)
        {
            var builder = new Builder { _id = id };
            return builder;
        }

        /// <summary>
        ///     Stops the time for a specified timeline as part of a test action.
        /// </summary>
        /// <param name="context">The context in which the test action is executed.</param>
        public static Builder StopTime(string id, string suffix)
        {
            var builder = new Builder { _id = id, _suffix = suffix };
            return builder;
        }

        /// <summary>
        ///     Sets the ID to be used in the StopTimeAction.
        /// </summary>
        /// <param name="id">The ID to be set for the StopTimeAction.</param>
        /// <returns>The current instance of the Builder, enabling method chaining.</returns>
        public Builder Id(string id)
        {
            _id = id;
            return this;
        }

        /// <summary>
        ///     Sets the suffix to be used in the StopTimeAction.
        /// </summary>
        /// <param name="timeSuffix">The suffix to be set for the StopTimeAction.</param>
        /// <returns>The current instance of the Builder, enabling method chaining.</returns>
        public Builder Suffix(string timeSuffix)
        {
            _suffix = timeSuffix;
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
