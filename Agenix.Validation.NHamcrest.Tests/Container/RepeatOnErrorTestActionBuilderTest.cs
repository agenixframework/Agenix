using Agenix.Core;
using Agenix.Core.Actions;
using Agenix.Core.Container;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.SleepAction.Builder;
using static Agenix.Core.Container.RepeatOnErrorUntilTrue.Builder;
using static Agenix.Validation.NHamcrest.Container.NHamcrestConditionExpression;
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Context.GetVariable("i"), NUnit.Framework.Is.Not.Null);
            Assert.That("1", NUnit.Framework.Is.EqualTo(Context.GetVariable("i")));
            Assert.That(Context.GetVariable("k"), NUnit.Framework.Is.Not.Null);
            Assert.That("2", NUnit.Framework.Is.EqualTo(Context.GetVariable("k")));
        }

        var test = builder.GetTestCase();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(2, NUnit.Framework.Is.EqualTo(test.GetActionCount()));
            Assert.That(typeof(RepeatOnErrorUntilTrue), NUnit.Framework.Is.EqualTo(test.GetActions()[0].GetType()));
            Assert.That("repeat-on-error", NUnit.Framework.Is.EqualTo(test.GetActions()[0].Name));
        }

        var container = (RepeatOnErrorUntilTrue)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(3, NUnit.Framework.Is.EqualTo(container.GetActionCount()));
            Assert.That(250L, NUnit.Framework.Is.EqualTo(container.AutoSleep));
            Assert.That("i gt 5", NUnit.Framework.Is.EqualTo(container.Condition));
            Assert.That(1, NUnit.Framework.Is.EqualTo(container.GetStart()));
            Assert.That("i", NUnit.Framework.Is.EqualTo(container.GetIndexName()));
            Assert.That(typeof(EchoAction), NUnit.Framework.Is.EqualTo(container.GetTestAction(0).GetType()));
        }

        container = (RepeatOnErrorUntilTrue)test.GetActions()[1];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(1, NUnit.Framework.Is.EqualTo(container.GetActionCount()));
            Assert.That(200L, NUnit.Framework.Is.EqualTo(container.AutoSleep));
            Assert.That(2, NUnit.Framework.Is.EqualTo(container.GetStart()));
            Assert.That("k", NUnit.Framework.Is.EqualTo(container.GetIndexName()));
            Assert.That(typeof(EchoAction), NUnit.Framework.Is.EqualTo(container.GetTestAction(0).GetType()));
        }
    }
}
