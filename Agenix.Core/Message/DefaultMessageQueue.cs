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

using System.Collections.Concurrent;
using System.Threading.Tasks;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Message;

/// <summary>
///     Represents the default implementation of a message queue. Provides functionality
///     to send, receive, purge messages, and manage queue state. Implements the
///     <see cref="IMessageQueue" /> interface.
/// </summary>
public class DefaultMessageQueue(string name) : IMessageQueue
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(DefaultMessageQueue));

    private static readonly ILogger RetryLog = LogManager.GetLogger("agenix.retry");

    /// <summary>
    ///     Blocking in memory message store.
    /// </summary>
    private readonly BlockingCollection<IMessage> _queue = new();

    /// <summary>
    ///     Flag to enable or disable message logging.
    /// </summary>
    private bool _loggingEnabled;

    /// <summary>
    ///     Polling interval when waiting for a synchronous reply message to arrive.
    /// </summary>
    private long _pollingInterval = 500;

    /// Sends a new message to the queue.
    /// @param message the message to be sent to the queue.
    public Task Send(IMessage message)
    {
        _queue.Add(message);
        return Task.CompletedTask;
    }

    /// Receives a message from the queue that satisfies the given selector.
    /// @param selector the criteria used to select the message.
    /// @return the selected message that meets the criteria, or null if no message matches.
    public Task<IMessage> Receive(MessageSelector selector)
    {
        var array = _queue.ToArray();
        foreach (var o in array)
        {
            var message = o;
            if (selector.Invoke(message) && _queue.TryTake(out message))
            {
                return Task.FromResult(message);
            }
        }

        return Task.FromResult<IMessage>(null);
    }

    /// Receives a message from the queue that matches the given selector within the specified timeout period.
    /// @param selector the selector to filter messages in the queue.
    /// @param timeout the time in milliseconds to wait for a matching message before giving up.
    /// @return the message that matches the selector, or null if no matching message is found within the timeout period.
    public async Task<IMessage> Receive(MessageSelector selector, long timeout)
    {
        var timeLeft = timeout;
        var message = await Receive(selector);

        while (message == null && timeLeft > 0)
        {
            timeLeft -= _pollingInterval;

            if (RetryLog.IsEnabled(LogLevel.Debug))
            {
                RetryLog.LogDebug("No message received with message selector - retrying in {TimeLeft} ms",
                    timeLeft > 0 ? _pollingInterval : _pollingInterval + timeLeft);
            }

            try
            {
                await Task.Delay((int)(timeLeft > 0 ? _pollingInterval : _pollingInterval + timeLeft));
            }
            catch (TaskCanceledException e)
            {
                RetryLog.LogWarning(e, "Task was canceled while waiting for retry");
            }

            message = await Receive(selector);
        }

        return message;
    }

    /// Removes messages from the queue that match the given selector criteria.
    /// @param selector the criteria used to purge messages from the queue.
    public Task Purge(IMessageSelector selector)
    {
        var array = _queue.ToArray();
        foreach (var o in array)
        {
            var message = o;
            if (!selector.Accept(message))
            {
                continue;
            }

            if (_queue.TryTake(out message))
            {
                if (Log.IsEnabled(LogLevel.Debug))
                {
                    Log.LogDebug("Purged message '{MessageId}' from in memory queue", message.Id);
                }
            }
            else
            {
                Log.LogWarning("Failed to purge message '{MessageId}' from in memory queue", message?.Id);
            }
        }

        return Task.CompletedTask;
    }

    /// Gets the polling interval.
    /// @return the polling interval.
    /// /
    public long GetPollingInterval()
    {
        return _pollingInterval;
    }

    /// Sets the pollingInterval.
    /// @param pollingInterval the pollingInterval to set
    /// /
    public void SetPollingInterval(long pollingInterval)
    {
        _pollingInterval = pollingInterval;
    }

    /// Determines if logging is enabled.
    /// @return true if logging is enabled, otherwise false.
    /// /
    public bool IsLoggingEnabled()
    {
        return _loggingEnabled;
    }

    /// Sets the logging enabled flag.
    /// @param loggingEnabled flag to enable or disable logging
    /// /
    public void SetLoggingEnabled(bool loggingEnabled)
    {
        _loggingEnabled = loggingEnabled;
    }

    /// Returns a string that represents the current instance.
    /// @return the name of the message queue.
    public override string ToString()
    {
        return name;
    }
}
