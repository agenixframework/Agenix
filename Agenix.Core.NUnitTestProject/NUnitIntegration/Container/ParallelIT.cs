using Agenix.Core.Annotations;
using Agenix.Core.Exceptions;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using static Agenix.Core.Container.Parallel.Builder;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.SleepAction.Builder;
using static Agenix.Core.Container.Iterate.Builder;
using static Agenix.Core.Container.Sequence.Builder;
using static Agenix.Core.Container.AssertContainer.Builder;
using static Agenix.Core.Actions.FailAction.Builder;

namespace Agenix.Core.NUnitTestProject.NUnitIntegration.Container;

[NUnitAgenixSupport]
public class ParallelIT
{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    [AgenixResource] private IGherkinTestActionRunner _gherkin;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [Test]
    public void ParallelContainer()
    {
        _gherkin.When(Parallel().Actions(
            Sleep().Milliseconds(150),
            Sequential().Actions(
                Sleep().Milliseconds(100),
                Echo("1")
            ),
            Echo("2"),
            Echo("3"),
            Iterate()
                .Condition("i lt= 5").Index("i").Actions(Echo("10"))
        ));

        _gherkin.When(Assert().Exception(typeof(CoreSystemException))
            .When(
                Parallel().Actions(
                    Sleep().Milliseconds(150),
                    Sequential().Actions(
                        Sleep().Milliseconds(100),
                        Fail("This went wrong too"),
                        Echo("1")
                    ),
                    Echo("2"),
                    Fail("This went wrong too"),
                    Echo("3"),
                    Iterate()
                        .Condition("i lt= 5").Index("i").Actions(Echo("10"))
                )));
    }
}