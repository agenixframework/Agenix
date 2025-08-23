#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
//
// Copyright (c) 2025 Agenix
//
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

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
public abstract class AbstractTestContainerBuilder<T, TS> : AbstractAsyncTestActionBuilder<T, TS>,
    IAsyncTestActionContainerBuilder<T, TS>
    where T : IAsyncTestActionContainer
    where TS : class
{
    /// <summary>
    ///     A collection used to store instances of asynchronous test action builders.
    ///     This list is used internally within the AbstractTestContainerBuilder to manage
    ///     test action builders that define asynchronous test actions to be executed.
    /// </summary>
    protected readonly List<IAsyncTestActionBuilder<IAsyncTestAction>> _actions = [];

    /// <summary>
    ///     Adds the specified actions to the current container builder.
    /// </summary>
    /// <param name="actions">An array of actions to be added.</param>
    /// <returns>The current instance of the builder with the added actions.</returns>
    public TS Actions(params IAsyncTestAction[] actions)
    {
        var actionBuilders = actions
            .Where(action => action is not NoopAsyncTestAction)
            .Select(IAsyncTestActionBuilder<IAsyncTestAction> (action) =>
                new FuncIAsyncTestActionBuilder<IAsyncTestAction>(() => action))
            .ToArray();

        return Actions(actionBuilders);
    }

    /// <summary>
    ///     Represents a collection of actions.
    /// </summary>
    public virtual TS Actions(params IAsyncTestActionBuilder<IAsyncTestAction>[] actions)
    {
        for (var i = 0; i < actions.Length; i++)
        {
            var current = actions[i];

            if (current.Build() is NoopAsyncTestAction)
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

        return Self;
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
            .Where(action => action is not NoopAsyncTestAction)
            .ToList());
        return container;
    }

    /// <summary>
    ///     Retrieves the list of action builders associated with the test container.
    /// </summary>
    /// <returns>A list of <see cref="ITestActionBuilder{ITestAction}" /> instances representing the actions.</returns>
    public List<IAsyncTestActionBuilder<IAsyncTestAction>> GetActions()
    {
        return _actions;
    }

    /// <summary>
    ///     Adds a test action to the current container builder using a provided test action delegate.
    /// </summary>
    /// <param name="testAction">A delegate that defines the test action to be added.</param>
    /// <returns>The current instance of the builder with the added test action.</returns>
    public TS Actions(TestActionAsync testAction)
    {
        return Actions(new DelegatingAsyncTestAction(testAction));
    }

    /// <summary>
    ///     Resolves the actual action builder, particularly useful if the provided builder is a delegating builder.
    /// </summary>
    /// <param name="builder">The action builder to resolve.</param>
    /// <returns>The resolved action builder.</returns>
    private IAsyncTestActionBuilder<IAsyncTestAction> ResolveActionBuilder(
        IAsyncTestActionBuilder<IAsyncTestAction> builder)
    {
        if (builder is IAsyncTestActionBuilder<IAsyncTestAction>.IDelegatingTestActionBuilder<IAsyncTestAction>
            delegatingBuilder)
        {
            return ResolveActionBuilder(delegatingBuilder.Delegate);
        }

        return builder;
    }

    /// <summary>
    ///     Performs the construction of a test action container.
    /// </summary>
    /// <returns>An instance of <see cref="IAsyncTestActionContainer" /> with the specified actions.</returns>
    protected abstract T DoBuild();
}
