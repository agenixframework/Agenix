using Agenix.Core.Annotations;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Actions.ApplyTestBehaviorAction.Builder;
using static Agenix.Core.Container.Sequence.Builder;

namespace Agenix.Core.NUnitTestProject.NUnitIntegration;

[NUnitAgenixSupport]
public class ApplyTestBehaviorIT
{
    [AgenixResource]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private ITestActionRunner _runner;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [Test]
    public void ShouldApply()
    {
        _runner.Run(Apply(new SayHelloBehavior()).Name("Apply runner.Apply(...)"));

        _runner.Run(_runner.ApplyBehavior(new SayHelloBehavior("Hi!")).Name("runner.ApplyBehavior(...)"));
    }

    [Test]
    public void ShouldApplyInContainer()
    {
        _runner.Run(Sequential()
            .Name("Sequential Container")
            .Description("A container that runs actions in a sequential order")
            .Actions(
                Echo("In Germany they say:"),
                Apply().Behavior(new SayHelloBehavior("Hallo")).On(_runner),
                Echo("In Spain they say:"),
                _runner.ApplyBehavior(new SayHelloBehavior("Hola"))
            ));
    }


    private class SayHelloBehavior(string greeting) : ITestBehavior
    {
        public SayHelloBehavior() : this("Hello")
        {
        }

        public void Apply(ITestActionRunner runner)
        {
            runner.Run(Echo($"{greeting} Agenix!"));
        }
    }
}