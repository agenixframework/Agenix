namespace Agenix.Screenplay;

/// <summary>
/// Represents an actionable behavior or operation that can be performed within a screenplay context.
/// </summary>
public interface Interaction : IPerformable
{
    /// <summary>
    /// Creates an instance of <see cref="AnonymousInteraction"/> that encapsulates a performable operation with a specified title and a list of steps.
    /// </summary>
    /// <param name="title">The descriptive title of the interaction, representing its purpose or intent.</param>
    /// <param name="steps">An array of <see cref="IPerformable"/> tasks composing the interaction.</param>
    /// <returns>An instance of <see cref="AnonymousInteraction"/> configured with the specified title and steps.</returns>
    public static AnonymousInteraction Where(string title, params IPerformable[] steps)
    {
        return Instrumented.InstanceOf<AnonymousInteraction>()
            .WithProperties(title, steps.ToList());
    }

    /// <summary>
    /// Creates an instance of <see cref="AnonymousInteraction"/> that encapsulates a performable operation with a specified title and operation.
    /// </summary>
    /// <param name="title">The title of the interaction, representing its purpose or description.</param>
    /// <param name="performableOperation">The performable operation defined as an action involving an <see cref="Actor"/>.</param>
    /// <returns>An instance of <see cref="AnonymousInteraction"/> configured with the specified title and performable operation.</returns>
    public static AnonymousInteraction Where(string title, Action<Actor> performableOperation)
    {
        return Instrumented.InstanceOf<AnonymousInteraction>()
            .WithProperties(title, performableOperation);
    }

    /// <summary>
    /// Creates an instance of <see cref="AnonymousInteraction"/> that encapsulates a performable operation with a specified title.
    /// </summary>
    /// <param name="title">The title of the interaction, representing its purpose or description.</param>
    /// <param name="performableOperation">The performable operation to be executed as part of the interaction.</param>
    /// <returns>An instance of <see cref="AnonymousInteraction"/> configured with the specified title and performable operation.</returns>
    public static AnonymousInteraction ThatPerforms(string title, Action performableOperation)
    {
        return Instrumented.InstanceOf<AnonymousInteraction>()
            .WithProperties(title, performableOperation);
    }
}