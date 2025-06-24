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
using Agenix.Api.Common;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Util;

/**
 * Utility class for test cases providing several utility
 * methods regarding Agenix test cases.
 */
public abstract class WaitUtils
{
    /**
     * Logger
     */
    private static readonly ILogger Log = LogManager.GetLogger(typeof(WaitUtils));

    /**
     * Prevent instantiation.
     */
    private WaitUtils()
    {
    }

    /// <summary>
    ///     Uses the given scheduler to wait for the specified container to complete its execution within a specified timeout.
    ///     If the container is already in a done state, the method returns immediately.
    ///     The method repeatedly checks the container's state and waits for it to complete.
    ///     If the container does not complete within the specified timeout, an exception is thrown.
    /// </summary>
    /// <param name="container">The container whose completion is awaited.</param>
    /// <param name="context">The test context to use when checking if the container is done.</param>
    /// <param name="timeout">The maximum time to wait for the container to complete, in milliseconds.</param>
    /// <return>A task representing the asynchronous waiting operation.</return>
    public static async Task WaitForCompletion(
        ICompletable container,
        TestContext context,
        long timeout = 10000L)
    {
        if (container.IsDone(context))
        {
            return;
        }

        using var cancellationTokenSource = new CancellationTokenSource((int)timeout);

        try
        {
            while (!container.IsDone(context))
            {
                Log.LogDebug("Wait for test container to finish properly ...");

                // This will throw TaskCanceledException when timeout occurs
                await Task.Delay(100, cancellationTokenSource.Token);
            }
        }
        catch (TaskCanceledException) when (cancellationTokenSource.Token.IsCancellationRequested)
        {
            // Timeout occurred
            throw new AgenixSystemException(
                "Failed to wait for the test container to finish properly - timeout exceeded");
        }
        catch (OperationCanceledException) when (cancellationTokenSource.Token.IsCancellationRequested)
        {
            // Timeout occurred
            throw new AgenixSystemException(
                "Failed to wait for the test container to finish properly - operation cancelled");
        }
        finally
        {
            await cancellationTokenSource.CancelAsync();
        }
    }
}
