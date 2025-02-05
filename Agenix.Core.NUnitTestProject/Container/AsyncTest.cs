using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agenix.Core.Container;
using Agenix.Core.Exceptions;
using Agenix.Core.Util;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Container;

/// <summary>
///     Represents a suite of asynchronous unit tests for verifying the behavior of an
///     asynchronous action container. The tests ensure correct execution paths for single
///     and multiple actions, handle action success, and verify behavior in failure or timeout scenarios.
/// </summary>
/// <remarks>
///     This test class uses NUnit as the test framework and mocks for verifying interactions
///     with actions. It extends from <see cref="AbstractNUnitSetUp" />, preparing mocks in the
///     <see cref="SetUp" /> method before each test execution.
/// </remarks>

[Platform(Exclude = "Linux", Reason = "Only runs on non-Linux platforms.")]
public class AsyncTest : AbstractNUnitSetUp
{
    // Creating mocks for the TestAction class
    private Mock<ITestAction> _action;
    private Mock<ITestAction> _error;
    private Mock<ITestAction> _success;

    // Setup is where we initialize our test setup, similar to the Java example's use of reset
    [SetUp]
    public void SetUp()
    {
        _action = new Mock<ITestAction>();
        _success = new Mock<ITestAction>();
        _error = new Mock<ITestAction>();
    }

    [Test]
    public async Task TestSingleActionAsync()
    {
        // Building the Async container
        var container = new Async.Builder()
            .Actions(_action.Object)
            .SuccessAction(_success.Object)
            .ErrorAction(_error.Object)
            .Build();

        // Execute the container
        container.Execute(Context);

        // Wait for the asynchronous operation to complete (Replace with appropriate wait logic if needed)
        await WaitUtils.WaitForCompletion(container, Context);

        // Verify that the action was executed
        _action.Verify(a => a.Execute(Context), Times.Once);

        // Verify that the success action was executed
        _success.Verify(s => s.Execute(Context), Times.Once);

        // Verify that the error action was never executed
        _error.Verify(e => e.Execute(Context), Times.Never);
    }

    [Test]
    public async Task TestMultipleActionsAsync()
    {
        var action1 = new Mock<ITestAction>();
        var action2 = new Mock<ITestAction>();
        var action3 = new Mock<ITestAction>();

        // Build the Async container
        var container = new Async.Builder()
            .SuccessAction(_success.Object)
            .ErrorAction(_error.Object)
            .Actions(action1.Object, action2.Object, action3.Object)
            .Build();

        // Execute the container
        container.Execute(Context);

        // Wait for the asynchronous operation to complete (Replace with appropriate wait logic if needed)
        await WaitUtils.WaitForCompletion(container, Context);

        // Verify that each action was executed once
        action1.Verify(a => a.Execute(Context), Times.Once);
        action2.Verify(a => a.Execute(Context), Times.Once);
        action3.Verify(a => a.Execute(Context), Times.Once);

        // Verify the success action was executed
        _success.Verify(s => s.Execute(Context), Times.Once);

        // Verify that the error action was never executed
        _error.Verify(e => e.Execute(Context), Times.Never);
    }

    [Test]
    public async Task TestFailingActionAsync()
    {
        var action1 = new Mock<ITestAction>();
        var action2 = new Mock<ITestAction>();
        var action3 = new Mock<ITestAction>();

        // Define a faulty action.
        var failAction = new Mock<ITestAction>();
        failAction.Setup(f => f.Execute(Context))
            .Throws(new CoreSystemException("Generated error to interrupt test execution"));

        // Build the Async container including the failAction
        var container = new Async.Builder()
            .SuccessAction(_success.Object)
            .ErrorAction(_error.Object)
            .Actions(action1.Object, failAction.Object, action2.Object, action3.Object)
            .Build();

        // Execute the container
        container.Execute(Context);

        // Wait for the asynchronous operation to complete (Replace with appropriate wait logic if needed)
        await WaitUtils.WaitForCompletion(container, Context);

        // Check for exceptions in context
        ClassicAssert.AreEqual(1, Context.GetExceptions().Count);
        ClassicAssert.IsInstanceOf<CoreSystemException>(Context.GetExceptions().First());
        ClassicAssert.AreEqual("Generated error to interrupt test execution", Context.GetExceptions().First().Message);

        // Verify execution order
        action1.Verify(a => a.Execute(Context), Times.Once);
        action2.Verify(a => a.Execute(Context), Times.Never);
        action3.Verify(a => a.Execute(Context), Times.Never);

        _error.Verify(e => e.Execute(Context), Times.Once);
        _success.Verify(s => s.Execute(Context), Times.Never);
    }

    [Test]
    public void TestWaitForFinishTimeout()
    {
        // Setup the action to simulate a delay
        _action.Setup(a => a.Execute(Context))
            .Callback(() => Thread.Sleep(500));

        var container = new Async.Builder()
            .Actions(_action.Object)
            .Build();

        container.Execute(Context);

        // Assert that the waitForDone logic throws a TimeoutException
        Assert.ThrowsAsync<CoreSystemException>(async () =>
        {
            await WaitUtils.WaitForCompletion(container, Context, 200);
        });
    }

    [Test]
    public async Task TestWaitForFinishErrorAsync()
    {
        // Set up the action to throw an exception
        _action.Setup(a => a.Execute(Context)).Throws(new CoreSystemException("FAILED!"));

        var container = new Async.Builder()
            .Actions(_action.Object)
            .Build();

        try
        {
            container.DoExecute(Context);
        }
        catch (CoreSystemException)
        {
            // Handle if immediate effects needed, such as logging, though typically the system would capture this.
        }

        await WaitUtils.WaitForCompletion(container, Context, 2000);

        // Assert that the appropriate exception was tracked within the context
        ClassicAssert.AreEqual(1, Context.GetExceptions().Count);
        ClassicAssert.IsInstanceOf<CoreSystemException>(Context.GetExceptions().First());
        ClassicAssert.AreEqual("FAILED!", Context.GetExceptions().First().Message);
    }
}