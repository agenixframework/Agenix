using Agenix.Core.Actions;
using Agenix.Core.Container;
using Agenix.Core.Exceptions;
using Moq;
using NUnit.Framework;

namespace Agenix.Core.NUnitTestProject.Container;

public class ConditionalTest : AbstractNUnitSetUp
{
    private readonly ITestAction _action = new Mock<ITestAction>().Object;

    [Test]
    public void TestConditionFalse()
    {
        Mock.Get(_action).Reset();

        var conditionalAction = new Conditional.Builder()
            .When("1 = 0")
            .Actions(_action)
            .Build();

        conditionalAction.Execute(Context);

        Mock.Get(_action).Verify(a => a.Execute(It.IsAny<TestContext>()), Times.Never);
    }

    [Test]
    public void TestConditionMatcherFalse()
    {
        Mock.Get(_action).Reset();

        var conditionalAction = new Conditional.Builder()
            .When("@LowerThan(-1)@")
            .Actions(_action)
            .Build();

        conditionalAction.Execute(Context);

        Mock.Get(_action).Verify(a => a.Execute(It.IsAny<TestContext>()), Times.Never);
    }

    [Test]
    public void TestSingleAction()
    {
        Mock.Get(_action).Reset();

        var conditionalAction = new Conditional.Builder()
            .When("1 = 1")
            .Actions(_action)
            .Build();

        conditionalAction.Execute(Context);

        Mock.Get(_action).Verify(a => a.Execute(Context));
    }

    [Test]
    public void TestMatcherSingleAction()
    {
        Mock.Get(_action).Reset();

        var conditionalAction = new Conditional.Builder()
            .When("@Empty()@")
            .Actions(_action)
            .Build();

        conditionalAction.Execute(Context);

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

        var conditionalAction = new Conditional.Builder()
            .When("1 = 1")
            .Actions(action1, action2, action3)
            .Build();

        conditionalAction.Execute(Context);

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

        var conditionalAction = new Conditional.Builder()
            .When("1 = 1")
            .Actions(new FailAction.Builder().Build(), action1, action2, action3)
            .Build();

        Assert.Throws<CoreSystemException>(() => { conditionalAction.Execute(Context); });
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

        var conditionalAction = new Conditional.Builder()
            .When("1 = 1")
            .Actions(action1, action2, action3, new FailAction.Builder().Build())
            .Build();

        Assert.Throws<CoreSystemException>(() =>
        {
            conditionalAction.Execute(Context);
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

        var conditionalAction = new Conditional.Builder()
            .When("1 = 1")
            .Actions(action1, new FailAction.Builder().Build(), action2, action3)
            .Build();

        Assert.Throws<CoreSystemException>(() =>
        {
            conditionalAction.Execute(Context);
            Mock.Get(action1).Verify(a => a.Execute(Context));
            Mock.Get(action2).Verify(a => a.Execute(Context));
            Mock.Get(action3).Verify(a => a.Execute(Context));
        });
    }
}