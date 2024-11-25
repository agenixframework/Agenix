using System.Collections.Generic;

namespace Agenix.Core.Container;

/// <summary>
///     Interface for building test action containers.
/// </summary>
/// <typeparam name="T">The type of test action container.</typeparam>
/// <typeparam name="TS">The type of the builder.</typeparam>
public interface ITestActionContainerBuilder<out T, out S> : ITestActionBuilder<T>
    where T : ITestActionContainer
    where S : class
{
    /// @param actions An array of actions to be added.
    /// @return The updated instance of the action container builder.
    /// /
    public S Actions(params ITestAction[] actions);

    /// Adds test actions to the action container.
    /// @param actions An array of actions to be added.
    /// @return The updated instance of the action container builder.
    public S Actions(params ITestActionBuilder<ITestAction>[] actions);

    /// Get the list of test actions for this container.
    /// @return List of test actions.
    public List<ITestActionBuilder<ITestAction>> GetActions();
}