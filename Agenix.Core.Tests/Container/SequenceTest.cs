using Agenix.Api.Exceptions;
using Agenix.Core.Actions;
using Agenix.Core.Container;
using Moq;
using NUnit.Framework;
using ITestAction = Agenix.Api.ITestAction;

namespace Agenix.Core.Tests.Container;

public class SequenceTest : AbstractNUnitSetUp
{
    private readonly ITestAction _action = new Mock<ITestAction>().Object;

    [Test]
    public void TestSingleAction()
    {
        Mock.Get(_action).Reset();

        var sequenceAction = new Sequence.Builder()
            .Actions(_action)
            .Build();

        sequenceAction.Execute(Context);

        Mock.Get(_action).Verify(a => a.Execute(Context));
    }

    [Test]
    public void TestMultipleActions()
    {
        var action1 = new Mock<ITestAction>().Object;
        var action2 = new Mock<ITestAction>().Object;
        var action3 = new Mock<ITestAction>().Object;

        Mock.Get(action1).Reset();
        Mock.Get(action2).Reset();
        Mock.Get(action3).Reset();

        var sequenceAction = new Sequence.Builder()
            .Actions(action1, action2, action3)
            .Build();

        sequenceAction.Execute(Context);

        Mock.Get(action1).Verify(a => a.Execute(Context));
        Mock.Get(action2).Verify(a => a.Execute(Context));
        Mock.Get(action3).Verify(a => a.Execute(Context));
    }

    [Test]
    public void TestFirstActionFailing()
    {
        var action1 = new Mock<ITestAction>().Object;
        var action2 = new Mock<ITestAction>().Object;
        var action3 = new Mock<ITestAction>().Object;

        Mock.Get(action1).Reset();
        Mock.Get(action2).Reset();
        Mock.Get(action3).Reset();

        var sequenceAction = new Sequence.Builder()
            .Actions(new FailAction.Builder().Build(), action1, action2, action3)
            .Build();

        Assert.Throws<AgenixSystemException>(() => { sequenceAction.Execute(Context); });
    }

    [Test]
    public void TestLastActionFailing()
    {
        var action1 = new Mock<ITestAction>().Object;
        var action2 = new Mock<ITestAction>().Object;
        var action3 = new Mock<ITestAction>().Object;

        Mock.Get(action1).Reset();
        Mock.Get(action2).Reset();
        Mock.Get(action3).Reset();

        var sequenceAction = new Sequence.Builder()
            .Actions(action1, action2, action3, new FailAction.Builder().Build())
            .Build();

        Assert.Throws<AgenixSystemException>(() =>
        {
            sequenceAction.Execute(Context);
            Mock.Get(action1).Verify(a => a.Execute(Context));
            Mock.Get(action2).Verify(a => a.Execute(Context));
            Mock.Get(action3).Verify(a => a.Execute(Context));
        });
    }

    [Test]
    public void TestFailingAction()
    {
        var action1 = new Mock<ITestAction>().Object;
        var action2 = new Mock<ITestAction>().Object;
        var action3 = new Mock<ITestAction>().Object;

        Mock.Get(action1).Reset();
        Mock.Get(action2).Reset();
        Mock.Get(action3).Reset();

        var sequenceAction = new Sequence.Builder()
            .Actions(action1, new FailAction.Builder().Build(), action2, action3)
            .Build();

        Assert.Throws<AgenixSystemException>(() =>
        {
            sequenceAction.Execute(Context);
            Mock.Get(action1).Verify(a => a.Execute(Context));
        });
    }
}
