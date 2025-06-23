using Agenix.Api;
using Agenix.Api.Annotations;
using Agenix.Api.Spi;
using Agenix.Api.Variable;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using static Agenix.Core.Actions.EchoAction.Builder;

namespace Agenix.Core.Tests.NUnitIntegration;

[NUnitAgenixSupport]
public class GlobalVariablesIT
{
    [AgenixResource]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private ITestActionRunner _runner;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [BindToRegistry(Name = "globalVariable")]
    public GlobalVariables GlobalVariables()
    {
        return new GlobalVariables.Builder()
            .WithVariable("globalWelcomingText", "globalVariableValue")
            .WithVariable("project.name", "Agenix")
            .Build();
    }

    [Test]
    public void GlobalProperties()
    {
        _runner.Run(Echo("Project name is: ${project.name}"));

        _runner.Run(Echo("Testing global variables: ${globalWelcomingText}"));
    }
}
