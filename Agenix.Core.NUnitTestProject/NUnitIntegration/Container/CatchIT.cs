using Agenix.Api.Annotations;
using Agenix.Api.Exceptions;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using static Agenix.Core.Container.Catch.Builder;
using static Agenix.Core.Actions.FailAction.Builder;

namespace Agenix.Core.NUnitTestProject.NUnitIntegration.Container;

[NUnitAgenixSupport]
public class CatchIT
{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    [AgenixResource] private IGherkinTestActionRunner _gherkin;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [Test]
    public void CatchAction()
    {
        _gherkin.When(CatchException().When(Fail("Fail!")));

        _gherkin.When(CatchException().Exception(typeof(AgenixSystemException)).When(Fail("Fail!")));

        _gherkin.When(CatchException()
            .Exception(nameof(AgenixSystemException))
            .When(Fail("Fail!")));
    }
}