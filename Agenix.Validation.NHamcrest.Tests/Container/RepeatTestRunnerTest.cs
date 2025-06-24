using Agenix.Core;
using Agenix.Core.Actions;
using Agenix.Core.Container;
using NUnit.Framework.Legacy;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.SleepAction.Builder;
using static Agenix.Core.Container.RepeatUntilTrue.Builder;
using static Agenix.Validation.NHamcrest.Container.NHamcrestConditionExpression;
using Is = NHamcrest.Is;


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
        Assert.That(Context.GetVariable("i"), NUnit.Framework.Is.EqualTo("5"));

        var test = builder.GetTestCase();
        Assert.That(test.GetActionCount(), NUnit.Framework.Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), NUnit.Framework.Is.EqualTo(typeof(RepeatUntilTrue)));
        Assert.That(test.GetActions()[0].Name, NUnit.Framework.Is.EqualTo("repeat"));

        var container = (RepeatUntilTrue)test.GetActions()[0];
        Assert.That(container.GetActionCount(), NUnit.Framework.Is.EqualTo(3));
        Assert.That(container.GetStart(), NUnit.Framework.Is.EqualTo(2));
        Assert.That(container.GetIndexName(), NUnit.Framework.Is.EqualTo("i"));
        Assert.That(container.GetTestAction(0).GetType(), NUnit.Framework.Is.EqualTo(typeof(EchoAction)));
    }
}
