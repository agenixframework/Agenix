using Agenix.Core.Annotations;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using static Agenix.Core.Container.FinallySequence.Builder;
using static Agenix.Core.Container.Timer.Builder;
using static Agenix.Core.Actions.StopTimerAction.Builder;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.SleepAction.Builder;

namespace Agenix.Core.NUnitTestProject.NUnitIntegration.Container;

[NUnitAgenixSupport]
public class TimerIT
{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    [AgenixResource] private IGherkinTestActionRunner _gherkin;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [Test]
    public void TimerTest()
    {
        _gherkin.Given(DoFinally().Actions(StopTimer("forkedTimer")));

        _gherkin.When(Timer()
            .TimerId("forkedTimer")
            .Interval(100)
            .Fork(true)
            .Actions(
                Echo(
                    "I'm going to run in the background and let some other test actions run (nested action run ${forkedTimer-index} times)"),
                Sleep().Milliseconds(50)
            ));

        _gherkin.When(Timer()
            .RepeatCount(3)
            .Interval(100)
            .Delay(50)
            .Actions(
                Sleep().Milliseconds(50),
                Echo(
                    "I'm going to repeat this message 3 times before the next test actions are executed")
            ));
        _gherkin.Then(Echo(
            "Test almost complete. Make sure all timers running in the background are stopped"));
    }
}