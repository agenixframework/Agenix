using Agenix.Core.Container;
using Moq;
using NUnit.Framework.Legacy;
using static Agenix.Validation.NHamcrest.Container.NHamcrestConditionExpression;
using Is = NHamcrest.Is;
using ITestAction = Agenix.Api.ITestAction;

namespace Agenix.Validation.NHamcrest.Tests.Container;

public class IterateTest : AbstractNUnitSetUp
{
    private readonly ITestAction _action = new Mock<ITestAction>().Object;

    [Test]
    public void TestHamcrestIterationConditionExpression()
    {
        Mock.Get(_action).Reset();

        var iterate = new Iterate.Builder()
            .Condition(AssertThat(Is.LessThanOrEqualTo(5)).AsIteratingCondition())
            .Index("i")
            .Actions(_action)
            .Build();
        iterate.Execute(Context);

        ClassicAssert.NotNull(Context.GetVariable("${i}"));
        ClassicAssert.AreEqual("5", Context.GetVariable("${i}"));

        Mock.Get(_action).Verify(a => a.Execute(Context), Times.Exactly(5));
    }
}