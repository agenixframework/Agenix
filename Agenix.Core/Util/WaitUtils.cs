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
        if (container.IsDone(context)) return;

        using var cancellationTokenSource = new CancellationTokenSource((int)timeout);

        try
        {
            while (!container.IsDone(context))
            {
                Log.LogDebug("Wait for test container to finish properly ...");

                // Wait for a small duration or until the cancellation token is triggered
                await Task.Delay(100, cancellationTokenSource.Token);

                // Check if timeout has occurred
                if (cancellationTokenSource.Token.IsCancellationRequested)
                    throw new TimeoutException("Failed to wait for the test container to finish properly");
            }
        }
        catch (Exception ex) when (ex is TaskCanceledException or TimeoutException or OperationCanceledException)
        {
            throw new AgenixSystemException("Failed to wait for test container to finish properly", ex);
        }
        finally
        {
            await cancellationTokenSource.CancelAsync();
        }
    }
}
