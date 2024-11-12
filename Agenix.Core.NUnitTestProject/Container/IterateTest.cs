using System.Collections.Generic;
using Agenix.Core.Actions;
using Agenix.Core.Container;
using Moq;
using NUnit.Framework;

namespace Agenix.Core.NUnitTestProject.Container;

public class IterateTest : AbstractNUnitSetUp
{
    private readonly ITestAction _action = new Mock<ITestAction>().Object;

    public static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            yield return new TestCaseData("i lt= 5");
            yield return new TestCaseData("@LowerThan(6)@");
        }
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void TestIteration(string expression)
    {
        Mock.Get(_action).Reset();

        var iterate = new Iterate.Builder()
            .Condition(expression)
            .Index("i")
            .Actions(_action)
            .Build();

        iterate.Execute(Context);

        Assert.That(Context.GetVariable("${i}"), Is.Not.Null);
        Assert.That(Context.GetVariable("${i}"), Is.EqualTo("5"));

        Mock.Get(_action).Verify(x => x.Execute(Context), Times.Exactly(5));
    }

    [Test]
    public void TestStep()
    {
        Mock.Get(_action).Reset();

        var iterate = new Iterate.Builder()
            .Step(2)
            .Condition("i lt= 10")
            .Index("i")
            .Actions(_action)
            .Build();

        iterate.Execute(Context);

        Assert.That(Context.GetVariable("${i}"), Is.Not.Null);
        Assert.That(Context.GetVariable("${i}"), Is.EqualTo("9"));

        Mock.Get(_action).Verify(x => x.Execute(Context), Times.Exactly(5));
    }

    [Test]
    public void TestStart()
    {
        Mock.Get(_action).Reset();

        var iterate = new Iterate.Builder()
            .Step(2)
            .StartsWith(2)
            .Condition("i lt= 10")
            .Index("i")
            .Actions(_action)
            .Build();

        iterate.Execute(Context);

        Assert.That(Context.GetVariable("${i}"), Is.Not.Null);
        Assert.That(Context.GetVariable("${i}"), Is.EqualTo("10"));

        Mock.Get(_action).Verify(x => x.Execute(Context), Times.Exactly(5));
    }

    [Test]
    public void TestNoIterationBasedOnCondition()
    {
        Mock.Get(_action).Reset();

        var iterate = new Iterate.Builder()
            .Condition("i lt 0")
            .Index("i")
            .Actions(_action)
            .Build();

        iterate.Execute(Context);

        Assert.Throws<KeyNotFoundException>(() =>
        {
            var variable = Context.GetVariables()["i"];
        });
    }

    [Test]
    public void TestIterationWithIndexManipulation()
    {
        var incrementTestAction = DefaultTestActionBuilder
            .Action(context =>
            {
                var end = long.Parse(context.GetVariable("end"));
                context.SetVariable("end", (end - 25).ToString());
            })
            .Build();

        Mock.Get(_action).Reset();

        Context.SetVariable("end", 100);

        var iterate = new Iterate.Builder()
            .Condition("i lt ${end}")
            .Index("i")
            .Actions(_action, incrementTestAction)
            .Build();

        iterate.Execute(Context);

        Assert.That(Context.GetVariable("${i}"), Is.Not.Null);
        Assert.That(Context.GetVariable("${i}"), Is.EqualTo("4"));

        Mock.Get(_action).Verify(x => x.Execute(Context), Times.Exactly(4));
    }

    [Test]
    public void TestIterationConditionExpression()
    {
        Mock.Get(_action).Reset();

        var iterate = new Iterate.Builder()
            .Condition((index, context) => index <= 5)
            .Index("i")
            .Actions(_action)
            .Build();

        iterate.Execute(Context);

        Assert.That(Context.GetVariable("${i}"), Is.Not.Null);
        Assert.That(Context.GetVariable("${i}"), Is.EqualTo("5"));

        Mock.Get(_action).Verify(x => x.Execute(Context), Times.Exactly(5));
    }
}