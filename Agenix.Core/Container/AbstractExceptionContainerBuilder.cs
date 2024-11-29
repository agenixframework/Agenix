namespace Agenix.Core.Container;

/// <summary>
///     Represents an abstract builder for exception containers that handle specific action configurations
///     prior to their execution within the framework. This class provides methods to add actions or action builders
///     to the container before execution.
/// </summary>
/// <typeparam name="T">
///     The type of the action container associated with this builder. It must inherit from
///     <see cref="AbstractActionContainer" />.
/// </typeparam>
/// <typeparam name="S">
///     The type of the builder that implements this abstract class. It must inherit from
///     <see cref="AbstractExceptionContainerBuilder{T, S}" />.
/// </typeparam>
public abstract class AbstractExceptionContainerBuilder<T, S> : AbstractTestContainerBuilder<T, S>
    where T : AbstractActionContainer
    where S : AbstractExceptionContainerBuilder<T, S>
{
    /// Adds specified actions to the container before its execution.
    /// This method accepts action instances.
    /// @param actions An array of action instances to be added to the container.
    /// @return An instance of the container with the specified actions included.
    /// /
    public S When(params ITestAction[] actions)
    {
        return Actions(actions);
    }

    /// Adds specified actions to the container before its execution. This version accepts action builders.
    /// @param actions An array of action builders that create actions to be added to the container.
    /// @return An instance of the container with the specified actions added.
    /// /
    public S When(params ITestActionBuilder<ITestAction>[] actions)
    {
        return Actions(actions);
    }
}