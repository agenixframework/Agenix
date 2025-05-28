namespace Agenix.Screenplay;

/// <summary>
/// A builder class for creating tasks in a screenplay-style testing framework.
/// </summary>
/// <remarks>
/// The <see cref="TaskBuilder"/> class allows the creation of an <see cref="AnonymousTask"/>
/// with a specific title and a series of steps for an actor to perform. This builder
/// simplifies the process of defining tasks by providing a fluent API.
/// </remarks>
public class TaskBuilder(string title)
{
    /// <summary>
    /// Defines an anonymous task where the actor attempts to perform the specified steps.
    /// </summary>
    /// <typeparam name="T">The type of steps to be performed, implementing <see cref="IPerformable"/>.</typeparam>
    /// <param name="steps">An array of steps that the actor will attempt to perform in the task.</param>
    /// <returns>An <see cref="AnonymousTask"/> representing the task that consists of the provided steps.</returns>
    public AnonymousTask WhereTheActorAttemptsTo<T>(params T[] steps) where T : IPerformable
    {
        return ITask.Where(title, steps);
    }
}