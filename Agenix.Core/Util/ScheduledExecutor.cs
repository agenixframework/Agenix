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
