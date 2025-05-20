using Agenix.Api;
using Agenix.Api.Annotations;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;
using static Agenix.Core.Actions.EchoAction.Builder;

namespace Agenix.Core.NUnitTestProject.NUnitIntegration;

[NUnitAgenixSupport]
public class EchoActionNUnitIT
{
    [AgenixResource]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private TestContext context;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [AgenixResource]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private IGherkinTestActionRunner gherkin;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
    [AgenixResource]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private ITestActionRunner runner;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [Test]
    public void EchoTest()
    {
        runner.Run(CreateVariable("time", "core:CurrentDate()"));

        runner.Run(Echo("Hello Agenix!"));

        runner.Run(Echo("CurrentTime is: ${time}"));
    }

    [Test]
    public void EchoGherkinTest()
    {
        gherkin.Given(CreateVariable("time", "core:CurrentDate()"));

        gherkin.When(Echo("Hello Agenix!"));

        gherkin.Then(Echo("CurrentTime is: ${time}"));
    }

    [Test]
    public void ContextInjection()
    {
        context.SetVariable("message", "Injection worked!");

        gherkin.Given(Echo("${message}"));
    }
}