using Agenix.Core.Actions;
using Agenix.Core.Container;
using Agenix.Core.Exceptions;
using Moq;
using Is = NHamcrest.Is;
using static Agenix.Validation.NHamcrest.Container.NHamcrestConditionExpression;
using ITestAction = Agenix.Core.ITestAction;

namespace Agenix.Validation.NHamcrest.Tests.Container;

public class RepeatOnErrorUntilTrueTest : AbstractNUnitSetUp
{
    private readonly Mock<ITestAction> _action = new();

    [Test]
    public void TestRepeatOnErrorNoSuccessHamcrestConditionExpression()
    {
        Assert.Throws<CoreSystemException>(() =>
        {
            _action.Reset();

            var repeat = new RepeatOnErrorUntilTrue.Builder()
                .AutoSleep(0)
                .Condition(AssertThat(Is.EqualTo(5)).AsIteratingCondition())
                .Index("i")
                .Actions(_action.Object, new FailAction.Builder().Build())
                .Build();

            repeat.Execute(Context);
            _action.Verify(a => a.Execute(Context), Times.Exactly(4));
        });
    }
}