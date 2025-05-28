namespace Agenix.Screenplay;

/// <summary>
/// An interaction that removes a value from an actor's memory.
/// </summary>
public class Forget : Interaction
{
    private readonly string _memoryKey;

    private Forget(string memoryKey)
    {
        _memoryKey = memoryKey;
    }

    public void PerformAs<T>(T actor) where T : Actor
    {
        actor.Forget<dynamic>(_memoryKey);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Forget"/> interaction for removing a value associated with the specified key from an actor's memory.
    /// </summary>
    /// <param name="memoryKey">The key identifying the value in the actor's memory to be forgotten.</param>
    /// <returns>A new instance of the <see cref="Forget"/> interaction configured with the provided memory key.</returns>
    public static Forget TheValueOf(string memoryKey)
    {
        return new Forget(memoryKey);
    }
}
