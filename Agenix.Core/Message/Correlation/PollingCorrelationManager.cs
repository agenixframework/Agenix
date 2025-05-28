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

using System.Threading;
using Agenix.Api.Context;
using Agenix.Api.Endpoint;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Message.Correlation;

/// <summary>
///     Extension of the default correlation manager adds a polling mechanism for find operation on the object store. In
///     case object
///     is not found in store, retry is automatically performed. Polling interval and overall retry timeout is usually
///     defined in endpoint configuration.
/// </summary>
/// <typeparam name="T"></typeparam>
public class PollingCorrelationManager<T> : DefaultCorrelationManager<T>
{
    private static readonly ILogger Log = LogManager.GetLogger("PollingCorrelationManager");

    private static readonly ILogger RetryLog = LogManager.GetLogger("agenix.RetryLogger");
    private readonly IPollableEndpointConfiguration _endpointConfiguration;
    private readonly string _retryLogMessage;

    /// <summary>
    ///     Constructor using fields.
    /// </summary>
    /// <param name="endpointConfiguration">The endpoint configuration.</param>
    /// <param name="retryLogMessage">The retry log message.</param>
    public PollingCorrelationManager(IPollableEndpointConfiguration endpointConfiguration, string retryLogMessage)
    {
        _retryLogMessage = retryLogMessage;
        _endpointConfiguration = endpointConfiguration;
    }

    /// <summary>
    ///     Convenience method for using default timeout settings of endpoint configuration.
    /// </summary>
    /// <param name="correlationKey">The correlation key.</param>
    /// <returns>The correlated object.</returns>
    public T Find(string correlationKey)
    {
        return Find(correlationKey, _endpointConfiguration.Timeout);
    }

    /// <summary>
    ///     Gets the correlation key for the given identifier.
    ///     Consults the test context with variables for retrieving the stored correlation key.
    /// </summary>
    /// <param name="correlationKeyName">The correlation key name.</param>
    /// <param name="context">The test context.</param>
    /// <returns>The correlation key.</returns>
    public override string GetCorrelationKey(string correlationKeyName, TestContext context)
    {
        if (Log.IsEnabled(LogLevel.Debug)) Log.LogDebug($"Get correlation key for '{correlationKeyName}'");

        string correlationKey = null;
        if (context.GetVariables().ContainsKey(correlationKeyName))
            correlationKey = context.GetVariable(correlationKeyName);

        var timeLeft = 1000L;
        var pollingInterval = 300L;
        while (correlationKey == null && timeLeft > 0)
        {
            timeLeft -= pollingInterval;

            if (RetryLog.IsEnabled(LogLevel.Debug))
                RetryLog.LogDebug(
                    $"Correlation key not available yet - retrying in {(timeLeft > 0 ? pollingInterval : pollingInterval + timeLeft)}ms");

            try
            {
                Thread.Sleep((int)(timeLeft > 0 ? pollingInterval : pollingInterval + timeLeft));
            }
            catch (ThreadInterruptedException e)
            {
                RetryLog.LogWarning(e, "Thread interrupted while waiting for retry");
            }

            if (context.GetVariables().ContainsKey(correlationKeyName))
                correlationKey = context.GetVariable(correlationKeyName);
        }

        if (correlationKey == null)
            throw new AgenixSystemException($"Failed to get correlation key for '{correlationKeyName}'");

        return correlationKey;
    }

    /// <summary>
    ///     Finds the stored object by its correlation key.
    /// </summary>
    /// <param name="correlationKey">The correlation key.</param>
    /// <param name="timeout">The timeout period in milliseconds.</param>
    /// <returns>The found object.</returns>
    public override T Find(string correlationKey, long timeout)
    {
        var timeLeft = timeout;
        var pollingInterval = _endpointConfiguration.PollingInterval;

        var stored = base.Find(correlationKey, timeLeft);

        while (stored == null && timeLeft > 0)
        {
            timeLeft -= pollingInterval;

            if (RetryLog.IsEnabled(LogLevel.Debug))
                RetryLog.LogDebug(
                    $"{_retryLogMessage} - retrying in {(timeLeft > 0 ? pollingInterval : pollingInterval + timeLeft)}ms");

            try
            {
                Thread.Sleep((int)(timeLeft > 0 ? pollingInterval : pollingInterval + timeLeft));
            }
            catch (ThreadInterruptedException e)
            {
                RetryLog.LogWarning(e, "Thread interrupted while waiting for retry");
            }

            stored = base.Find(correlationKey, timeLeft);
        }

        return stored;
    }

    /// <summary>
    ///     Gets the retry logger message.
    /// </summary>
    /// <returns>The retry log message.</returns>
    public string GetRetryLogMessage()
    {
        return _retryLogMessage;
    }
}
