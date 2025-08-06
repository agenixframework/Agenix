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

using System.Threading;
using System.Threading.Tasks;
using Agenix.Api;
using Agenix.Api.Common;
using Agenix.Api.Context;

namespace Agenix.Core.Actions;

/// <summary>
///     Abstract base class for test actions. Class provides a default name and description.
/// </summary>
public abstract class AbstractTestActionAsync : IAsyncTestAction, INamed, IAsyncDescribed
{
    /// <summary>
    ///     Describing the test action
    /// </summary>
    protected string Description;

    /// <summary>
    ///     Abstract base class for test actions.
    ///     Provides a default implementation for name and description management.
    /// </summary>
    public AbstractTestActionAsync()
    {
        Name = GetType().Name;
    }

    /// <summary>
    ///     Abstract base class for test actions.
    ///     Facilitates name and description initialization and execution behavior definition.
    /// </summary>
    public AbstractTestActionAsync(string name, string description)
    {
        Name = name;
        Description = description;
    }

    /// <summary>
    ///     Abstract base class for test actions.
    ///     Provides default implementation for name and description management.
    /// </summary>
    public AbstractTestActionAsync(string name, AbstractAsyncTestActionBuilder<IAsyncTestAction, dynamic> builder)
    {
        Name = builder.GetName() ?? name;
        Description = builder.GetDescription();
    }

    /// <summary>
    ///     Gets the description of the test action. The description provides additional context or details
    ///     about the specific test action implementation.
    /// </summary>
    /// <returns>
    ///     A string representing the description of the test action.
    /// </returns>
    public string GetDescription()
    {
        return Description;
    }

    /// <summary>
    ///     Sets the description of the test action and returns the current instance of the test action.
    /// </summary>
    /// <param name="description">The description to be assigned to the test action.</param>
    /// <returns>The current instance of the test action with the updated description.</returns>
    public IAsyncTestAction SetDescription(string description)
    {
        Description = description;
        return this;
    }

    /// <summary>
    ///     TestAction name injected as spring bean name
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    ///     Executes the test action asynchronously.
    /// </summary>
    /// <param name="context">The context in which the test action is executed.</param>
    /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ExecuteAsync(TestContext context, CancellationToken cancellationToken = default)
    {
        return DoExecute(context, cancellationToken);
    }

    /// <summary>
    ///     Sets the name of the object.
    /// </summary>
    /// <param name="name">
    ///     The new name to assign.
    /// </param>
    public void SetName(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     Executes the custom logic for a test action asynchronously.
    /// </summary>
    /// <param name="context">The test context that provides data and services for the execution.</param>
    /// <param name="cancellationToken">A token for monitoring cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public abstract Task DoExecute(TestContext context, CancellationToken cancellationToken = default);
}
