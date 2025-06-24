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

using Agenix.Api;

namespace Agenix.Core;

/// Interface that extends ITestActionRunner to support behavior-driven development (BDD) style methods.
/// Methods include Given, When, Then, and And as aliases to represent different stages of the Gherkin syntax in BDD.
/// /
public interface IGherkinTestActionRunner : ITestActionRunner
{
    /// Behavior driven style alias for run method.
    /// @param action The test action.
    /// @typeparam T The type of the test action.
    /// @return The executed test action.
    /// /
    T Given<T>(T action) where T : ITestAction
    {
        return Given(() => action);
    }

    /// <summary>
    ///     Behavior driven style alias for run method.
    /// </summary>
    /// <typeparam name="T">The type of the test action.</typeparam>
    /// <param name="action">The test action.</param>
    /// <returns>The executed test action.</returns>
    T Given<T>(Func<T> builder) where T : ITestAction
    {
        return Run(builder);
    }

    T Given<T>(ITestActionBuilder<T> builder) where T : ITestAction
    {
        return Run(builder);
    }

    /// Behavior driven style alias for run method.
    /// @param action The test action.
    /// @return The executed test action.
    /// /
    T When<T>(T action) where T : ITestAction
    {
        return When(() => action);
    }

    T When<T>(ITestActionBuilder<T> builder) where T : ITestAction
    {
        return Run(builder);
    }

    /// Behavior driven style alias for run method.
    /// @param builder The function that builds the test action.
    /// @return The executed test action.
    /// /
    T When<T>(Func<T> builder) where T : ITestAction
    {
        return Run(builder);
    }


    /// Behavior driven style alias for run method.
    /// @param action The test action.
    /// @return The executed test action.
    /// /
    T Then<T>(T action) where T : ITestAction
    {
        return Then(() => action);
    }

    /// Behavior driven style alias for run method.
    /// @param builder The test action builder function.
    /// @return The executed test action.
    /// /
    T Then<T>(Func<T> builder) where T : ITestAction
    {
        return Run(builder);
    }

    T Then<T>(ITestActionBuilder<T> builder) where T : ITestAction
    {
        return Run(builder);
    }

    /// Behavior driven style alias for run method.
    /// @param action The test action to be executed.
    /// @return The executed test action.
    /// /
    T And<T>(T action) where T : ITestAction
    {
        return And(() => action);
    }

    /// Behavior driven style alias for run method.
    /// @param builder Function that builds the test action.
    /// @return The executed test action.
    /// /
    T And<T>(Func<T> builder) where T : ITestAction
    {
        return Run(builder);
    }

    T And<T>(ITestActionBuilder<T> builder) where T : ITestAction
    {
        return Run(builder);
    }
}
