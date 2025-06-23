using Agenix.Api.Annotations;
using Agenix.Api.Log;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using static Agenix.Core.Actions.DefaultTestActionBuilder;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.SleepAction.Builder;
using static Agenix.Core.Actions.StopTimeAction.Builder;
using static Agenix.Core.Actions.TraceVariablesAction.Builder;
using static Agenix.Core.Container.Async.Builder;

namespace Agenix.Core.Tests.NUnitIntegration.Container;

[NUnitAgenixSupport]
[Platform(Exclude = "Linux", Reason = "Only runs on non-Linux platforms.")]
public class AsyncIT
{
    private static readonly ILogger Log = LogManager.GetLogger(typeof(AsyncIT));
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    [AgenixResource] private IGherkinTestActionRunner _gherkin;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [Test]
    public void AsyncContainer()
    {
        _gherkin.When(Async().Actions(
            StopTime(),
            Sleep().Milliseconds(500),
            Echo("Hello Agenix!"),
            StopTime()
        ));

        _gherkin.When(Async().Actions(
            Echo("Hello Agenix!"),
            Action(context => context.SetVariable("anonymous", "anonymous")),
            Sleep().Milliseconds(500),
            Action(context => Log.LogInformation(context.GetVariable("anonymous")))
        ));

        _gherkin.When(Async().Actions(
            StopTime(),
            Sleep().Milliseconds(200),
            Echo("Hello Agenix!"),
            StopTime()
        ));

        _gherkin.When(Async().Actions(
            Echo("Hello Agenix!"),
            Action(context => context.SetVariable("anonymous", "anonymous")),
            Sleep().Milliseconds(200),
            Action(context => Log.LogInformation(context.GetVariable("anonymous")))
        ));

        _gherkin.When(Sleep().Milliseconds(500));

        _gherkin.When(TraceVariables("anonymous"));
    }
}
