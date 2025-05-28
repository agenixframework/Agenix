namespace Agenix.Screenplay;

/// <summary>
/// Represents an anonymous performable action or task to be executed by an actor in the screenplay pattern.
/// </summary>
/// <remarks>
/// This class allows for the definition of actions or tasks via anonymous functions, providing flexibility in describing custom behaviors for an actor.
/// </remarks>
public class AnonymousPerformableFunction(Action<Actor> actions) : IPerformable
{
    public void PerformAs<T>(T actor) where T : Actor
    {
        actions(actor);
    }
    
}
