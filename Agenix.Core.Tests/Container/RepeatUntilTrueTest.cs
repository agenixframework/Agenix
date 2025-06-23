using System.Collections.Generic;
using Agenix.Core.Container;
using Moq;
using NUnit.Framework;
using ITestAction = Agenix.Api.ITestAction;

namespace Agenix.Core.Tests.Container;

public class RepeatUntilTrueTest : AbstractNUnitSetUp
{
    private readonly ITestAction _action = new Mock<ITestAction>().Object;

    public static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            yield return new TestCaseData("i = 5");
            yield return new TestCaseData("@GreaterThan(4)@");
        }
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void TestRepeat(string expression)
    {
        Mock.Get(_action).Reset();

        var iterate = new RepeatUntilTrue.Builder()
            .Condition(expression)
            .Index("i")
            .Actions(_action)
            .Build();

        iterate.Execute(Context);

        Assert.That(Context.GetVariable("${i}"), Is.Not.Null);
        Assert.That(Context.GetVariable("${i}"), Is.EqualTo("4"));

        Mock.Get(_action).Verify(x => x.Execute(Context), Times.Exactly(4));
    }

    [Test]
    public void TestRepeatMinimumOnce()
    {
        Mock.Get(_action).Reset();

        var iterate = new RepeatUntilTrue.Builder()
            .Condition("i gt 0")
            .Index("i")
            .Actions(_action)
            .Build();

        iterate.Execute(Context);

        Assert.That(Context.GetVariable("${i}"), Is.Not.Null);
        Assert.That(Context.GetVariable("${i}"), Is.EqualTo("1"));

        Mock.Get(_action).Verify(x => x.Execute(Context), Times.Exactly(1));
    }

    [Test]
    public void TestIterationConditionExpression()
    {
        Mock.Get(_action).Reset();

        var iterate = new RepeatUntilTrue.Builder()
            .Condition((index, context) => index == 5)
            .Index("i")
            .Actions(_action)
            .Build();

        iterate.Execute(Context);

        Assert.That(Context.GetVariable("${i}"), Is.Not.Null);
        Assert.That(Context.GetVariable("${i}"), Is.EqualTo("4"));

        Mock.Get(_action).Verify(x => x.Execute(Context), Times.Exactly(4));
    }
}
