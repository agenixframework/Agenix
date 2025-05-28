namespace Agenix.Screenplay;

/**
 * A task or action that can be performed by an actor.
 * 
 * 
 * It is common to use builder methods to create instances of the Performable class, in order to make the tests read
 * more fluently. For example:
 * 
 * [source,c#]
 * --
 * purchase().anApple().thatCosts(0).dollars()
 * --
 */
public interface IPerformable
{
    /// <summary>
    ///     Performs the specified action or task using the provided actor.
    /// </summary>
    /// <typeparam name="T">The type of actor performing the action.</typeparam>
    /// <param name="actor">The actor that will perform the action or task.</param>
    void PerformAs<T>(T actor) where T : Actor;

    /// <summary>
    ///     Chains the current performable to another performable, creating a composite performable that will execute both in
    ///     sequence.
    /// </summary>
    /// <param name="nextPerformable">The next performable to be executed.</param>
    /// <returns>A composite performable combining the current and the next performable.</returns>
    IPerformable Then(IPerformable nextPerformable)
    {
        return CompositePerformable.From(this, nextPerformable);
    }
}