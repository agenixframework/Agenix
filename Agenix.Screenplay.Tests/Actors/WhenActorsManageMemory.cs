using Agenix.Screenplay.Questions;
using NUnit.Framework;

namespace Agenix.Screenplay.Tests.Actors;

public class WhenActorsRememberThings
{
    [Test]
    public void AnActorCanRememberInformation()
    {
        var actor = Actor.Named("Archie");

        actor.Remember("TOTAL_COST", 100);

        var totalCost = actor.Recall<int>("TOTAL_COST");

        Assert.That(totalCost, Is.EqualTo(100));
    }

    private static Question<int> TotalCost()
    {
        return new Question<int>(_ => 100);
    }

    [Test]
    public void AnActorCanRememberTheAnswerToAQuestion()
    {
        var actor = Actor.Named("Archie");

        actor.Remember("TOTAL_COST", TotalCost());

        var totalCost = actor.Recall<int>("TOTAL_COST");

        Assert.That(totalCost, Is.EqualTo(100));
    }

    [Test]
    public void AnActorCanForgetWhatTheyKnow()
    {
        var actor = Actor.Named("Archie");

        actor.Remember("TOTAL_COST", TotalCost());

        actor.Forget("TOTAL_COST");

        var totalCost = actor.Recall<int?>("TOTAL_COST");

        Assert.That(totalCost, Is.Null);
    }

    [Test]
    public void AnActorCanRecallEverythingTheyKnow()
    {
        var actor = Actor.Named("Archie");

        actor.Remember("COLOR", "Red");
        actor.Remember("FLAVOUR", "Vanilla");

        var memory = actor.RecallAll();

        Assert.That(memory["COLOR"], Is.EqualTo("Red"));
        Assert.That(memory["FLAVOUR"], Is.EqualTo("Vanilla"));
    }
}
