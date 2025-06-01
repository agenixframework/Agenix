using Agenix.Core;
using Agenix.Core.Actions;
using Agenix.Core.Container;
using NUnit.Framework.Legacy;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.SleepAction.Builder;
using static Agenix.Validation.NHamcrest.Container.NHamcrestConditionExpression;
using static Agenix.Core.Container.RepeatOnErrorUntilTrue.Builder;
using Is = NHamcrest.Is;

namespace Agenix.Validation.NHamcrest.Tests.Container;

public class RepeatOnErrorTestActionBuilderTest : AbstractNUnitSetUp
{
    [Test]
    public void TestRepeatOnErrorBuilderWithHamcrestConditionExpression()
    {
        var builder = new DefaultTestCaseRunner(Context);
        builder.SetVariable("var", "foo");

        builder.Run(
            RepeatOnError().AutoSleep(250)
                .Until("i gt 5")
                .Actions(Echo("${var}"), Sleep().Milliseconds(50), Echo("${var}"))
        );

        builder.Run(
            RepeatOnError().AutoSleep(200)
                .Until(AssertThat(Is.EqualTo(5)).AsIteratingCondition())
                .Index("k")
                .StartsWith(2)
                .Actions(Echo("${var}"))
        );

        ClassicAssert.IsNotNull(Context.GetVariable("i"));
        ClassicAssert.AreEqual(Context.GetVariable("i"), "1");
        ClassicAssert.IsNotNull(Context.GetVariable("k"));
        ClassicAssert.AreEqual(Context.GetVariable("k"), "2");

        var test = builder.GetTestCase();
        ClassicAssert.AreEqual(test.GetActionCount(), 2);
        ClassicAssert.AreEqual(test.GetActions()[0].GetType(), typeof(RepeatOnErrorUntilTrue));
        ClassicAssert.AreEqual(test.GetActions()[0].Name, "repeat-on-error");

        var container = (RepeatOnErrorUntilTrue)test.GetActions()[0];
        ClassicAssert.AreEqual(container.GetActionCount(), 3);
        ClassicAssert.AreEqual(container.AutoSleep, 250L);
        ClassicAssert.AreEqual(container.Condition, "i gt 5");
        ClassicAssert.AreEqual(container.GetStart(), 1);
        ClassicAssert.AreEqual(container.GetIndexName(), "i");
        ClassicAssert.AreEqual(container.GetTestAction(0).GetType(), typeof(EchoAction));

        container = (RepeatOnErrorUntilTrue)test.GetActions()[1];
        ClassicAssert.AreEqual(container.GetActionCount(), 1);
        ClassicAssert.AreEqual(container.AutoSleep, 200L);
        ClassicAssert.AreEqual(container.GetStart(), 2);
        ClassicAssert.AreEqual(container.GetIndexName(), "k");
        ClassicAssert.AreEqual(container.GetTestAction(0).GetType(), typeof(EchoAction));
    }
}
