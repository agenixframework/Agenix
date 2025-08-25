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
///     Represents an anonymous, asynchronous performable function within the screenplay pattern.
/// </summary>
public class AnonymousPerformableFunction : ITask
{
    private readonly string _title;
    private readonly Func<Actor, Task> _asyncOperation;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AnonymousPerformableFunction"/> class.
    /// </summary>
    /// <param name="title">The title or description of the operation.</param>
    /// <param name="asyncOperation">The asynchronous operation to be performed by an actor.</param>
    public AnonymousPerformableFunction(string title, Func<Actor, Task> asyncOperation)
    {
        _title = title;
        _asyncOperation = asyncOperation;
    }

    /// <summary>
    ///     Asynchronously performs the operation using the provided actor.
    /// </summary>
    /// <typeparam name="T">The type of actor performing the action.</typeparam>
    /// <param name="actor">The actor that will perform the action.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task PerformAsAsync<T>(T actor, CancellationToken cancellationToken = default) where T : Actor
    {
        await _asyncOperation(actor);
    }

    /// <summary>
    ///     Returns a string representation of this performable function.
    /// </summary>
    /// <returns>The title of this performable function.</returns>
    public override string ToString()
    {
        return _title;
    }
}
