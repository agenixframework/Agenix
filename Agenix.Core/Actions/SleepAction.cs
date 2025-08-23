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
using System.Threading.Tasks;
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
public class SleepAction(SleepAction.Builder builder) : AbstractTestActionAsync("sleep", builder)
{
    /// Logger for SleepAction.
    /// /
    private static readonly ILogger Log = LogManager.GetLogger(typeof(SleepAction));

    /// <summary>
    ///     Represents the duration for which the sleep action will pause execution.
    /// </summary>
    public string Time { get; } = builder.Time;

    /// <summary>
    ///     Represents the unit of time associated with the duration specified for the sleep action.
    /// </summary>
    public ScheduledExecutor.TimeUnit TimeUnit { get; } = builder.TimeUnit;

    /// <summary>
    ///     Executes the sleep action, causing the current thread to pause execution for the specified duration.
    /// </summary>
    /// <param name="context">The test context that provides the dynamic resolution of the sleep duration value.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token to observe while waiting, which can be used to cancel the delay
    ///     operation.
    /// </param>
    /// <returns>A task that represents the asynchronous delay operation.</returns>
    public override async Task DoExecute(TestContext context, CancellationToken cancellationToken = default)
    {
        var duration = context.ResolveDynamicValue(Time);

        try
        {
            TimeSpan parsedDuration;
            if (duration.Contains('.'))
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

            Log.LogInformation("Sleeping {Duration} {TimeUnit}", duration, TimeUnit);

            // Use Task.Delay instead of Thread.Sleep for async methods
            await Task.Delay(parsedDuration, cancellationToken);

            Log.LogInformation("Returning after {Duration} {TimeUnit}", duration, TimeUnit);
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation properly
            throw;
        }
        catch (Exception e)
        {
            throw new AgenixSystemException(e.Message, e);
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


    /// <summary>
    ///     Represents a builder for constructing asynchronous test actions related to sleep behavior,
    ///     supporting configuration of time duration and units for delaying test execution.
    /// </summary>
    public sealed class Builder : AbstractAsyncTestActionBuilder<IAsyncTestAction, dynamic>
    {
        internal string Time = "5000";
        internal ScheduledExecutor.TimeUnit TimeUnit = ScheduledExecutor.TimeUnit.MILLISECONDS;

        /// <summary>
        ///     Configures the delay builder, allowing customization of the duration
        ///     and time unit for a sleep action during test execution.
        /// </summary>
        /// <returns>
        ///     A new instance of the sleep action builder for defining the delay settings.
        /// </returns>
        public static Builder Delay()
        {
            return new Builder();
        }

        /// <summary>
        ///     Specifies an action that pauses the execution of the current test thread for a defined duration.
        /// </summary>
        /// <returns>A builder instance with configurable methods to specify the sleep duration and time unit.</returns>
        public static Builder Sleep()
        {
            return new Builder();
        }

        /// <summary>
        ///     Configures the sleep action to pause execution for the specified amount of time in milliseconds.
        /// </summary>
        /// <param name="milliseconds">The duration of the delay, in milliseconds.</param>
        /// <returns>An updated instance of the builder with the specified delay configuration.</returns>
        public Builder Milliseconds(int milliseconds)
        {
            return WithTime(milliseconds.ToString(), ScheduledExecutor.TimeUnit.MILLISECONDS);
        }

        /// <summary>
        ///     Configures the sleep duration in milliseconds for the action.
        /// </summary>
        /// <param name="milliseconds">The duration for which the action will sleep, specified in milliseconds.</param>
        /// <returns>A builder instance configured with the specified sleep duration in milliseconds.</returns>
        public Builder Milliseconds(long milliseconds)
        {
            return WithTime(milliseconds.ToString(), ScheduledExecutor.TimeUnit.MILLISECONDS);
        }

        /// <summary>
        ///     Configures the sleep duration in milliseconds using a string expression.
        /// </summary>
        /// <param name="expression">The string expression representing the duration in milliseconds.</param>
        /// <returns>Returns the current builder instance with the updated configuration.</returns>
        public Builder Milliseconds(string expression)
        {
            WithTime(expression, ScheduledExecutor.TimeUnit.MILLISECONDS);
            return this;
        }

        /// <summary>
        ///     Configures the sleep action with the specified duration in seconds.
        /// </summary>
        /// <param name="seconds">The duration, in seconds, for which the sleep action will delay execution.</param>
        /// <returns>
        ///     An instance of the builder configured with the specified duration.
        /// </returns>
        public Builder Seconds(double seconds)
        {
            Milliseconds((int)Math.Round(seconds * 1000));
            return this;
        }

        /// <summary>
        ///     Sets the duration of the sleep action in seconds.
        /// </summary>
        /// <param name="seconds">The duration of the sleep in seconds.</param>
        /// <returns>Returns the builder instance with the configured sleep duration in seconds.</returns>
        public Builder Seconds(int seconds)
        {
            return WithTime((seconds * 1000L).ToString(), ScheduledExecutor.TimeUnit.MILLISECONDS);
        }

        /// <summary>
        ///     Sets the duration for the sleep action in seconds.
        /// </summary>
        /// <param name="seconds">The duration, in seconds, to execute the sleep action.</param>
        /// <returns>Returns the builder instance with the updated sleep duration in milliseconds.</returns>
        public Builder Seconds(long seconds)
        {
            return WithTime((seconds * 1000L).ToString(), ScheduledExecutor.TimeUnit.MILLISECONDS);
        }

        /// <summary>
        ///     Configures the delay duration based on the provided timespan.
        /// </summary>
        /// <param name="duration">The duration of the delay as a <see cref="TimeSpan" />.</param>
        /// <returns>A builder instance for further customization of the sleep action.</returns>
        public Builder WithTime(TimeSpan duration)
        {
            Milliseconds(duration.Milliseconds);
            return this;
        }

        /// <summary>
        ///     Sets the execution time and the unit of time for the sleep action.
        /// </summary>
        /// <param name="expression">A string representation of the time duration for the sleep action.</param>
        /// <param name="newTimeUnit">The unit of time (e.g., MILLISECONDS, SECONDS) for the specified duration.</param>
        /// <returns>Returns the builder instance with the updated time and time unit configuration.</returns>
        public Builder WithTime(string expression, ScheduledExecutor.TimeUnit newTimeUnit)
        {
            Time = expression;
            TimeUnit = newTimeUnit;
            return this;
        }

        /// <summary>
        ///     Constructs and returns a new instance of the SleepAction class using its associated builder.
        /// </summary>
        /// <returns>A fully constructed instance of the SleepAction class.</returns>
        public override SleepAction Build()
        {
            return new SleepAction(this);
        }
    }
}
