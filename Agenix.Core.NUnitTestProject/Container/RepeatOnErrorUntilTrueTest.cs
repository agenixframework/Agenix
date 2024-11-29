using System.Collections.Generic;
using Agenix.Core.Actions;
using Agenix.Core.Container;
using Agenix.Core.Exceptions;
using Moq;
using NUnit.Framework;

namespace Agenix.Core.NUnitTestProject.Container;

/// <summary>
///     Test class for executing scenarios using RepeatOnErrorUntilTrue container logic in the NUnit framework.
/// </summary>
/// <remarks>
///     Tests include verifying successful iteration on initial execution and handling errors when conditions are not met.
///     It also verifies the integration and behavior of mocked actions along with expression-based conditions.
/// </remarks>
public class RepeatOnErrorUntilTrueTest : AbstractNUnitSetUp
{
    private readonly Mock<ITestAction> _action = new();

    [Test]
    [TestCaseSource(nameof(ExpressionProvider))]
    public void TestSuccessOnFirstIteration(string expression)
    {
        _action.Reset();

        var repeat = new RepeatOnErrorUntilTrue.Builder()
            .Condition(expression)
            .Index("i")
            .Actions(context => { _action.Object.Execute(context); })
            .Build();

        repeat.Execute(Context);

        _action.Verify(a => a.Execute(Context));
    }

    public static IEnumerable<object[]> ExpressionProvider()
    {
        yield return ["i = 5"];
        yield return ["@greaterThan(4)@"];
    }

    [Test]
    public void TestRepeatOnErrorNoSuccess()
    {
        Assert.Throws<CoreSystemException>(() =>
        {
            _action.Reset();

            var repeat = new RepeatOnErrorUntilTrue.Builder()
                .AutoSleep(0)
                .Condition("i = 5")
                .Index("i")
                .Actions(_action.Object, new FailAction.Builder().Build())
                .Build();

            repeat.Execute(Context);

            _action.Verify(a => a.Execute(Context), Times.Exactly(4));
        });
    }

    [Test]
    public void TestRepeatOnErrorNoSuccessConditionExpression()
    {
        Assert.Throws<CoreSystemException>(() =>
        {
            _action.Reset();

            var repeat = new RepeatOnErrorUntilTrue.Builder()
                .AutoSleep(0)
                .Condition((index, ctx) => index == 5)
                .Index("i")
                .Actions(_action.Object, new FailAction.Builder().Build())
                .Build();

            repeat.Execute(Context);

            _action.Verify(a => a.Execute(Context), Times.Exactly(4));
        });
    }
}