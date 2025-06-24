using Agenix.Core;
using Agenix.Core.Container;
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

        Assert.That(Context.GetVariable("noExecution"), NUnit.Framework.Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Context.GetVariable("noExecution"), NUnit.Framework.Is.EqualTo("true"));
            Assert.That(Context.GetVariable("execution"), NUnit.Framework.Is.Not.Null);
        }
        Assert.That(Context.GetVariable("execution"), NUnit.Framework.Is.EqualTo("true"));

        var test = builder.GetTestCase();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.GetActionCount(), NUnit.Framework.Is.EqualTo(2));
            Assert.That(test.GetActions()[0].GetType(), NUnit.Framework.Is.EqualTo(typeof(Conditional)));
            Assert.That(test.GetActions()[0].Name, NUnit.Framework.Is.EqualTo("conditional"));
            Assert.That(test.GetActions()[1].GetType(), NUnit.Framework.Is.EqualTo(typeof(Conditional)));
            Assert.That(test.GetActions()[1].Name, NUnit.Framework.Is.EqualTo("conditional"));
        }

        var container = (Conditional)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(container.GetActionCount(), NUnit.Framework.Is.EqualTo(2));
            Assert.That(container.ConditionExpression, NUnit.Framework.Is.Not.Null);
        }
    }
}
