using System.IO;
using Agenix.Core.Annotations;
using Agenix.Core.Exceptions;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;
using static Agenix.Core.Actions.FailAction.Builder;
using static Agenix.Core.Container.AssertContainer.Builder;
using static Agenix.Core.Actions.DefaultTestActionBuilder;

namespace Agenix.Core.NUnitTestProject.NUnitIntegration.Container;

[NUnitAgenixSupport]
public class AssertIT
{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    [AgenixResource] private IGherkinTestActionRunner _gherkin;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [Test]
    public void AssertAction()
    {
        _gherkin.Given(CreateVariable("failMessage", "Something went wrong!"));

        _gherkin.Then(Assert().Exception(typeof(CoreSystemException)).When(Fail("Fail once")));

        _gherkin.Then(Assert().Exception(typeof(CoreSystemException))
            .Message("Fail again")
            .When(Fail("Fail again")));

        _gherkin.Then(Assert().Exception(typeof(CoreSystemException))
            .Message("${failMessage}")
            .When(Fail("${failMessage}")));

        _gherkin.Then(Assert().Exception(typeof(CoreSystemException))
            .Message("@Contains('wrong')@")
            .When(Fail("${failMessage}")));

        _gherkin.Then(Assert().Exception(typeof(ValidationException))
            .When(Assert().Exception(typeof(IOException))
                .When(Fail("Fail another time"))));

        _gherkin.Then(Assert().Exception(typeof(ValidationException))
            .When(Assert().Exception(typeof(CoreSystemException))
                .Message("Fail again")
                .When(Fail("Fail with nice error message"))));

        _gherkin.Then(Assert().Exception(typeof(ValidationException))
            .When(Assert().Exception(typeof(CoreSystemException))
                .Message("Fail again")
                .When(Echo("Nothing fails here"))));

        _gherkin.Then(Assert().Exception(typeof(CoreSystemException))
            .Message("Unknown variable 'foo'")
            .When(Action(context => context.GetVariable("foo"))));
    }
}