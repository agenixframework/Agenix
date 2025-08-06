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
///     Represents an anonymous performable action or task to be executed by an actor in the screenplay pattern.
/// </summary>
/// <remarks>
///     This class allows for the definition of actions or tasks via anonymous functions, providing flexibility in
///     describing custom behaviors for an actor.
/// </remarks>
public class AnonymousPerformableFunction(Action<Actor> actions) : IPerformable
{
    /// <summary>
    ///     Executes the specified actions or tasks as the provided actor asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the actor performing the actions, which must extend from the Actor class.</typeparam>
    /// <param name="actor">The actor executing the actions defined by this performable function.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous execution of the performable function.</returns>
    public async Task PerformAsAsync<T>(T actor, CancellationToken cancellationToken = default) where T : Actor
    {
        if (cancellationToken.IsCancellationRequested)
        {
            await Task.FromCanceled(cancellationToken);
        }

        actions(actor);
    }
}
