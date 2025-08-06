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
using System.Threading.Tasks;
using Agenix.Api;
using Agenix.Api.Common;
using Agenix.Api.Container;
using Agenix.Api.Context;
using Agenix.Api.Log;
using Agenix.Core.Actions;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Container;

/// <summary>
///     AbstractAsyncActionContainer serves as an abstract base class for asynchronous action containers.
///     It provides mechanisms to manage a collection of asynchronous test actions, facilitating their lifecycle
///     management, including execution, retrieval, and state updates such as tracking active and executed actions.
/// </summary>
public abstract class AbstractAsyncActionContainer : AbstractTestActionAsync, IAsyncTestActionContainer, ICompletable
{
    private static readonly ILogger Log = LogManager.GetLogger(typeof(AbstractAsyncActionContainer));

    /// <summary>
    ///     List of all executed actions during a container run.
    /// </summary>
    private readonly List<IAsyncTestAction> _executedActions = [];

    //Last executed action for error reporting reasons
    /// <summary>
    ///     The last executed action, used primarily for error reporting.
    /// </summary>
    private IAsyncTestAction _activeAction;

    /// <summary>
    ///     List of nested actions.
    /// </summary>
    protected List<IAsyncTestActionBuilder<IAsyncTestAction>> Actions = [];

    // Constructors and methods...
    /// <summary>
    ///     Represents an abstract base class for asynchronous action containers,
    ///     providing functionality to manage and execute nested asynchronous test actions.
    ///     Supports lifecycle management, action addition, retrieval, and execution tracking.
    /// </summary>
    protected AbstractAsyncActionContainer()
    {
    }

    /// <summary>
    ///     Represents an abstract base class for asynchronous action containers,
    ///     providing mechanisms for managing a collection of asynchronous test actions.
    ///     Includes functionality for adding, retrieving, executing, and tracking actions in a test lifecycle.
    ///     Supports hierarchical and nested test action structures.
    /// </summary>
    protected AbstractAsyncActionContainer(string name, string description,
        List<IAsyncTestActionBuilder<IAsyncTestAction>> actions)
        : base(name, description)
    {
        Actions = actions;
    }

    /// <summary>
    ///     Represents an abstract base class for asynchronous action containers,
    ///     enabling the management, execution, and organization of asynchronous test actions.
    ///     Provides methods for adding, retrieving, and tracking test actions along with their execution states.
    ///     Supports lifecycle operations and customization through builders.
    /// </summary>
    protected AbstractAsyncActionContainer(string name,
        AbstractAsyncTestContainerBuilder<IAsyncTestActionContainer, dynamic> builder)
        : base(name, builder.GetDescription() ?? "")
    {
        Actions = builder.GetActions();
    }

    /// <summary>
    ///     Sets the list of actions in the action container.
    /// </summary>
    /// <param name="actions">The list of actions to be set in the container.</param>
    /// <returns>The current instance of the action container.</returns>
    public IAsyncTestActionContainer SetActions(List<IAsyncTestAction> actions)
    {
        Actions = actions
            .Select(IAsyncTestActionBuilder<IAsyncTestAction> (a) =>
                new FuncIAsyncTestActionBuilder<IAsyncTestAction>(() => a))
            .ToList();
        return this;
    }

    /// <summary>
    ///     Retrieves the list of actions that have been added to the action container after building them.
    /// </summary>
    /// <returns>A list of built actions currently in the action container.</returns>
    public virtual List<IAsyncTestAction> GetActions()
    {
        return Actions.Select(a => a.Build()).ToList();
    }

    /// <summary>
    ///     Retrieves the total count of actions in the action container.
    /// </summary>
    /// <returns>The number of actions currently in the action container.</returns>
    public long GetActionCount()
    {
        return Actions.Count;
    }

    /// <summary>
    ///     Adds multiple test actions to the action container.
    /// </summary>
    /// <param name="action">An array of test actions to be added.</param>
    /// <returns>The action container with the newly added test actions.</returns>
    public IAsyncTestActionContainer AddTestActions(params IAsyncTestAction[] action)
    {
        // Convert the input actions to a list of FuncITestActionBuilder
        var actionBuilders = action
            .Select(IAsyncTestActionBuilder<IAsyncTestAction> (a) =>
                new FuncIAsyncTestActionBuilder<IAsyncTestAction>(() => a))
            .ToList();

        Actions.AddRange(actionBuilders);
        return this;
    }

