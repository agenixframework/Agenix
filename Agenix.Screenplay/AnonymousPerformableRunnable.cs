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

namespace Agenix.Screenplay;

/// <summary>
///     Represents an anonymous, executable task or action that can be performed by an actor within the screenplay pattern.
/// </summary>
/// <remarks>
///     The AnonymousPerformableRunnable class provides a way to define performable tasks or actions
///     inline using a delegate. It encapsulates an action that can be executed by any actor.
///     This supports concise and flexible task definition to enhance the readability of actor behaviors.
/// </remarks>
/// <example>
///     This class is typically used when you want to define a performable action inline for simplicity.
/// </example>
public class AnonymousPerformableRunnable(Task<Action> actions) : IPerformable
{
    /// <summary>
    ///     Executes an action or task as the specified actor in the context of the screenplay pattern.
    /// </summary>
    /// <typeparam name="T">The type of the actor performing the action, which must derive from <see cref="Actor" />.</typeparam>
    /// <param name="actor">The actor performing the action.</param>
    /// <param name="cancellationToken">
    ///     A token that allows the operation to be cancelled if required. Defaults to <see cref="CancellationToken.None" /> if
    ///     not specified.
    /// </param>
    /// <returns>A <see cref="Task" /> representing the asynchronous execution of the performable action.</returns>
    public async Task PerformAsAsync<T>(T actor, CancellationToken cancellationToken = default) where T : Actor
    {
        if (cancellationToken.IsCancellationRequested)
        {
            await Task.FromCanceled(cancellationToken);
        }

        var action = await actions.ConfigureAwait(false);
        action.Invoke();
    }
}
