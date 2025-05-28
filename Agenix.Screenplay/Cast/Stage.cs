namespace Agenix.Screenplay.Cast;

/// <summary>
///     The stage keeps track of the actors currently in a screenplay test, referenced by name.
///     You rarely need to use this class directly in your tests. Normally you would use the OnStage class instead.
/// </summary>
public class Stage
{
    private readonly Casting _cast;
    private Actor _actorInTheSpotlight;

    public Stage(Casting cast)
    {
        _cast = cast;
    }

    /// <summary>
    ///     Place an actor with a given name in the spotlight, without the intent to have them perform an action at this time.
    /// </summary>
    public Actor ShineSpotlightOn(string actorName)
    {
        var knownActor = _cast.GetActors()
            .FirstOrDefault(actor => actor.Name.Equals(actorName, StringComparison.OrdinalIgnoreCase));

        _actorInTheSpotlight = knownActor ?? _cast.ActorNamed(actorName);
        return TheActorInTheSpotlight().WithNoPronoun();
    }

    /// <summary>
    ///     Return the current actor in the spotlight.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when there is no actor in the spotlight.</exception>
    public Actor TheActorInTheSpotlight()
    {
        if (_actorInTheSpotlight == null)
            throw new ArgumentNullException(nameof(_actorInTheSpotlight), "No actor is currently in the spotlight");

        return _actorInTheSpotlight;
    }

    /// <summary>
    ///     A shortened form of TheActorInTheSpotlight()
    /// </summary>
    public Actor TheActor()
    {
        return TheActorInTheSpotlight();
    }

    /// <summary>
    ///     This method should be called at the end of the screenplay test to perform teardown actions on each actor.
    ///     It will generally be done automatically by the test framework.
    /// </summary>
    public void DrawTheCurtain()
    {
        _cast.DismissAll();
    }

    /// <summary>
    ///     Check whether there is any actor in the spotlight at the moment.
    /// </summary>
    public bool AnActorIsOnStage()
    {
        return _actorInTheSpotlight != null;
    }
}