    /// <summary>
    ///     Adds a single test action to the action container.
    /// </summary>
    /// <param name="action">The test action to be added.</param>
    /// <returns>The action container with the newly added test action.</returns>
    public IAsyncTestActionContainer AddTestAction(IAsyncTestAction action)
    {
        Actions.Add(new FuncIAsyncTestActionBuilder<IAsyncTestAction>(() => action));
        return this;
    }

    /// <summary>
    ///     Retrieves the index of the specified test action within the list of executed actions.
    /// </summary>
    /// <param name="action">The test action whose index is to be found.</param>
    /// <returns>The index of the specified test action if found; otherwise, -1.</returns>
    public int GetActionIndex(IAsyncTestAction action)
    {
        return _executedActions.IndexOf(action);
    }

    /// <summary>
    ///     Sets the specified action as the active action in the action container.
    /// </summary>
    /// <param name="action">The action to be set as the active action.</param>
    public void SetActiveAction(IAsyncTestAction action)
    {
        _activeAction = action;
    }

    /// <summary>
    ///     Adds the specified action to the list of executed actions.
    /// </summary>
    /// <param name="action">The action that has been executed.</param>
    public void SetExecutedAction(IAsyncTestAction action)
    {
        _executedActions.Add(action);
    }

    /// <summary>
    ///     Retrieves the currently active test action within the action container.
    /// </summary>
    /// <returns>The active test action.</returns>
    public IAsyncTestAction GetActiveAction()
    {
        return _activeAction;
    }

    /// <summary>
    ///     Retrieves the list of executed actions.
    /// </summary>
    /// <returns>A list of executed test actions.</returns>
    public List<IAsyncTestAction> GetExecutedActions()
    {
        return _executedActions;
    }

    /// <summary>
    ///     Retrieves the test action at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the test action to retrieve.</param>
    /// <returns>The test action at the specified index.</returns>
    public virtual IAsyncTestAction GetTestAction(int index)
    {
        return index < _executedActions.Count ? _executedActions[index] : Actions[index].Build();
    }

    /// <summary>
    ///     Checks if all actions in the container are completed within the given test context.
    /// </summary>
    /// <param name="context">The test context to verify the completion status of actions.</param>
    /// <return>True if all actions are completed; otherwise, false.</return>
    public virtual bool IsDone(TestContext context)
    {
        // No actions configured -> container is trivially done
        if (Actions.Count == 0)
        {
            return true;
        }

        // If there are actions configured but none executed yet, we are not done.
        // This also covers potential races where _activeAction isn't visible yet across threads.
        if (_executedActions.Count == 0)
        {
            return false;
        }

        // If there is an active action that hasn't been marked executed yet, we are not done.
        if (!_executedActions.Contains(_activeAction))
        {
            return false;
        }

        // Verify all executed actions that are completable have actually finished.
        foreach (var action in new List<IAsyncTestAction>(_executedActions))
        {
            if (action is ICompletable completable && !completable.IsDone(context))
            {
                if (Log.IsEnabled(LogLevel.Debug))
                {
                    var actionName = string.IsNullOrWhiteSpace(action.Name)
                        ? action.GetType().Name
                        : action.Name;

                    Log.LogDebug("{ActionName} not completed yet", actionName);
                }

                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Adds a test action built by the specified action builder to the action container.
    /// </summary>
    /// <param name="action">The action builder that constructs the test action to be added.</param>
    /// <returns>The action container with the newly added test action.</returns>
    public AbstractAsyncActionContainer AddTestAction(IAsyncTestActionBuilder<IAsyncTestAction> action)
    {
        Actions.Add(action);
        return this;
    }

    /// <summary>
    ///     Executes the specified action in the given test context.
    /// </summary>
    /// <param name="action">The action to be executed.</param>
    /// <param name="context">The context in which the action should be executed.</param>
    protected async Task ExecuteAction(IAsyncTestAction action, TestContext context)
    {
        try
        {
            SetActiveAction(action);
            await action.ExecuteAsync(context);
        }
        finally
        {
            SetExecutedAction(action);
        }
    }
}
