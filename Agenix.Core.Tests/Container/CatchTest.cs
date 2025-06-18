using Agenix.Api.Exceptions;
using Agenix.Core.Actions;
using Agenix.Core.Container;
using Moq;
using NUnit.Framework;
using static Agenix.Core.Container.Catch.Builder;
using ITestAction = Agenix.Api.ITestAction;

namespace Agenix.Core.Tests.Container;

/// <summary>
///     Represents a set of unit tests for exception handling functionality using the Catch class.
/// </summary>
/// <remarks>
///     The CatchTest class includes various test methods that validate the behavior of exception handling
///     mechanisms. These tests check default exception handling, specific exception handling, scenarios
///     where no exception is present, and the execution flow of actions within the exception handling context.
///     Each test case leverages the NUnit testing framework for assertions and Mock objects to verify outcomes.
/// </remarks>
public class CatchTest : AbstractNUnitSetUp
{
    [Test]
    public void TestCatchDefaultException()
    {
        var catchAction = CatchException()
            .Actions(new FailAction.Builder())
            .Build();

        catchAction.Execute(Context);
    }

    [Test]
    public void TestCatchException()
    {
        var catchAction = CatchException()
            .Actions(new FailAction.Builder())
            .Exception(typeof(AgenixSystemException))
            .Build();

        catchAction.Execute(Context);
    }

    [Test]
    public void TestNothingToCatch()
    {
        var catchAction = CatchException()
            .Actions(new EchoAction.Builder())
            .Exception(typeof(AgenixSystemException))
            .Build();

        catchAction.Execute(Context);
    }

    [Test]
    public void TestCatchFirstActionFailing()
    {
        var actionMock = new Mock<ITestAction>();

        actionMock.Reset();

        var catchAction = new Catch.Builder()
            .Actions(new FailAction.Builder().Build(), actionMock.Object)
            .Exception(typeof(AgenixSystemException))
            .Build();

        catchAction.Execute(Context);

        actionMock.Verify(action => action.Execute(Context), Times.Once);
    }

    [Test]
    public void TestCatchSomeActionFailing()
    {
        var actionMock = new Mock<ITestAction>();

        actionMock.Reset();

        var catchAction = new Catch.Builder()
            .Actions(actionMock.Object, new FailAction.Builder().Build(), actionMock.Object)
            .Exception(typeof(AgenixSystemException))
            .Build();

        catchAction.Execute(Context);

        actionMock.Verify(action => action.Execute(Context), Times.Exactly(2));
    }
}
