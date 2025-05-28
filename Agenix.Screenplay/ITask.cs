using Agenix.Screenplay.Annotations;

namespace Agenix.Screenplay;

/// <summary>
///     Represents a task to be performed by an actor within the screenplay design pattern.
/// </summary>
/// <remarks>
///     The ITask interface defines a structure for creating reusable and composable tasks
///     that actors can perform. It extends the IPerformable interface and provides
///     factory methods for creating anonymous tasks, task builders, and other
///     performable operations.
/// </remarks>
public interface ITask : IPerformable
{
    /// <summary>
    ///     Creates an anonymous task composed of an array of steps to be performed.
    /// </summary>
    /// <param name="steps">An array of steps implementing the IPerformable interface that define the task.</param>
    /// <typeparam name="T">The type of the steps, which must implement the IPerformable interface.</typeparam>
    /// <returns>An instance of AnonymousTask representing the composed task.</returns>
    public static AnonymousTask Where<T>(params T[] steps) where T : IPerformable
    {
        return Instrumented.InstanceOf<AnonymousTask>()
            .WithProperties(HumanReadableTaskName.ForCurrentMethod(), steps.ToList());
    }

    /// <summary>
    ///     Creates a TaskBuilder instance with the specified title to define a task.
    /// </summary>
    /// <param name="title">The title of the task, used for clarity and identification.</param>
    /// <returns>A TaskBuilder instance to further define the task steps.</returns>
    public static TaskBuilder Called(string title)
    {
        return new TaskBuilder(title);
    }

    /// <summary>
    ///     Create a new Performable Task made up of a list of Performables.
    /// </summary>
    public static AnonymousTask Where<T>(string title, params T[] steps) where T : IPerformable
    {
        return Instrumented.InstanceOf<AnonymousTask>()
            .WithProperties(title, steps.ToList());
    }

    /// <summary>
    ///     Creates an anonymous performable function from a given operation for an actor.
    /// </summary>
    /// <typeparam name="T">
    ///     The type that the operation will utilize during execution. Must implement
    ///     <see cref="IPerformable" />.
    /// </typeparam>
    /// <param name="performableOperation">The action to be performed by the actor.</param>
    /// <returns>An instance of <see cref="AnonymousPerformableFunction" /> that encapsulates the operation.</returns>
    public static AnonymousPerformableFunction Where<T>(Action<Actor> performableOperation)
    {
        return Instrumented.InstanceOf<AnonymousPerformableFunction>()
            .WithProperties(HumanReadableTaskName.ForCurrentMethod(), performableOperation);
    }

    /// <summary>
    ///     Creates an anonymous performable function with a title and a specified action for the actor.
    /// </summary>
    /// <param name="title">The title or description of the performable function.</param>
    /// <param name="performableOperation">The action to be performed by the actor.</param>
    /// <returns>An anonymous performable function configured with the given title and action.</returns>
    public static AnonymousPerformableFunction Where<T>(string title, Action<Actor> performableOperation)
    {
        return Instrumented.InstanceOf<AnonymousPerformableFunction>()
            .WithProperties(title, performableOperation);
    }

    /// <summary>
    ///     Creates an anonymous performable runnable task using the provided action delegate.
    /// </summary>
    /// <param name="performableOperation">The action to be executed as part of the task.</param>
    /// <typeparam name="T">The type context for which the task will be performed.</typeparam>
    /// <returns>An instance of <see cref="AnonymousPerformableRunnable" /> configured with the provided action.</returns>
    public static AnonymousPerformableRunnable ThatPerforms<T>(Action performableOperation)
    {
        return Instrumented.InstanceOf<AnonymousPerformableRunnable>()
            .WithProperties(HumanReadableTaskName.ForCurrentMethod(), performableOperation);
    }

    /// <summary>
    ///     Creates a new instance of AnonymousPerformableRunnable with the specified title and a performable operation.
    /// </summary>
    /// <param name="title">The title describing the anonymous performable runnable.</param>
    /// <param name="performableOperation">The operation to be performed as part of this performable.</param>
    /// <returns>An instance of AnonymousPerformableRunnable configured with the specified title and performable operation.</returns>
    public static AnonymousPerformableRunnable ThatPerforms<T>(string title, Action performableOperation)
    {
        return Instrumented.InstanceOf<AnonymousPerformableRunnable>()
            .WithProperties(title, performableOperation);
    }
}
