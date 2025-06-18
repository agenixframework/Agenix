using Agenix.Core;
using Agenix.Core.Container;
using NUnit.Framework.Legacy;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;
using static Agenix.Core.Container.Conditional.Builder;
using Is = NHamcrest.Is;
using static Agenix.Validation.NHamcrest.Container.NHamcrestConditionExpression;

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
        ClassicAssert.AreEqual(Context.GetVariable("noExecution"), "true");
        ClassicAssert.IsNotNull(Context.GetVariable("execution"));
        ClassicAssert.AreEqual(Context.GetVariable("execution"), "true");

        var test = builder.GetTestCase();
        ClassicAssert.AreEqual(test.GetActionCount(), 2);
        ClassicAssert.AreEqual(test.GetActions()[0].GetType(), typeof(Conditional));
        ClassicAssert.AreEqual(test.GetActions()[0].Name, "conditional");
        ClassicAssert.AreEqual(test.GetActions()[1].GetType(), typeof(Conditional));
        ClassicAssert.AreEqual(test.GetActions()[1].Name, "conditional");

        var container = (Conditional)test.GetActions()[0];
        ClassicAssert.AreEqual(container.GetActionCount(), 2);
        ClassicAssert.IsNotNull(container.ConditionExpression);
    }
}
