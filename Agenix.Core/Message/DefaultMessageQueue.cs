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

using System.Collections.Concurrent;
using System.Threading;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Message;

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
    public void Send(IMessage message)
    {
        _queue.Add(message);
    }

    /// Receives a message from the queue that satisfies the given selector.
    /// @param selector the criteria used to select the message.
    /// @return the selected message that meets the criteria, or null if no message matches.
    public IMessage Receive(MessageSelector selector)
    {
        var array = _queue.ToArray();
        foreach (var o in array)
        {
            var message = o;
            if (selector.Invoke(message) && _queue.TryTake(out message)) return message;
        }

        return null;
    }

    /// Receives a message from the queue that matches the given selector within the specified timeout period.
    /// @param selector the selector to filter messages in the queue.
    /// @param timeout the time in milliseconds to wait for a matching message before giving up.
    /// @return the message that matches the selector, or null if no matching message is found within the timeout period.
    public IMessage Receive(MessageSelector selector, long timeout)
    {
        var timeLeft = timeout;
        var message = Receive(selector);

        while (message == null && timeLeft > 0)
        {
            timeLeft -= _pollingInterval;

            if (RetryLog.IsEnabled(LogLevel.Debug))
                RetryLog.LogDebug("No message received with message selector - retrying in " +
                                  (timeLeft > 0 ? _pollingInterval : _pollingInterval + timeLeft) + "ms");

            try
            {
                Thread.Sleep((int)(timeLeft > 0 ? _pollingInterval : _pollingInterval + timeLeft));
            }
            catch (ThreadInterruptedException e)
            {
                RetryLog.LogWarning(e, "Thread interrupted while waiting for retry");
            }

            message = Receive(selector);
        }

        return message;
    }

    /// Removes messages from the queue that match the given selector criteria.
    /// @param selector the criteria used to purge messages from the queue.
    public void Purge(IMessageSelector selector)
    {
        var array = _queue.ToArray();
        foreach (var o in array)
        {
            var message = o;
            if (!selector.Accept(message)) continue;
            if (_queue.TryTake(out message))
            {
                if (Log.IsEnabled(LogLevel.Debug)) Log.LogDebug($"Purged message '{message.Id}' from in memory queue");
            }
            else
            {
                Log.LogWarning($"Failed to purge message '{message.Id}' from in memory queue");
            }
        }
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

    public override string ToString()
    {
        return name;
    }
}
