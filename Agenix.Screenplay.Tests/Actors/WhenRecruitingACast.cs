using Agenix.Screenplay.Cast;
using NSubstitute;
using NUnit.Framework;

namespace Agenix.Screenplay.Tests.Actors;

public class WhenRecruitingACast
{
    [Test]
    public void ACastCanProvideActorsByName()
    {
        // Given
        var cast = Casting.OfStandardActors();

        // When
        var actor = cast.ActorNamed("Joe");

        // Then
        Assert.That(actor.Name, Is.EqualTo("Joe"));
    }

    [Test]
    public void CastMembersCanBeTrained()
    {
        // Given
        var performShakespeare = new PerformShakespeare();

        // And
        var globeTheatreCast = Casting.WhereEveryoneCan(performShakespeare);

        // When
        var laurence = globeTheatreCast.ActorNamed("Laurence");

        // Then
        Assert.That(laurence.AbilityTo<PerformShakespeare>(), Is.SameAs(performShakespeare),
            "Actor should have the ability to perform Shakespeare");
    }

    [Test]
    public void CastMembersCanBeTrainedToDoArbitraryThings()
    {
        // Given
        Action<Actor> fetchTheCoffee = actor => actor.WhoCan(Fetch.Some("Coffee"));
        var globeTheatreCast = Casting.WhereEveryoneCan(fetchTheCoffee);

        // When
        var kenneth = globeTheatreCast.ActorNamed("Kenneth");

        // Then
        Assert.That(kenneth.AbilityTo<Fetch>().Item, Is.EqualTo("Coffee"),
            "Actor should be able to fetch coffee");
    }

    [Test]
    public void CastMembersCanTidyUpAfterThemselves()
    {
        // Given
        var performHamlet = Substitute.For<PerformHamlet>();
        OnStage.SetTheStage(Casting.WhereEveryoneCan(performHamlet));
        OnStage.TheActorCalled("Laurence");

        // When
        OnStage.DrawTheCurtain();

        // Then
        performHamlet.Received(1).TearDown();
    }

    [Test]
    public void TheActorIsAShorterFormOfTheActorCalled()
    {
        // Given
        var performHamlet = Substitute.For<PerformHamlet>();
        OnStage.SetTheStage(Casting.WhereEveryoneCan(performHamlet));
        OnStage.TheActor("Laurence");

        // When
        OnStage.DrawTheCurtain();

        // Then
        performHamlet.Received(1).TearDown();
    }

    [Test]
    public void CastMembersCanBeUsedBeforeKnowingTheirNames()
    {
        // Given
        OnStage.SetTheStage(Casting.OfStandardActors());
        var theNextActor = OnStage.ANewActor();

        // When
        var kenneth = OnStage.TheActorCalled("Kenneth");

        // Then
        Assert.That(kenneth, Is.SameAs(theNextActor),
            "The previously created unnamed actor should be the same instance as the named actor");
    }

    [Test]
    public void WhenACastMemberIsCalledTheyMoveIntoTheSpotlight()
    {
        // Given
        OnStage.SetTheStage(Casting.OfStandardActors());
        var kenneth = OnStage.TheActorCalled("Kenneth");

        // When
        var inTheSpotlight = OnStage.TheActorInTheSpotlight();

        // Then
        Assert.That(inTheSpotlight, Is.SameAs(kenneth),
            "The actor in the spotlight should be the last called actor");
    }

    [Test]
    public void WeCanSetANewStage()
    {
        // Given
        var oldStage = OnStage.SetTheStage(Casting.OfStandardActors());
        var newStage = new Stage(Casting.OfStandardActors());

        // When
        OnStage.SetTheStage(newStage);

        // Then
        Assert.That(OnStage.GetStage(), Is.SameAs(newStage),
            "The current stage should be the newly set stage");
    }


    public class PerformShakespeare : IAbility
    {
    }

    public class Fetch : IAbility
    {
        private Fetch(string item)
        {
            Item = item;
        }

        public string Item { get; }

        public static Fetch Some(string item)
        {
            return new Fetch(item);
        }
    }


    public class PerformHamlet : IAbility, IHasTeardown
    {
        public void TearDown()
        {
        }
    }
}
