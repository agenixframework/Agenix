using System.Collections.Generic;
using System.Linq;
using Agenix.Core.Actions;

namespace Agenix.Core.Container;

/// <summary>
///     Abstract base class providing a builder structure for test action containers.
/// </summary>
/// <typeparam name="T">Type that implements ITestActionContainer.</typeparam>
/// <typeparam name="TS">Type that implements both ITestActionContainerBuilder and ITestActionBuilder.</typeparam>
public abstract class AbstractTestContainerBuilder<T, TS> : AbstractTestActionBuilder<T, TS>,
    ITestActionContainerBuilder<T>
    where T : ITestActionContainer
    where TS : ITestActionContainerBuilder<T>
{
    protected readonly List<ITestActionBuilder<ITestAction>> _actions = [];

    /// <summary>
    ///     Adds the specified actions to the current container builder.
    /// </summary>
    /// <param name="actions">An array of actions to be added.</param>
    /// <returns>The current instance of the builder with the added actions.</returns>
    public ITestActionContainerBuilder<T> Actions(params ITestAction[] actions)
    {
        var actionBuilders = actions
            .Where(action => action is not NoopTestAction)
            .Select(ITestActionBuilder<ITestAction> (action) => new FuncITestActionBuilder(() => action))
            .ToArray();

        return Actions(actionBuilders);
    }

    /// <summary>
    ///     Represents a collection of actions.
    /// </summary>
    public ITestActionContainerBuilder<T> Actions(params ITestActionBuilder<ITestAction>[] actions)
    {
        for (var i = 0; i < actions.Length; i++)
        {
            var current = actions[i];

            if (current.Build() is NoopTestAction) continue;

            if (_actions.Count == i)
                _actions.Add(current);
            else if (!ResolveActionBuilder(_actions[i]).Equals(ResolveActionBuilder(current)))
                _actions.Insert(i, current);
        }

        return self;
    }

    /// <summary>
    ///     Constructs the final test action container, assigning any applicable actions.
    /// </summary>
    /// <returns>
    ///     An instance of <see cref="ITestActionContainer" /> that contains the specified actions, excluding any Noop
    ///     test actions.
    /// </returns>
    public override T Build()
    {
        var container = DoBuild();

        container.SetActions(_actions
            .Select(builder => builder.Build())
            .Where(action => action is not NoopTestAction)
            .ToList());
        return container;
    }

    /// <summary>
    ///     Retrieves the list of action builders associated with the test container.
    /// </summary>
    /// <returns>A list of <see cref="ITestActionBuilder{ITestAction}" /> instances representing the actions.</returns>
    public List<ITestActionBuilder<ITestAction>> GetActions()
    {
        return _actions;
    }

    /// <summary>
    ///     Resolves the actual action builder, particularly useful if the provided builder is a delegating builder.
    /// </summary>
    /// <param name="builder">The action builder to resolve.</param>
    /// <returns>The resolved action builder.</returns>
    private ITestActionBuilder<ITestAction> ResolveActionBuilder(ITestActionBuilder<ITestAction> builder)
    {
        if (builder is ITestActionBuilder<ITestAction>.IDelegatingTestActionBuilder<ITestAction> delegatingBuilder)
            return ResolveActionBuilder(delegatingBuilder.Delegate);
        return builder;
    }

    /// <summary>
    ///     Performs the construction of a test action container.
    /// </summary>
    /// <returns>An instance of <see cref="ITestActionContainer" /> with the specified actions.</returns>
    protected abstract T DoBuild();
}