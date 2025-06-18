using System;
using Agenix.Api.Annotations;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using static Agenix.Core.Container.Sequence.Builder;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.SleepAction.Builder;
using static Agenix.Core.Actions.StopTimeAction.Builder;
using static Agenix.Core.Actions.DefaultTestActionBuilder;

namespace Agenix.Core.Tests.NUnitIntegration.Container;

[NUnitAgenixSupport]
public class SequentialIT
{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    [AgenixResource] private IGherkinTestActionRunner _gherkin;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [Test]
    public void SequentialContainer()
    {
        _gherkin.When(Sequential().Actions(
            StopTime(),
            Sleep().Milliseconds(500),
            Echo("Hello Agenix!"),
            StopTime()
        ));

        _gherkin.When(Sequential().Actions(
            Echo("Hello Agenix!"),
            Action(context => context.SetVariable("anonymous", "anonymous")),
            Sleep().Milliseconds(500),
            Action(context => Console.WriteLine(context.GetVariable("anonymous")))
        ));

        _gherkin.When(Sequential().Actions(
            StopTime(),
            Sleep().Milliseconds(200),
            Echo("Hello Agenix!"),
            StopTime()
        ));

        _gherkin.When(Sequential().Actions(
            Echo("Hello Agenix!"),
            Action(context => context.SetVariable("anonymous", "anonymous")),
            Sleep().Milliseconds(200),
            Action(context => Console.WriteLine(context.GetVariable("anonymous")))
        ));
    }
}
