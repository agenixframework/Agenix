using System;
using System.Threading.Tasks;
using Agenix.Core.Common;
using Agenix.Core.Exceptions;
using log4net;

namespace Agenix.Core.Actions;

public abstract class AbstractAsyncTestAction : AbstractTestAction, ICompletable
{
    private static readonly ILog _log = LogManager.GetLogger(typeof(AbstractAsyncTestAction));
    private Task _finished;

    /// Determines if the asynchronous test action is completed.
    /// @param context The test context which provides required execution details.
    /// @return true if the action has finished executing or is disabled; otherwise, false.
    /// /
    public bool IsDone(TestContext context)
    {
        return (_finished?.IsCompleted ?? IsDisabled(context)) || IsDisabled(context);
    }

    public override void DoExecute(TestContext context)
    {
        var tcs = new TaskCompletionSource<TestContext>();


        // Set up completion logic
        tcs.Task.ContinueWith(task =>
        {
            if (task.Exception != null)
                OnError(context, task.Exception.GetBaseException());
            else if (context.HasExceptions())
                OnError(context, context.GetExceptions()[0]);
            else
                OnSuccess(context);
        });

        // Execute async action
        _finished = Task.Run(async () =>
        {
            try
            {
                await DoExecuteAsync(context);
            }
            catch (Exception e)
            {
                _log.Warn("Async test action execution raised error", e);
                context.AddException(
                    (CoreSystemException)(e is CoreSystemException ? e : new CoreSystemException(e.Message)));
            }
            finally
            {
                tcs.SetResult(context);
            }
        });
    }

    public abstract Task DoExecuteAsync(TestContext context);

    /**
     * Optional validation step after async test action performed with success.
     * @param context
     */
    public void OnSuccess(TestContext context)
    {
    }

    /**
     * Optional validation step after async test action performed with success.
     * @param context
     */
    public void OnError(TestContext context, Exception error)
    {
    }
}