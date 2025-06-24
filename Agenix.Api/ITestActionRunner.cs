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

namespace Agenix.Api;

/// Interface for running test actions.
/// /
public interface ITestActionRunner
{
    /// Runs the provided test action.
    /// @param action The test action to be executed.
    /// @return The executed test action.
    /// /
    T Run<T>(T action) where T : ITestAction;

    /// Runs the provided test action.
    /// @param action The test action to be executed.
    /// @return The executed test action.
    T Run<T>(Func<T> action) where T : ITestAction;

    /// Runs a given test action.
    /// @param action The test action to run.
    /// @return The result of the test action.
    /// /
    T Run<T>(ITestActionBuilder<T> builder) where T : ITestAction;

    /// Applies a specified test behavior using the given behavior interface.
    /// <param name="behavior">The test behavior to be applied.</param>
    /// <return>A builder for constructing the ApplyTestBehaviorAction with the specified behavior.</return>
    ITestActionBuilder<ITestAction> ApplyBehavior(ITestBehavior behavior);

    /// Applies a specified test behavior to the test action runner.
    /// <param name="behavior">The test behavior to be applied.</param>
    /// <return>A builder for creating a test action with the applied behavior.</return>
    ITestActionBuilder<ITestAction> ApplyBehavior(TestBehavior behavior);
}
