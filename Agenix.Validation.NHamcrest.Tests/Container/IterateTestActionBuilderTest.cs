using Agenix.Core;
using Agenix.Core.Container;
using NUnit.Framework.Legacy;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;
using static Agenix.Core.Container.Iterate.Builder;
using static Agenix.Validation.NHamcrest.Container.NHamcrestConditionExpression;
using Is = NHamcrest.Is;

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
        Assert.That(Context.GetVariable("i"), NUnit.Framework.Is.EqualTo("4"));

        var test = builder.GetTestCase();
        Assert.That(test.GetActionCount(), NUnit.Framework.Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), NUnit.Framework.Is.EqualTo(typeof(Iterate)));
        Assert.That(test.GetActions()[0].Name, NUnit.Framework.Is.EqualTo("iterate"));

        var container = (Iterate)test.GetActions()[0];
        Assert.That(container.GetActionCount(), NUnit.Framework.Is.EqualTo(1));
        Assert.That(container.GetIndexName(), NUnit.Framework.Is.EqualTo("i"));
        Assert.That(container.GetStep(), NUnit.Framework.Is.EqualTo(1));
        Assert.That(container.GetStart(), NUnit.Framework.Is.EqualTo(0));
    }
}
