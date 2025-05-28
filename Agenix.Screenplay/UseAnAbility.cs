using Agenix.Screenplay.Exceptions;

namespace Agenix.Screenplay;

/// <summary>
/// A factory to get an Ability of an Actor, when we don't care what it is specifically, just
/// that it can do what we expect it to.
/// Example Usage:
/// UseAnAbility.Of(actor).That&lt;ICanLevitate&gt;().Hover()
/// </summary>
public class UseAnAbility
{
    private readonly Actor _actor;

    private UseAnAbility(Actor actor)
    {
        _actor = actor;
    }

    /// <summary>
    /// Creates an instance of the UseAnAbility class for the specified actor, allowing access to abilities the actor possesses.
    /// </summary>
    /// <param name="actor">The actor whose abilities are to be accessed.</param>
    /// <returns>An instance of the UseAnAbility class associated with the specified actor.</returns>
    public static UseAnAbility Of(Actor actor)
    {
        return new UseAnAbility(actor);
    }

    /// <summary>
    /// If the actor has an Ability that implements the Interface, return that. If
    /// there are multiple candidate Abilities, the first one found will be returned.
    /// </summary>
    /// <typeparam name="T">The interface type that we expect to find</typeparam>
    /// <returns>The Ability that implements the interface</returns>
    /// <exception cref="NoMatchingAbilityException">Thrown when no matching ability is found</exception>
    public T That<T>() where T : class
    {
        var ability = _actor.GetAbilityThatExtends<T>();

        if (ability == null)
        {
            throw new NoMatchingAbilityException(
                $"{_actor} does not have an Ability that extends {typeof(T).Name}");
        }
        return ability;
    }
}
