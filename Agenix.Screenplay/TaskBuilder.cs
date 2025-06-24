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

using Agenix.Screenplay.Annotations;

namespace Agenix.Screenplay;

/// <summary>
///     A builder class for creating tasks in a screenplay-style testing framework.
/// </summary>
/// <remarks>
///     The <see cref="TaskBuilder" /> class allows the creation of an <see cref="AnonymousTask" />
///     with a specific title and a series of steps for an actor to perform. This builder
///     simplifies the process of defining tasks by providing a fluent API.
/// </remarks>
public class TaskBuilder(string title)
{
    /// <summary>
    ///     Defines an anonymous task where the actor attempts to perform the specified steps.
    /// </summary>
    /// <typeparam name="T">The type of steps to be performed, implementing <see cref="IPerformable" />.</typeparam>
    /// <param name="steps">An array of steps that the actor will attempt to perform in the task.</param>
    /// <returns>An <see cref="AnonymousTask" /> representing the task that consists of the provided steps.</returns>
    public AnonymousTask WhereTheActorAttemptsTo<T>(params T[] steps) where T : IPerformable
    {
        return ITask.Where(title, steps);
    }
}
