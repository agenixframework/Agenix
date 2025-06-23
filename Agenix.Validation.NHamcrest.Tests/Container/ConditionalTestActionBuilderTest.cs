using Agenix.Core;
using Agenix.Core.Container;
using NUnit.Framework.Legacy;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Container.Conditional.Builder;
using static Agenix.Validation.NHamcrest.Container.NHamcrestConditionExpression;
using Is = NHamcrest.Is;

namespace Agenix.Validation.NHamcrest.Tests.Container;

public class ConditionalTestActionBuilderTest : AbstractNUnitSetUp
{
    [Test]
    public void TestConditionalBuilderHamcrestConditionExpression()
    {
        var builder = new DefaultTestCaseRunner(Context);
        builder.SetVariable("var", 5);
        builder.SetVariable("noExecution", "true");

        builder.Run(Conditional().When(AssertThat("${var}", Is.EqualTo("5")).AsCondition())
            .Actions(Echo("${var}"), CreateVariable("execution", "true")));

        builder.Run(Conditional().When(AssertThat("${var}", Is.LessThan("5")).AsCondition())
            .Actions(Echo("${var}"), CreateVariable("noExecution", "false")));

        ClassicAssert.IsNotNull(Context.GetVariable("noExecution"));
        Assert.That("true", NUnit.Framework.Is.EqualTo(Context.GetVariable("noExecution")));
        ClassicAssert.IsNotNull(Context.GetVariable("execution"));
        Assert.That("true", NUnit.Framework.Is.EqualTo(Context.GetVariable("execution")));

        var test = builder.GetTestCase();
        Assert.That(2, NUnit.Framework.Is.EqualTo(test.GetActionCount()));
        Assert.That(typeof(Conditional), NUnit.Framework.Is.EqualTo(test.GetActions()[0].GetType()));
        Assert.That("conditional", NUnit.Framework.Is.EqualTo(test.GetActions()[0].Name));
        Assert.That(typeof(Conditional), NUnit.Framework.Is.EqualTo(test.GetActions()[1].GetType()));
        Assert.That("conditional", NUnit.Framework.Is.EqualTo(test.GetActions()[1].Name));

        var container = (Conditional)test.GetActions()[0];
        Assert.That(2, NUnit.Framework.Is.EqualTo(container.GetActionCount()));
        ClassicAssert.IsNotNull(container.ConditionExpression);
    }
}
