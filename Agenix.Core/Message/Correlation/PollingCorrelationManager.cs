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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
    ///     Gets the correlation key for the given identifier.
    ///     Consults the test context with variables for retrieving the stored correlation key.
    /// </summary>
    /// <param name="correlationKeyName">The correlation key name.</param>
    /// <param name="context">The test context.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The correlation key.</returns>
    public override async Task<string> GetCorrelationKey(string correlationKeyName, TestContext context,
        CancellationToken cancellationToken = default)
    {
        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug("Get correlation key for '{CorrelationKeyName}'", correlationKeyName);
        }

        var correlationKey = GetCorrelationKeyFromContext(context, correlationKeyName);

        var timeLeft = 1000L;
        const long pollingInterval = 300L;

        if (correlationKey != null)
        {
            return correlationKey;
        }

        while (timeLeft > 0)
        {
            var delayTime = Math.Min(pollingInterval, timeLeft);
            timeLeft -= delayTime;

            if (RetryLog.IsEnabled(LogLevel.Debug))
            {
                RetryLog.LogDebug(
                    "Correlation key not available yet - retrying in {DelayTime}ms, {TimeLeft}ms remaining",
                    delayTime, timeLeft);
            }

            try
            {
                await Task.Delay((int)delayTime, cancellationToken);
            }
            catch (OperationCanceledException e)
            {
                RetryLog.LogWarning(e,
                    "Operation was canceled while waiting for correlation key '{CorrelationKeyName}'",
                    correlationKeyName);
                throw new AgenixSystemException(
                    $"Operation was canceled while waiting for correlation key '{correlationKeyName}'", e);
            }

            correlationKey = GetCorrelationKeyFromContext(context, correlationKeyName);
            if (correlationKey != null)
            {
                return correlationKey;
            }
        }

        throw new AgenixSystemException($"Failed to get correlation key for '{correlationKeyName}'");
    }

    private static string GetCorrelationKeyFromContext(TestContext context, string correlationKeyName)
    {
        return context.GetVariables().TryGetValue(correlationKeyName, out var value) ? value as string : null;
    }


    /// <summary>
    ///     Finds the stored object by its correlation key.
    /// </summary>
    /// <param name="correlationKey">The correlation key.</param>
    /// <param name="timeout">The timeout period in milliseconds.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The found object.</returns>
    public override async Task<T> Find(string correlationKey, long timeout,
        CancellationToken cancellationToken = default)
    {
        var timeLeft = timeout;
        var pollingInterval = _endpointConfiguration.PollingInterval;

        var stored = await base.Find(correlationKey, timeLeft, cancellationToken);

        while (EqualityComparer<T>.Default.Equals(stored, default) && timeLeft > 0)
        {
            timeLeft -= pollingInterval;
            var delayTime = (int)(timeLeft > 0 ? pollingInterval : pollingInterval + timeLeft);

            if (RetryLog.IsEnabled(LogLevel.Debug))
            {
                RetryLog.LogDebug("{RetryLogMessage} - retrying in {DelayTime}ms", _retryLogMessage, delayTime);
            }

            try
            {
                await Task.Delay(delayTime, cancellationToken);
            }
            catch (OperationCanceledException e)
            {
                RetryLog.LogWarning(e, "Operation was canceled while waiting for retry");
                throw new AgenixSystemException("Operation was canceled while waiting for retry");
            }

            stored = await base.Find(correlationKey, timeLeft, cancellationToken);
        }

        return stored;
    }

    /// <summary>
    ///     Convenience method for using default timeout settings of endpoint configuration.
    /// </summary>
    /// <param name="correlationKey">The correlation key.</param>
    /// <returns>The correlated object.</returns>
    public async Task<T> Find(string correlationKey)
    {
        return await Find(correlationKey, _endpointConfiguration.Timeout);
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
