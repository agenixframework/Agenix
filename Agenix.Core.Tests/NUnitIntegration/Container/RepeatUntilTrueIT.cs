using Agenix.Api.Annotations;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Container.RepeatUntilTrue.Builder;

namespace Agenix.Core.Tests.NUnitIntegration.Container;

[NUnitAgenixSupport]
public class RepeatUntilTrueIT
{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    [AgenixResource] private IGherkinTestActionRunner _gherkin;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [Test]
    public void RepeatContainer()
    {
        _gherkin.Given(CreateVariable("max", "3"));

        _gherkin.When(Repeat().Until("i gt agenix:RandomNumber(1)").Index("i").Actions(Echo("index is: ${i}")));

        _gherkin.When(Repeat().Until("i gt= 5").Index("i").Actions(Echo("index is: ${i}")));

        _gherkin.When(Repeat().Until("(i gt 5) or (i = 5)").Index("i").Actions(Echo("index is: ${i}")));

        _gherkin.When(Repeat().Until("(i gt 5) and (i gt 3)").Index("i").Actions(Echo("index is: ${i}")));

        _gherkin.When(Repeat().Until("i gt 0").Index("i").Actions(Echo("index is: ${i}")));

        _gherkin.When(Repeat().Until("${max} lt i").Index("i").Actions(Echo("index is: ${i}")));
    }
}
