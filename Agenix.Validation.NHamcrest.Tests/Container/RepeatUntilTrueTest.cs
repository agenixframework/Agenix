using Agenix.Core.Container;
using Moq;
using NUnit.Framework.Legacy;
using Is = NHamcrest.Is;
using static Agenix.Validation.NHamcrest.Container.NHamcrestConditionExpression;
using ITestAction = Agenix.Api.ITestAction;

namespace Agenix.Validation.NHamcrest.Tests.Container;

public class RepeatUntilTrueTest : AbstractNUnitSetUp
{
    private readonly ITestAction _action = Mock.Of<ITestAction>();

    [Test]
    public void TestRepeatHamcrestConditionExpression()
    {
        Mock.Get(_action).Reset();

        var repeatUntilTrue = new RepeatUntilTrue.Builder()
            .Condition(AssertThat(Is.EqualTo(5)).AsIteratingCondition())
            .Index("i")
            .Actions(_action)
            .Build();
        repeatUntilTrue.Execute(Context);

        ClassicAssert.IsNotNull(Context.GetVariable("${i}"));
        ClassicAssert.AreEqual("4", Context.GetVariable("${i}"));

        Mock.Get(_action).Verify(a => a.Execute(Context), Times.Exactly(4));
    }
}
