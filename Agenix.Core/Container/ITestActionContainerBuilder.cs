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
using Agenix.Api;
using Agenix.Api.Container;

namespace Agenix.Core.Container;

/// <summary>
///     Interface for building test action containers.
/// </summary>
/// <typeparam name="T">The type of test action container.</typeparam>
/// <typeparam name="TS">The type of the builder.</typeparam>
public interface ITestActionContainerBuilder<out T, out S> : ITestActionBuilder<T>
    where T : ITestActionContainer
    where S : class
{
    /// @param actions An array of actions to be added.
    /// @return The updated instance of the action container builder.
    /// /
    public S Actions(params ITestAction[] actions);

    /// Adds test actions to the action container.
    /// @param actions An array of actions to be added.
    /// @return The updated instance of the action container builder.
    public S Actions(params ITestActionBuilder<ITestAction>[] actions);

    /// Get the list of test actions for this container.
    /// @return List of test actions.
    public List<ITestActionBuilder<ITestAction>> GetActions();
}
