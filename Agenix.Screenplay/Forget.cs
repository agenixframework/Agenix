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
///     An interaction that removes a value from an actor's memory.
/// </summary>
public class Forget : Interaction
{
    private readonly string _memoryKey;

    private Forget(string memoryKey)
    {
        _memoryKey = memoryKey;
    }

    /// <summary>
    ///     Executes the interaction of forgetting a value from the actor's memory using the specified memory key.
    /// </summary>
    /// <typeparam name="T">The type of the actor performing the interaction, constrained to <see cref="Actor" />.</typeparam>
    /// <param name="actor">The actor performing the interaction.</param>
    /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation of forgetting the value.</returns>
    public async Task PerformAsAsync<T>(T actor, CancellationToken cancellationToken = default) where T : Actor
    {
        if (cancellationToken.IsCancellationRequested)
        {
            await Task.FromCanceled(cancellationToken);
        }

        await actor.Forget(_memoryKey);
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="Forget" /> interaction for removing a value associated with the specified
    ///     key from an actor's memory.
    /// </summary>
    /// <param name="memoryKey">The key identifying the value in the actor's memory to be forgotten.</param>
    /// <returns>A new instance of the <see cref="Forget" /> interaction configured with the provided memory key.</returns>
    public static Forget TheValueOf(string memoryKey)
    {
        return new Forget(memoryKey);
    }
}
