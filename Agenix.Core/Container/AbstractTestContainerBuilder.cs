#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System.Collections.Generic;
using System.Linq;
using Agenix.Api;
using Agenix.Api.Container;
using Agenix.Core.Actions;

namespace Agenix.Core.Container;

/// <summary>
///     Abstract base class providing a builder structure for test action containers.
/// </summary>
/// <typeparam name="T">Type that implements ITestActionContainer.</typeparam>
/// <typeparam name="TS">Type that implements both ITestActionContainerBuilder and ITestActionBuilder.</typeparam>
public abstract class AbstractTestContainerBuilder<T, TS> : AbstractTestActionBuilder<T, TS>,
    ITestActionContainerBuilder<T, TS>
    where T : ITestActionContainer
    where TS : class
{
    protected readonly List<ITestActionBuilder<ITestAction>> _actions = [];

    /// <summary>
    ///     Adds the specified actions to the current container builder.
    /// </summary>
    /// <param name="actions">An array of actions to be added.</param>
    /// <returns>The current instance of the builder with the added actions.</returns>
    public TS Actions(params ITestAction[] actions)
    {
        var actionBuilders = actions
            .Where(action => action is not NoopTestAction)
            .Select(ITestActionBuilder<ITestAction> (action) => new FuncITestActionBuilder<ITestAction>(() => action))
            .ToArray();

        return Actions(actionBuilders);
    }

    /// <summary>
    ///     Represents a collection of actions.
    /// </summary>
    public virtual TS Actions(params ITestActionBuilder<ITestAction>[] actions)
    {
        for (var i = 0; i < actions.Length; i++)
        {
            var current = actions[i];

            if (current.Build() is NoopTestAction)
            {
                continue;
            }

            if (_actions.Count == i)
            {
                _actions.Add(current);
            }
            else if (!ResolveActionBuilder(_actions[i]).Equals(ResolveActionBuilder(current)))
            {
                _actions.Insert(i, current);
            }
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
    ///     Adds a test action to the current container builder using a provided test action delegate.
    /// </summary>
    /// <param name="testAction">A delegate that defines the test action to be added.</param>
    /// <returns>The current instance of the builder with the added test action.</returns>
    public TS Actions(TestAction testAction)
    {
        return Actions(new DelegatingTestAction(testAction));
    }

    /// <summary>
    ///     Resolves the actual action builder, particularly useful if the provided builder is a delegating builder.
    /// </summary>
    /// <param name="builder">The action builder to resolve.</param>
    /// <returns>The resolved action builder.</returns>
    private ITestActionBuilder<ITestAction> ResolveActionBuilder(ITestActionBuilder<ITestAction> builder)
    {
        if (builder is ITestActionBuilder<ITestAction>.IDelegatingTestActionBuilder<ITestAction> delegatingBuilder)
        {
            return ResolveActionBuilder(delegatingBuilder.Delegate);
        }

        return builder;
    }

    /// <summary>
    ///     Performs the construction of a test action container.
    /// </summary>
    /// <returns>An instance of <see cref="ITestActionContainer" /> with the specified actions.</returns>
    protected abstract T DoBuild();
}
