namespace Agenix.Screenplay;

/// <summary>
/// Represents an entity that can refer to an actor to perform actions using a specific ability.
/// </summary>
public interface IRefersToActor
{
    /// <summary>
    /// Provides the ability to use an actor to perform an action associated
    /// with the specified ability interface.
    /// </summary>
    /// <typeparam name="T">The type of ability to act as, which must implement IAbility.</typeparam>
    /// <param name="actor">The actor who is performing the associated action.</param>
    /// <returns>An instance of the specified ability type associated with the actor.</returns>
    T AsActor<T>(Actor actor) where T : IAbility;
}