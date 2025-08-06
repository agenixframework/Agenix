using Agenix.Api.Annotations;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using Agenix.Screenplay.Abilities;
using Agenix.Screenplay.Questions;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Screenplay.Tests.Actors;

[NUnitAgenixSupport]
public class WhenActorsRememberThingsUsingTestContext
{
    [AgenixResource] private TestContext _testContext;

    [Test]
    public async Task AnActorCanRememberInformation()
    {
        var actor = Actor.Named("Archie");
        actor.WhoCan(new UseTheAgenixTestContext(_testContext));

        await actor.Remember("TOTAL_COST", 100);

        var totalCost = await actor.Recall<int>("TOTAL_COST");

        Assert.That(totalCost, Is.EqualTo(100));
    }

    private static Question<int> TotalCost()
    {
        return new Question<int>(_ => 100);
    }

    [Test]
    public async Task AnActorCanRememberTheAnswerToAQuestion()
    {
        var actor = Actor.Named("Archie");
        actor.WhoCan(new UseTheAgenixTestContext(_testContext));

        await actor.Remember("TOTAL_COST", TotalCost());

        var totalCost = await actor.Recall<int>("TOTAL_COST");

        Assert.That(totalCost, Is.EqualTo(100));
    }

    [Test]
    public async Task AnActorCanForgetWhatTheyKnow()
    {
        var actor = Actor.Named("Archie");
        actor.WhoCan(new UseTheAgenixTestContext(_testContext));

        await actor.Remember("TOTAL_COST", TotalCost());

        await actor.Forget("TOTAL_COST");

        var totalCost = await actor.Recall<int?>("TOTAL_COST");

        Assert.That(totalCost, Is.Null);
    }

    [Test]
    public async Task AnActorCanRecallEverythingTheyKnow()
    {
        var actor = Actor.Named("Archie");
        actor.WhoCan(new UseTheAgenixTestContext(_testContext));

        await actor.Remember("COLOR", "Red");
        await actor.Remember("FLAVOUR", "Vanilla");

        var memory = actor.RecallAll();

        Assert.That(memory["COLOR"], Is.EqualTo("Red"));
        Assert.That(memory["FLAVOUR"], Is.EqualTo("Vanilla"));
    }
}
