namespace Agenix.Screenplay;

/// <summary>
/// Represents an anonymous, executable task or action that can be performed by an actor within the screenplay pattern.
/// </summary>
/// <remarks>
/// The AnonymousPerformableRunnable class provides a way to define performable tasks or actions
/// inline using a delegate. It encapsulates an action that can be executed by any actor.
/// This supports concise and flexible task definition to enhance the readability of actor behaviors.
/// </remarks>
/// <example>
/// This class is typically used when you want to define a performable action inline for simplicity.
/// </example>
public class AnonymousPerformableRunnable(Action actions) : IPerformable
{
    public void PerformAs<T>(T actor) where T : Actor
    {
        actions();
    }
}
