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

namespace Agenix.Core.Util;

/// <summary>
///     Provides methods to schedule and execute tasks at specified intervals.
/// </summary>
public class ScheduledExecutor
{
    /// <summary>
    ///     Represents time units for scheduling tasks.
    /// </summary>
    public enum TimeUnit
    {
        MILLISECONDS,
        SECONDS,
        MINUTES
    }

    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly TaskScheduler _taskScheduler = TaskScheduler.Default;

    /// <summary>
    ///     Schedules a task to run at a fixed rate after an initial delay.
    /// </summary>
    /// <param name="action">The action to execute periodically.</param>
    /// <param name="initialDelay">The initial delay before the first execution.</param>
    /// <param name="period">The period between subsequent executions.</param>
    /// <param name="timeUnit">The unit of time for the initial delay and period.</param>
    /// <returns>A Task representing the scheduled action.</returns>
    public Task ScheduleAtFixedRate(Action action, long initialDelay, long period, TimeUnit timeUnit)
    {
        var delay = ConvertToMilliseconds(initialDelay, timeUnit);
        var interval = ConvertToMilliseconds(period, timeUnit);

        return Task.Factory.StartNew(async () =>
        {
            await Task.Delay(delay, _cancellationTokenSource.Token);
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                action();
                await Task.Delay(interval, _cancellationTokenSource.Token);
            }
        }, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, _taskScheduler);
    }

    /// <summary>
    ///     Initiates an orderly shutdown of the executor, stopping the acceptance of new tasks and attempts to cancel
    ///     currently executing tasks.
    /// </summary>
    public void Shutdown()
    {
        _cancellationTokenSource.Cancel();
    }

    /// <summary>
    ///     Waits for the executor to terminate within the specified timeout.
    /// </summary>
    /// <param name="timeout">The maximum time to wait for termination.</param>
    /// <param name="timeUnit">The time unit of the timeout duration.</param>
    public void AwaitTermination(int timeout, TimeUnit timeUnit)
    {
        Thread.Sleep(ConvertToMilliseconds(timeout, timeUnit));
    }

    /// <summary>
    ///     Checks if the executor is terminated, meaning all tasks are cancelled and the executor is shut down.
    /// </summary>
    /// <returns>True if the executor is terminated, otherwise false.</returns>
    public bool IsTerminated()
    {
        // Placeholder for actual implementation
        return _cancellationTokenSource.IsCancellationRequested;
    }

    /// <summary>
    ///     Forces the executor to shut down immediately, cancelling all scheduled and running tasks.
    /// </summary>
    public void ShutdownNow()
    {
        _cancellationTokenSource.Cancel();
    }

    /// <summary>
    ///     Converts a given time duration and its unit to milliseconds.
    /// </summary>
    /// <param name="time">The time duration to convert.</param>
    /// <param name="timeUnit">The unit of the time duration.</param>
    /// <returns>The time duration in milliseconds.</returns>
    private int ConvertToMilliseconds(long time, TimeUnit timeUnit)
    {
        // Convert time unit to milliseconds
        return timeUnit switch
        {
            TimeUnit.MILLISECONDS => (int)time,
            TimeUnit.SECONDS => (int)(time * 1000),
            TimeUnit.MINUTES => (int)(time * 60000),
            _ => throw new ArgumentException("Unsupported TimeUnit")
        };
    }
}
