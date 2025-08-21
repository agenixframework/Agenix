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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Log;
using Agenix.Core.Actions;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Container;

/// <summary>
///     Represents an asynchronous action container that manages the execution
///     of a set of actions asynchronously in a test context. It handles both
///     the primary action execution and the retrieval of success or error
///     actions configured within the container.
/// </summary>
public class Async(Async.Builder builder)
    : AbstractAsyncActionContainer(builder.GetName() ?? "async", builder.GetDescription(), builder.GetActions())
{
    /// Static logger instance for the Async class.
    /// /
    private static readonly ILogger Log = LogManager.GetLogger(typeof(Async));

    private readonly List<IAsyncTestActionBuilder<IAsyncTestAction>> _errorActions = builder._errorActions;
    private readonly List<IAsyncTestActionBuilder<IAsyncTestAction>> _successActions = builder._successActions;

    /// Executes the asynchronous container's primary action in the provided test context.
    /// <param name="context">The context in which the test action is executed, providing runtime data and dependencies.</param>
    /// <param name="cancellationToken">
    ///     A token to signal operation cancellation, enabling cooperative cancellation of async
    ///     operations.
    /// </param>
    /// <returns>A task that represents the asynchronous execution of the container's primary action.</returns>
    public override Task DoExecute(TestContext context, CancellationToken cancellationToken = default)
    {
        Log.LogDebug("Async container forking action execution ...");

        var asyncTestAction = new ExtendedAbstractAsyncTestAction(this);

        // Don’t pass the external token to Task.Run to avoid throwing/canceling before the task starts.
        var task = Task.Run(() => ExecuteAction(asyncTestAction, context));

        // Ensure exceptions are observed and logged
        task.ContinueWith(
            t => Log.LogError(t.Exception, "Async test action failed"),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);

        return Task.CompletedTask;
    }

    /// Retrieves the list of success test actions that have been configured for this Async container.
    /// <return>A list of built ITestAction instances representing success actions.</return>
    public List<IAsyncTestAction> GetSuccessTestActions()
    {
        return _successActions.Select(actionBuilder => actionBuilder.Build()).ToList();
    }

    /// Retrieves the list of error test actions that have been configured for this Async container.
    /// <return>A list of built ITestAction instances representing error actions.</return>
    public List<IAsyncTestAction> GetErrorTestActions()
    {
        return _errorActions.Select(actionBuilder => actionBuilder.Build()).ToList();
    }

    /// <summary>
    ///     Represents an extended asynchronous test action within the async action container.
    ///     It is responsible for managing the execution of a set of actions asynchronously,
    ///     and handling completion events such as success or errors during the execution process.
    /// </summary>
    private sealed class ExtendedAbstractAsyncTestAction(Async outerInstance) : AbstractAsyncTestAction
    {
        public override async Task DoExecuteAsync(TestContext context)
        {
            foreach (var action in outerInstance.Actions.Select(actionBuilder => actionBuilder.Build()))
            {
                await outerInstance.ExecuteAction(action, context);
            }
        }

        protected override async Task OnError(TestContext context, Exception error)
        {
            Log.LogInformation("Apply error actions after async container ...");
            foreach (var action in outerInstance._errorActions.Select(actionBuilder => actionBuilder.Build()))
            {
                await action.ExecuteAsync(context);
            }
        }

        protected override async Task OnSuccess(TestContext context)
        {
            Log.LogInformation("Apply success actions after async container ...");
            foreach (var action in outerInstance._successActions.Select(actionBuilder => actionBuilder.Build()))
            {
                await action.ExecuteAsync(context);
            }
        }
    }

    /// <summary>
    ///     Builder class for constructing instances of the Async action container.
    ///     It provides methods to define success and error actions, allowing these
    ///     actions to be specified either individually or in bulk. The builder follows
    ///     a fluent API pattern to enable chaining and facilitate flexible configuration.
    /// </summary>
    public class Builder : AbstractAsyncTestContainerBuilder<Async, Builder>
    {
        internal readonly List<IAsyncTestActionBuilder<IAsyncTestAction>> _errorActions = [];
        internal readonly List<IAsyncTestActionBuilder<IAsyncTestAction>> _successActions = [];

        /// Fluent API action building entry method used in C# DSL.
        /// @return
        /// /
        public static Builder Async()
        {
            return new Builder();
        }

        /// Adds an error action.
        /// @param action The error action to add.
        /// @return Returns the builder instance for chaining.
        /// /
        public Builder ErrorAction(IAsyncTestAction action)
        {
            _errorActions.Add(new FuncIAsyncTestActionBuilder<IAsyncTestAction>(() => action));
            return this;
        }

        /// <summary>
        ///     Adds a new error action to the list of error actions in the builder.
        /// </summary>
        /// <param name="action">The action to be added as an error action.</param>
        /// <returns>The builder instance with the newly added error action.</returns>
        public Builder ErrorAction(TestActionAsync action)
        {
            _errorActions.Add(
                new FuncIAsyncTestActionBuilder<IAsyncTestAction>(() => new DelegatingAsyncTestAction(action)));
            return this;
        }

        /// Adds an error action.
        /// <param name="action">The error action to add.</param>
        /// <return>Returns the builder instance for chaining.</return>
        public Builder ErrorAction(IAsyncTestActionBuilder<IAsyncTestAction> action)
        {
            _errorActions.Add(action);
            return this;
        }

        /// Adds a success action.
        /// @param action The success action to add.
        /// @return Returns the builder instance for chaining.
        /// /
        public Builder SuccessAction(IAsyncTestAction action)
        {
            _successActions.Add(new FuncIAsyncTestActionBuilder<IAsyncTestAction>(() => action));
            return this;
        }

        /// <summary>
        ///     Adds a new success action to the list of success actions in the builder.
        /// </summary>
        /// <param name="action">The action to be added as a success action.</param>
        /// <returns>The builder instance with the newly added success action.</returns>
        public Builder SuccessAction(TestActionAsync action)
        {
            _successActions.Add(
                new FuncIAsyncTestActionBuilder<IAsyncTestAction>(() => new DelegatingAsyncTestAction(action)));
            return this;
        }

        /// Adds a success action.
        /// <param name="action">The success action to add.</param>
        /// <return>Returns the builder instance for chaining.</return>
        public Builder SuccessAction(IAsyncTestActionBuilder<IAsyncTestAction> action)
        {
            _successActions.Add(action);
            return this;
        }

        /// Adds multiple error actions.
        /// <param name="actions">The error actions to add as params.</param>
        /// <return>Returns the builder instance for chaining.</return>
        public Builder ErrorActions(params IAsyncTestActionBuilder<IAsyncTestAction>[] actions)
        {
            _errorActions.AddRange(actions.ToList());
            return this;
        }

        /// Adds a collection of error actions to the builder.
        /// <param name="actions">An array of error actions to be added.</param>
        /// <return>The current instance of the Builder.</return>
        public Builder ErrorActions(params TestActionAsync[] actions)
        {
            _errorActions.AddRange(actions.Select(action =>
                new FuncIAsyncTestActionBuilder<IAsyncTestAction>(() => new DelegatingAsyncTestAction(action))));
            return this;
        }

        /// Adds multiple error actions to the builder.
        /// <param name="actions">The error actions to be added.</param>
        /// <returns>The builder instance with the added error actions.</returns>
        public Builder ErrorActions(params IAsyncTestAction[] actions)
        {
            _errorActions.AddRange(actions.Select(action =>
                new FuncIAsyncTestActionBuilder<IAsyncTestAction>(() => action)));
            return this;
        }

        /// Adds multiple success actions.
        /// <param name="actions">The success actions to add as params.</param>
        /// <return>Returns the builder instance for chaining.</return>
        public Builder SuccessActions(params IAsyncTestActionBuilder<IAsyncTestAction>[] actions)
        {
            _successActions.AddRange(actions.ToList());
            return this;
        }

        /// Adds a collection of success actions to the builder.
        /// <param name="actions">An array of success actions to be added.</param>
        /// <return>The current instance of the Builder.</return>
        public Builder SuccessActions(params TestActionAsync[] actions)
        {
            _successActions.AddRange(actions.Select(action =>
                new FuncIAsyncTestActionBuilder<IAsyncTestAction>(() => new DelegatingAsyncTestAction(action))));
            return this;
        }

        /// Adds multiple success actions to the builder.
        /// <param name="actions">The success actions to be added.</param>
        /// <returns>The builder instance with the added success actions.</returns>
        public Builder SuccessActions(params IAsyncTestAction[] actions)
        {
            _successActions.AddRange(actions.Select(action =>
                new FuncIAsyncTestActionBuilder<IAsyncTestAction>(() => action)));
            return this;
        }

        /// Builds and returns an instance of Async.
        /// The constructed Async instance.
        public override Async Build()
        {
            return DoBuild();
        }

        /// Constructs a new instance of the Async class using the provided Builder instance.
        /// @return
        /// A new instance of the Async class.
        protected override Async DoBuild()
        {
            return new Async(this);
        }
    }
}
