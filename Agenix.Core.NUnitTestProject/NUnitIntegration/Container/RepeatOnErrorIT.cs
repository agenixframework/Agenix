using Agenix.Core.Annotations;
using Agenix.Core.Exceptions;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Container.AssertContainer.Builder;
using static Agenix.Core.Actions.FailAction.Builder;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;
using static Agenix.Core.Container.RepeatOnErrorUntilTrue.Builder;

namespace Agenix.Core.NUnitTestProject.NUnitIntegration.Container;

[NUnitAgenixSupport]
public class RepeatOnErrorIT
{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    [AgenixResource] private IGherkinTestActionRunner _gherkin;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [Test]
    public void RepeatOnErrorContainer()
    {
        _gherkin.Given(CreateVariable("message", "Hello Test Framework!"));

        _gherkin.When(RepeatOnError().Name("Repeat just 1").Until("i = 5").Index("i")
            .Actions(Echo("${i}. Attempt: ${message}")));

        _gherkin.When(RepeatOnError().Name("Repeat just 1 with the sleep of 500 ms").Until("i = 5").AutoSleep(500)
            .Index("i").Actions(Echo("${i}. Attempt: ${message}")));

        _gherkin.When(Assert().Exception(typeof(CoreSystemException))
            .When(
                RepeatOnError().Name("Repeat 3 times")
                    .Until("i = 3")
                    .AutoSleep(200)
                    .Index("i")
                    .Actions(Echo("${i}. Attempt: ${message}"), Fail())
            ));
    }
}