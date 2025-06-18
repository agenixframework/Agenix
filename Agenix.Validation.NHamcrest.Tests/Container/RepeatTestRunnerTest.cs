using Agenix.Core;
using Agenix.Core.Actions;
using Agenix.Core.Container;
using NUnit.Framework.Legacy;
using Is = NHamcrest.Is;
using static Agenix.Core.Container.RepeatUntilTrue.Builder;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.SleepAction.Builder;
using static Agenix.Validation.NHamcrest.Container.NHamcrestConditionExpression;


namespace Agenix.Validation.NHamcrest.Tests.Container;

public class RepeatTestRunnerTest : AbstractNUnitSetUp
{
    [Test]
    public void TestRepeatBuilderWithHamcrestConditionExpression()
    {
        var builder = new DefaultTestCaseRunner(Context);
        builder.SetVariable("var", "foo");

        builder.Run(Repeat()
            .Until(AssertThat(Is.GreaterThan(5)).AsIteratingCondition())
            .Index("i")
            .StartsWith(2)
            .Actions(
                Echo("${var}"),
                Sleep().Milliseconds(100),
                Echo("${var}")
            ));

        ClassicAssert.IsNotNull(Context.GetVariable("i"));
        ClassicAssert.AreEqual("5", Context.GetVariable("i"));

        var test = builder.GetTestCase();
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.AreEqual(typeof(RepeatUntilTrue), test.GetActions()[0].GetType());
        ClassicAssert.AreEqual("repeat", test.GetActions()[0].Name);

        var container = (RepeatUntilTrue)test.GetActions()[0];
        ClassicAssert.AreEqual(3, container.GetActionCount());
        ClassicAssert.AreEqual(2, container.GetStart());
        ClassicAssert.AreEqual("i", container.GetIndexName());
        ClassicAssert.AreEqual(typeof(EchoAction), container.GetTestAction(0).GetType());
    }
}
