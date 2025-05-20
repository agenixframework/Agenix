using System;
using System.Threading.Tasks;
using Agenix.Api.Exceptions;
using Agenix.Core.Actions;
using log4net;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Actions;

/// <summary>
///     Represents a unit test class for testing asynchronous actions derived from AbstractAsyncTestAction.
/// </summary>
public class AbstractAsyncTestActionTest : AbstractNUnitSetUp
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(AbstractAsyncTestActionTest));

    [Test]
    public async Task TestOnSuccess()
    {
        var result = new TaskCompletionSource<bool>();

        var action = new CustomAsyncTestActionSuccess(result);

        action.Execute(Context);

        ClassicAssert.IsTrue(await TaskExtensions.TimeoutAfter(result.Task, TimeSpan.FromMilliseconds(1000)));
    }

    [Test]
    public void TestOnError()
    {
        var result = new TaskCompletionSource<bool>();

        var action = new CustomAsyncTestActionError(result);

        // Execute the action
        var exception = Assert.ThrowsAsync<AgenixSystemException>(async () =>
            {
                action.Execute(Context);
                await TaskExtensions.TimeoutAfter(result.Task, TimeSpan.FromMilliseconds(1000));
            },
            "Expected CoreSystemException with specific message.");

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("Failed!"));
    }

    private static class TaskExtensions
    {
        public static async Task<T> TimeoutAfter<T>(Task<T> task, TimeSpan timeout)
        {
            if (task == await Task.WhenAny(task, Task.Delay(timeout))) return await task;

            throw new TimeoutException();
        }
    }

    private class CustomAsyncTestActionError(TaskCompletionSource<bool> result) : AbstractAsyncTestAction
    {
        public override Task DoExecuteAsync(TestContext context)
        {
            return Task.Run(() => throw new AgenixSystemException("Failed!"));
        }

        public override void OnSuccess(TestContext context)
        {
            result.SetResult(false);
        }

        public override void OnError(TestContext context, Exception error)
        {
            result.SetException(error);
        }
    }

    private class CustomAsyncTestActionSuccess(TaskCompletionSource<bool> result) : AbstractAsyncTestAction
    {
        public override Task DoExecuteAsync(TestContext context)
        {
            return Task.Run(() => { Log.Info("Success!"); });
        }

        public override void OnSuccess(TestContext context)
        {
            result.SetResult(true);
        }

        public override void OnError(TestContext context, Exception error)
        {
            result.SetException(error);
        }
    }
}