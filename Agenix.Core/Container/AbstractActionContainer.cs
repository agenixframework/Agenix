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
using Agenix.Api.Common;
using Agenix.Api.Container;
using Agenix.Api.Context;
using Agenix.Api.Log;
using Agenix.Core.Actions;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Container;

/// <summary>
///     AbstractActionContainer is an abstract base class that provides common functionality for action containers.
///     It facilitates the management and execution of a list of nested actions and supports action lifecycle operations.
/// </summary>
public abstract class AbstractActionContainer : AbstractTestAction, ITestActionContainer, ICompletable
{
    private static readonly ILogger Log = LogManager.GetLogger(typeof(AbstractActionContainer));

    /// <summary>
    ///     List of all executed actions during container run.
    /// </summary>
    private readonly List<ITestAction> _executedActions = [];

    //Last executed action for error reporting reasons
    /// <summary>
    ///     The last executed action, used primarily for error reporting.
    /// </summary>
    private ITestAction _activeAction;

    /// <summary>
    ///     List of nested actions.
    /// </summary>
    protected List<ITestActionBuilder<ITestAction>> actions = [];

    // Constructors and methods...
    protected AbstractActionContainer()
    {
    }

    protected AbstractActionContainer(string name, string description, List<ITestActionBuilder<ITestAction>> actions)
        : base(name, description)
    {
        this.actions = actions;
    }

    protected AbstractActionContainer(string name, AbstractTestContainerBuilder<ITestActionContainer, dynamic> builder)
        : base(name, builder.GetDescription() ?? "")
    {
        actions = builder.GetActions();
    }

    /// <summary>
    ///     Checks if all actions in the container are completed within the given test context.
    /// </summary>
    /// <param name="context">The test context to verify the completion status of actions.</param>
    /// <return>True if all actions are completed; otherwise, false.</return>
    public virtual bool IsDone(TestContext context)
    {
        if (actions.Count == 0 || IsDisabled(context))
        {
            return true;
        }

        if (_activeAction == null && _executedActions.Count == 0)
        {
            return true;
        }

        if (!_executedActions.Contains(_activeAction))
        {
            return false;
        }

        foreach (var action in new List<ITestAction>(_executedActions))
        {
            if (action is ICompletable completable && !completable.IsDone(context))
            {
                if (Log.IsEnabled(LogLevel.Debug))
                {
                    var actionName = string.IsNullOrWhiteSpace(action.Name)
                        ? action.GetType().Name
                        : action.Name;

                    Log.LogDebug($"{actionName} not completed yet");
                }

                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Sets the list of actions in the action container.
    /// </summary>
    /// <param name="toAdd">The list of actions to be set in the container.</param>
    /// <returns>The current instance of the action container.</returns>
    public ITestActionContainer SetActions(List<ITestAction> toAdd)
    {
        actions = toAdd
            .Select(ITestActionBuilder<ITestAction> (a) => new FuncITestActionBuilder<ITestAction>(() => a))
            .ToList();
        return this;
    }

    /// <summary>
    ///     Retrieves the list of actions that have been added to the action container after building them.
    /// </summary>
    /// <returns>A list of built actions currently in the action container.</returns>
    public virtual List<ITestAction> GetActions()
    {
        return actions.Select(a => a.Build()).ToList();
    }

    /// <summary>
    ///     Retrieves the total count of actions in the action container.
    /// </summary>
    /// <returns>The number of actions currently in the action container.</returns>
    public long GetActionCount()
    {
        return actions.Count;
    }

    /// <summary>
    ///     Adds multiple test actions to the action container.
    /// </summary>
    /// <param name="toAdd">An array of test actions to be added.</param>
    /// <returns>The action container with the newly added test actions.</returns>
    public ITestActionContainer AddTestActions(params ITestAction[] toAdd)
    {
        // Convert the input actions to a list of FuncITestActionBuilder
        var actionBuilders = toAdd
            .Select(ITestActionBuilder<ITestAction> (a) => new FuncITestActionBuilder<ITestAction>(() => a))
            .ToList();

        actions.AddRange(actionBuilders);
        return this;
    }

    /// <summary>
    ///     Adds a single test action to the action container.
    /// </summary>
    /// <param name="action">The test action to be added.</param>
    /// <returns>The action container with the newly added test action.</returns>
    public ITestActionContainer AddTestAction(ITestAction action)
    {
        actions.Add(new FuncITestActionBuilder<ITestAction>(() => action));
        return this;
    }

    /// <summary>
    ///     Retrieves the index of the specified test action within the list of executed actions.
    /// </summary>
    /// <param name="action">The test action whose index is to be found.</param>
    /// <returns>The index of the specified test action if found; otherwise, -1.</returns>
    public int GetActionIndex(ITestAction action)
    {
        return _executedActions.IndexOf(action);
    }

    /// <summary>
    ///     Sets the specified action as the active action in the action container.
    /// </summary>
    /// <param name="action">The action to be set as the active action.</param>
    public void SetActiveAction(ITestAction action)
    {
        _activeAction = action;
    }

    /// <summary>
    ///     Adds the specified action to the list of executed actions.
    /// </summary>
    /// <param name="action">The action that has been executed.</param>
    public void SetExecutedAction(ITestAction action)
    {
        _executedActions.Add(action);
    }

    /// <summary>
    ///     Retrieves the currently active test action within the action container.
    /// </summary>
    /// <returns>The active test action.</returns>
    public ITestAction GetActiveAction()
    {
        return _activeAction;
    }

    /// <summary>
    ///     Retrieves the list of executed actions.
    /// </summary>
    /// <returns>A list of executed test actions.</returns>
    public List<ITestAction> GetExecutedActions()
    {
        return _executedActions;
    }

    /// <summary>
    ///     Retrieves the test action at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the test action to retrieve.</param>
    /// <returns>The test action at the specified index.</returns>
    public virtual ITestAction GetTestAction(int index)
    {
        return index < _executedActions.Count ? _executedActions[index] : actions[index].Build();
    }

    /// <summary>
    ///     Executes the specified action in the given test context.
    /// </summary>
    /// <param name="action">The action to be executed.</param>
    /// <param name="context">The context in which the action should be executed.</param>
    protected void ExecuteAction(ITestAction action, TestContext context)
    {
        try
        {
            SetActiveAction(action);
            action.Execute(context);
        }
        finally
        {
            SetExecutedAction(action);
        }
    }

    /// <summary>
    ///     Adds a test action built by the specified action builder to the action container.
    /// </summary>
    /// <param name="action">The action builder that constructs the test action to be added.</param>
    /// <returns>The action container with the newly added test action.</returns>
    public AbstractActionContainer AddTestAction(ITestActionBuilder<ITestAction> action)
    {
        actions.Add(action);
        return this;
    }
}
