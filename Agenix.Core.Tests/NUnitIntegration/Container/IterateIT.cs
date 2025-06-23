using System;
using Agenix.Api.Annotations;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;
using static Agenix.Core.Actions.DefaultTestActionBuilder;
using static Agenix.Core.Actions.EchoAction.Builder;
using static Agenix.Core.Container.Iterate.Builder;
using ITestAction = Agenix.Api.ITestAction;

namespace Agenix.Core.Tests.NUnitIntegration.Container;

[NUnitAgenixSupport]
public class IterateIT
{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    [AgenixResource] private IGherkinTestActionRunner _gherkin;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [Test]
    public void IterateContainer()
    {
        _gherkin.Given(CreateVariable("max", "3"));

        _gherkin.When(Iterate().Condition("i lt= agenix:RandomNumber(1)").Index("i").Actions(Echo("index is: ${i}")));

        _gherkin.When(Iterate().Condition("i lt 20").Index("i").Actions(Echo("index is: ${i}")));

        _gherkin.When(Iterate().Condition("(i lt 5) or (i = 5)").Index("i").Actions(Echo("index is: ${i}")));

        _gherkin.When(Iterate().Condition("(i lt 5) and (i lt 3)").Index("i").Actions(Echo("index is: ${i}")));

        _gherkin.When(Iterate().Condition("i = 0").Index("i").Actions(Echo("index is: ${i}")));

        _gherkin.When(Iterate().Condition("${max} gt= i").Index("i").Actions(Echo("index is: ${i}")));

        var anonymous = Action(context => Console.WriteLine(context.GetVariable("index"))).Build();

        _gherkin.When(Iterate().Condition("i lt 5").Index("i")
            .Actions(CreateVariable("index", "${i}"), new FuncITestActionBuilder<ITestAction>(() => anonymous)));
    }
}
