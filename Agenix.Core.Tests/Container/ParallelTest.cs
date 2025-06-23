using System.Collections.Generic;
using Agenix.Api.Exceptions;
using Agenix.Core.Actions;
using Agenix.Core.Container;
using Moq;
using NUnit.Framework;
using ITestAction = Agenix.Api.ITestAction;

namespace Agenix.Core.Tests.Container;

public class ParallelTest : AbstractNUnitSetUp
{
    private readonly Mock<ITestAction> _actionMock = new();

    [Test]
    public void TestSingleAction()
    {
        var parallelAction = new Parallel.Builder().Build();

        _actionMock.Reset();

        var actionList = new List<ITestAction> { _actionMock.Object };

        parallelAction.SetActions(actionList);
        parallelAction.Execute(Context);

        _actionMock.Verify(action => action.Execute(Context), Times.Once);
    }

    [Test]
    public void TestParallelMultipleActions()
    {
        var parallelAction = new Parallel.Builder().Build();

        _actionMock.Reset();

        var actionList = new List<ITestAction>
        {
            new EchoAction.Builder().Build(), _actionMock.Object, new EchoAction.Builder().Build()
        };

        parallelAction.SetActions(actionList);

        parallelAction.Execute(Context);

        _actionMock.Verify(action => action.Execute(Context), Times.Once);
    }

    [Test]
    public void TestParallelActions()
    {
        var parallelAction = new Parallel.Builder().Build();

        var actionList = new List<ITestAction>
        {
            new EchoAction.Builder().Build(), new EchoAction.Builder().Build(), new EchoAction.Builder().Build()
        };

        var sleep = new SleepAction.Builder()
            .Milliseconds(300L)
            .Build();
        actionList.Add(sleep);

        parallelAction.SetActions(actionList);

        parallelAction.Execute(Context);
    }

    [Test]
    public void TestOneActionThatIsFailing()
    {
        var parallelAction = new Parallel.Builder().Build();

        var actionList = new List<ITestAction> { new FailAction.Builder().Build() };

        parallelAction.SetActions(actionList);

        Assert.Throws<AgenixSystemException>(() => parallelAction.Execute(Context));
    }

    [Test]
    public void TestOnlyActionFailingActions()
    {
        var parallelAction = new Parallel.Builder().Build();

        var actionList = new List<ITestAction>
        {
            new FailAction.Builder().Build(), new FailAction.Builder().Build(), new FailAction.Builder().Build()
        };

        parallelAction.SetActions(actionList);

        Assert.Throws<ParallelContainerException>(() => parallelAction.Execute(Context));
    }

    [Test]
    public void TestSingleFailingAction()
    {
        var parallelAction = new Parallel.Builder().Build();

        var actionList = new List<ITestAction>
        {
            new EchoAction.Builder().Build(), new FailAction.Builder().Build(), new EchoAction.Builder().Build()
        };

        parallelAction.SetActions(actionList);

        Assert.Throws<AgenixSystemException>(() => parallelAction.Execute(Context));
    }

    [Test]
    public void TestSomeFailingActions()
    {
        var parallelAction = new Parallel.Builder().Build();

        _actionMock.Reset(); // Equivalent to resetting the mock in Java

        var actionList = new List<ITestAction>
        {
            new EchoAction.Builder().Build(),
            new FailAction.Builder().Build(),
            _actionMock.Object,
            new FailAction.Builder().Build()
        };

        parallelAction.SetActions(actionList);

        Assert.Throws<ParallelContainerException>(() => parallelAction.Execute(Context));

        _actionMock.Verify(action => action.Execute(Context), Times.Once);
    }
}
