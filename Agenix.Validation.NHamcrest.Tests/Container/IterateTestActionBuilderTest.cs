using Agenix.Core;
using Agenix.Core.Container;
using NUnit.Framework.Legacy;
using Is = NHamcrest.Is;
using static Agenix.Core.Container.Iterate.Builder;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;
using static Agenix.Validation.NHamcrest.Container.NHamcrestConditionExpression;

namespace Agenix.Validation.NHamcrest.Tests.Container;

public class IterateTestActionBuilderTest : AbstractNUnitSetUp
{
    [Test]
    public void TestIterateBuilderWithHamcrestConditionExpression()
    {
        var builder = new DefaultTestCaseRunner(Context);
        builder.Run(Iterate()
            .Step(1)
            .StartsWith(0)
            .Condition(AssertThat(Is.LessThan(5)).AsIteratingCondition())
            .Actions(CreateVariable("index", "${i}")));

        ClassicAssert.NotNull(Context.GetVariable("i"));
        ClassicAssert.AreEqual("4", Context.GetVariable("i"));

        var test = builder.GetTestCase();
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.AreEqual(typeof(Iterate), test.GetActions()[0].GetType());
        ClassicAssert.AreEqual("iterate", test.GetActions()[0].Name);

        var container = (Iterate)test.GetActions()[0];
        ClassicAssert.AreEqual(1, container.GetActionCount());
        ClassicAssert.AreEqual("i", container.GetIndexName());
        ClassicAssert.AreEqual(1, container.GetStep());
        ClassicAssert.AreEqual(0, container.GetStart());
    }
}