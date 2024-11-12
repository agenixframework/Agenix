using System;
using System.Threading;
using System.Threading.Tasks;
using Agenix.Core.Common;
using Agenix.Core.Exceptions;
using log4net;

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
    private static readonly ILog _log = LogManager.GetLogger(typeof(DefaultTestCase));

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
                _log.Debug("Wait for test container to finish properly ...");

                // Wait for a small duration or until the cancellation token is triggered
                await Task.Delay(100, cancellationTokenSource.Token);

                // Check if timeout has occurred
                if (cancellationTokenSource.Token.IsCancellationRequested)
                    throw new TimeoutException("Failed to wait for the test container to finish properly");
            }
        }
        catch (Exception ex) when (ex is TaskCanceledException or TimeoutException or OperationCanceledException)
        {
            throw new CoreSystemException("Failed to wait for test container to finish properly", ex);
        }
        finally
        {
            await cancellationTokenSource.CancelAsync();
        }
    }
}