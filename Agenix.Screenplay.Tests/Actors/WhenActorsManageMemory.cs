using Agenix.Screenplay.Questions;
using NUnit.Framework;

namespace Agenix.Screenplay.Tests.Actors;

public class WhenActorsRememberThings
{
    [Test]
    public async Task AnActorCanRememberInformation()
    {
        var actor = Actor.Named("Archie");

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

        await actor.Remember("TOTAL_COST", TotalCost());

        var totalCost = await actor.Recall<int>("TOTAL_COST");

        Assert.That(totalCost, Is.EqualTo(100));
    }

    [Test]
    public async Task AnActorCanForgetWhatTheyKnow()
    {
        var actor = Actor.Named("Archie");

        await actor.Remember("TOTAL_COST", TotalCost());

        await actor.Forget("TOTAL_COST");

        var totalCost = await actor.Recall<int?>("TOTAL_COST");

        Assert.That(totalCost, Is.Null);
    }

    [Test]
    public async Task AnActorCanRecallEverythingTheyKnow()
    {
        var actor = Actor.Named("Archie");

        await actor.Remember("COLOR", "Red");
        await actor.Remember("FLAVOUR", "Vanilla");

        var memory = actor.RecallAll();

        Assert.That(memory["COLOR"], Is.EqualTo("Red"));
        Assert.That(memory["FLAVOUR"], Is.EqualTo("Vanilla"));
    }
}
