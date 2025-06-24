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
using System.Globalization;
using System.Threading;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Core.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Actions;

/// <summary>
///     Stop the test execution for a given amount of time.
/// </summary>
/// <param name="builder"></param>
public class SleepAction(SleepAction.Builder builder) : AbstractTestAction("sleep", builder)
{
    /// Logger for SleepAction.
    /// /
    private static readonly ILogger Log = LogManager.GetLogger(typeof(SleepAction));

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
            TimeSpan parsedDuration;
            if (duration.IndexOf('.', StringComparison.Ordinal) > 0)
            {
                parsedDuration = TimeUnit switch
                {
                    ScheduledExecutor.TimeUnit.MILLISECONDS => TimeSpan.FromMilliseconds(Math.Round(double.Parse(
                        duration, CultureInfo.InvariantCulture
                    ))),
                    ScheduledExecutor.TimeUnit.SECONDS => TimeSpan.FromSeconds(
                        Math.Round(double.Parse(duration, CultureInfo.InvariantCulture))),
                    ScheduledExecutor.TimeUnit.MINUTES => TimeSpan.FromMinutes(
                        Math.Round(double.Parse(duration, CultureInfo.InvariantCulture))),
                    _ => throw new AgenixSystemException(
                        "Unsupported time expression for sleep action - please use one of milliseconds, seconds, minutes")
                };
            }
            else
            {
                parsedDuration = TimeSpan.FromMilliseconds(
                    ConvertToMilliseconds(long.Parse(duration), TimeUnit));
            }

            Log.LogInformation($"Sleeping {duration} {TimeUnit}");

            Thread.Sleep(parsedDuration);

            Log.LogInformation($"Returning after {duration} {TimeUnit}");
        }
        catch (ThreadInterruptedException e)
        {
            throw new AgenixSystemException(e.Message);
        }
    }

    private static long ConvertToMilliseconds(long value, ScheduledExecutor.TimeUnit sourceUnit)
    {
        return sourceUnit switch
        {
            ScheduledExecutor.TimeUnit.MILLISECONDS => value,
            ScheduledExecutor.TimeUnit.SECONDS => value * 1000,
            ScheduledExecutor.TimeUnit.MINUTES => value * 60 * 1000,
            _ => throw new AgenixSystemException("Unsupported time unit")
        };
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
