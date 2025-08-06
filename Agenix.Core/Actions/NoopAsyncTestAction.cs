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
using Agenix.Api.Context;

namespace Agenix.Core.Actions;

/**
 * Special test action doing nothing but implementing the test action interface. This is useful during C# dsl fluent API
 * that needs to return a test action, but this should not be included or executed during the test run. See test behavior applying
 * test actions.
 */
public class NoopAsyncTestAction : IAsyncTestAction
{
    /// <summary>
    ///     Executes the test action asynchronously. This implementation does not perform any action
    ///     and serves as a no-operation (noop) test action.
    /// </summary>
    /// <param name="context">The test context containing additional data required for execution.</param>
    /// <param name="cancellationToken">An optional token to observe cancellation requests.</param>
    /// <returns>A completed task, as this action does not perform any operation.</returns>
    public Task ExecuteAsync(TestContext context, CancellationToken cancellationToken = default)
    {
        // do nothing

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Returns the name of the test action as a string.
    /// </summary>
    /// <returns>A string representing the name of the test action.</returns>
    public string GetName()
    {
        return "noop";
    }
}
