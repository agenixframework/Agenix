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
using Agenix.Api.Container;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Core.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Container;

/// <summary>
///     Represents a configurable timer that can execute a set of actions periodically using async/await patterns.
///     The Timer class allows setting parameters such as delay, interval, and repeat count through its builder, and
///     provides functionality to stop the timer.
/// </summary>
public class Timer(Timer.Builder builder)
    : AbstractAsyncActionContainer(builder.GetName() ?? "timer", builder.GetDescription(), builder.GetActions()),
        IStopTimer, IDisposable

{
    /// <summary>
    ///     Suffix used to identify index-related variables in the context of a Timer.
    /// </summary>
    public const string IndexSuffix = "-index";

    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(Timer));

    private static readonly AtomicLong NextSerialNumber = new();
    private readonly SemaphoreSlim _completionSemaphore = new(0, 1);

    private CancellationTokenSource _cancellationTokenSource;
    private int _completionSignaled;

    private bool _disposed;

    /// <summary>
    ///     A boolean variable indicating whether the timer process has completed its execution.
    ///     When set to true, it signifies that the timer has either finished its loop based on the specified repeat count
    ///     or has been manually stopped.
    /// </summary>
    protected bool TimerComplete;

    /// <summary>
    ///     Represents an exception occurring during the execution of the Timer.
    ///     This exception is used to capture and store any runtime errors encountered while the Timer performs its operations.
    /// </summary>
    protected AgenixSystemException timerException;

    /// <summary>
    ///     Determines whether the timer should run in a separate task.
    /// </summary>
    public bool Fork { get; } = builder.fork;

    /// <summary>
    ///     Represents the interval in milliseconds for the timer execution cycle.
    /// </summary>
    public long Interval { get; } = builder.interval;

    /// <summary>
    ///     Represents the initial delay before the timer starts executing its actions.
    /// </summary>
    public long Delay { get; } = builder.delay;

    /// <summary>
    ///     Specifies the number of times the timer will repeat its execution cycle.
    /// </summary>
    public int RepeatCount { get; } = builder.repeatCount;

    /// <summary>
    ///     Gets the unique identifier for this timer instance.
    /// </summary>
    public string TimerId { get; } = builder.timerId;

    /// <summary>
    ///     Represents the exception encountered during the execution of the timer.
    ///     Provides details about any issues that occurred while the timer was running.
    /// </summary>
    public AgenixSystemException TimerException => timerException;

    /// <summary>
    ///     Disposes of the timer resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Stops the timer, ensuring it is disposed of properly, and marks the timer as complete.
    /// </summary>
    public Task StopTimer()
    {
        if (_disposed)
        {
            return Task.CompletedTask;
        }

        _cancellationTokenSource?.Cancel();
        TimerComplete = true;
        SignalCompletion();

        return Task.CompletedTask;
    }

    private void SignalCompletion()
    {
        // Ensure semaphore is released only once to avoid SemaphoreFullException
        if (Interlocked.Exchange(ref _completionSignaled, 1) == 0)
        {
            _completionSemaphore.Release();
        }
    }

    /// <summary>
    ///     Executes the timer in the given test context, determining whether to run the timer asynchronously or synchronously
    ///     based on the configuration.
    /// </summary>
    /// <param name="context">The test context within which the timer executes.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    public override async Task DoExecute(TestContext context, CancellationToken cancellationToken = default)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        if (Fork)
        {
            _ = Task.Run(async () => await ConfigureAndRunTimerAsync(context, _cancellationTokenSource.Token),
                _cancellationTokenSource.Token);
        }
        else
        {
            await ConfigureAndRunTimerAsync(context, _cancellationTokenSource.Token);
        }
    }

    /// <summary>
    ///     Configures and runs the timer associated with the given test context using async/await patterns.
    /// </summary>
    /// <param name="context">The test context within which the timer operates.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    private async Task ConfigureAndRunTimerAsync(TestContext context, CancellationToken cancellationToken)
    {
        // Register the timer with the test context using its ID
        context.RegisterTimer(TimerId, this);

        var indexCount = 0;

        try
        {
            // Initial delay
            if (Delay > 0)
            {
                Log.LogDebug("Timer initial delay of {Delay} milliseconds", Delay);
                await Task.Delay((int)Delay, cancellationToken);
            }

            // Timer loop
            while (indexCount < RepeatCount && !TimerComplete && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    indexCount++;
                    UpdateIndexCountInTestContext(context, indexCount);
                    Log.LogDebug("Timer event fired #{IndexCount} - executing nested actions", indexCount);

                    await ExecuteActionsAsync(context, cancellationToken);

                    // Wait for the interval before next execution
                    if (indexCount < RepeatCount && !TimerComplete)
                    {
                        await Task.Delay((int)Interval, cancellationToken);
                    }
                }
                catch (OperationCanceledException ex)
                {
                    Log.LogDebug(ex, "Timer operation was cancelled");
                    break;
                }
                catch (Exception e)
                {
                    HandleException(e, context);
                    break;
                }
            }

            if (indexCount >= RepeatCount)
            {
                Log.LogDebug("Timer complete: {RepeatCount} iterations reached", RepeatCount);
            }
        }
        catch (OperationCanceledException ex)
        {
            Log.LogDebug(ex, "Timer was cancelled");
        }
        finally
        {
            TimerComplete = true;
            SignalCompletion();
        }

        // If not forked, wait for completion and throw exception if needed
        if (!Fork && TimerException != null)
        {
            throw TimerException;
        }
    }

    /// <summary>
    ///     Executes all actions asynchronously.
    /// </summary>
    /// <param name="context">The test context.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    private async Task ExecuteActionsAsync(TestContext context, CancellationToken cancellationToken)
    {
        foreach (var actionBuilder in Actions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ExecuteAction(actionBuilder.Build(), context);
        }
    }

    /// <summary>
    ///     Updates the current index count in the given test context.
    /// </summary>
    /// <param name="context">The test context where the index count should be updated.</param>
    /// <param name="indexCount">The current index count to be updated in the context.</param>
    private void UpdateIndexCountInTestContext(TestContext context, int indexCount)
    {
        context.SetVariable(TimerId + IndexSuffix, indexCount.ToString());
    }

    /// <summary>
    ///     Handles exceptions that occur during the execution of a Timer instance.
    /// </summary>
    /// <param name="e">The exception that was raised during timer execution.</param>
    /// <param name="context">The context of the test where the exception occurred.</param>
    private void HandleException(Exception e, TestContext context)
    {
        if (e is AgenixSystemException coreSystemException)
        {
            timerException = coreSystemException;
        }
        else
        {
            timerException = new AgenixSystemException(e.Message, e);
        }

        Log.LogError("Timer stopped as a result of nested action error ({EMessage})", e.Message);
        StopTimer();

        if (Fork)
        {
            context.AddException(TimerException);
        }
    }

    /// <summary>
    ///     Waits for the timer to complete asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    public async Task WaitForCompletionAsync(CancellationToken cancellationToken = default)
    {
        if (!TimerComplete)
        {
            await _completionSemaphore.WaitAsync(cancellationToken);
        }
    }

    /// <summary>
    ///     Disposes of the timer resources.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            StopTimer();
            _cancellationTokenSource?.Dispose();
            _completionSemaphore?.Dispose();
            _disposed = true;
        }
    }

    /// <summary>
    ///     Builder class for constructing Timer objects with customizable properties such as delay, interval, and repeat
    ///     count.
    /// </summary>
    public class Builder : AbstractTestContainerBuilder<Timer, Builder>
    {
        internal int delay;
        internal bool fork;
        internal long interval = 1000L;
        internal int repeatCount = int.MaxValue;
        internal string timerId;

        /// <summary>
        ///     Fluent API entry point for creating and configuring Timer objects in a C# DSL.
        /// </summary>
        /// <returns>A new instance of Timer.Builder for chaining configuration methods.</returns>
        public static Builder Timer()
        {
            return new Builder();
        }

        /// <summary>
        ///     Sets the initial delay in milliseconds before the first timer event should fire.
        /// </summary>
        /// <param name="newDelay">The delay in milliseconds.</param>
        /// <returns>The current instance of Builder for chaining configuration methods.</returns>
        public Builder Delay(int newDelay)
        {
            delay = newDelay;
            return this;
        }

        /// <summary>
        ///     Sets the interval in milliseconds between each timer event. Once the interval has elapsed, the next timer event is
        ///     fired.
        /// </summary>
        /// <param name="newInterval">The interval in milliseconds between each timer event.</param>
        /// <returns>The current instance of Builder for chaining configuration methods.</returns>
        public Builder Interval(long newInterval)
        {
            interval = newInterval;
            return this;
        }

        /// <summary>
        ///     Sets the maximum number of times the timer event will be fired before the timer is stopped.
        /// </summary>
        /// <param name="newRepeatCount">The maximum number of times the timer should fire.</param>
        /// <returns>The current instance of Builder for chaining configuration methods.</returns>
        public Builder RepeatCount(int newRepeatCount)
        {
            repeatCount = newRepeatCount;
            return this;
        }

        /// <summary>
        ///     Configures the timer to allow it to run in parallel with other actions.
        /// </summary>
        /// <param name="newFork">A boolean value indicating whether the timer should be forked.</param>
        /// <returns>The current instance of Builder for chaining configuration methods.</returns>
        public Builder Fork(bool newFork)
        {
            fork = newFork;
            return this;
        }

        /// <summary>
        ///     Sets the timer's identifier, allowing it to be referenced by other test actions such as stop-timer.
        /// </summary>
        /// <param name="newTimerId">A unique identifier for the timer within the test context.</param>
        /// <returns>The current instance of Builder for chaining configuration methods.</returns>
        public Builder Id(string newTimerId)
        {
            return TimerId(newTimerId);
        }

        /// <summary>
        ///     Sets the timer's unique identifier, enabling reference from other test actions like stop-timer.
        /// </summary>
        /// <param name="newTimerId">A unique identifier for the timer within the test context.</param>
        /// <returns>The current instance of Builder for chaining configuration methods.</returns>
        public Builder TimerId(string newTimerId)
        {
            timerId = newTimerId;
            return this;
        }

        /// <summary>
        ///     Generates and returns a unique serial number as an integer.
        /// </summary>
        /// <returns>A unique serial number as an integer.</returns>
        private static int SerialNumber()
        {
            return (int)NextSerialNumber.IncrementAndGet();
        }

        /// <summary>
        ///     Constructs and returns a new Timer instance using the current builder configuration.
        ///     Ensures all necessary properties are set and initializes the timer with a unique identifier if not provided.
        /// </summary>
        /// <returns>A new instance of the Timer class configured with the current builder instance.</returns>
        protected override Timer DoBuild()
        {
            if (string.IsNullOrEmpty(timerId))
            {
                timerId = "agenix-timer-" + SerialNumber();
            }

            return new Timer(this);
        }
    }
}
