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

/// Interface that extends ITestActionRunner to support behavior-driven development (BDD) style methods.
/// Methods include Given, When, Then, And as aliases to represent different stages of the Gherkin syntax in BDD.
/// /
public interface IGherkinAsyncTestActionRunner : IAsyncTestActionRunner
{
    /// Behavior-driven style alias for run method.
    /// @param action The test action.
    /// @typeparam T The type of the test action.
    /// @return The executed test action.
    /// /
    async Task<T> Given<T>(T action) where T : IAsyncTestAction
    {
        return await Given(() => action);
    }

    /// Behavior-driven style alias for the run method, representing the "Given" stage in
    /// behavior-driven development (BDD).
    /// <param name="builder">The test action to be executed.</param>
    /// <typeparam name="T">The type of the test action.</typeparam>
    /// <returns>The executed test action.</returns>
    async Task<T> Given<T>(Func<T> builder) where T : IAsyncTestAction
    {
        return await Run(builder);
    }

    /// Executes the "Given" step in a behavior-driven development (BDD) style.
    /// This method runs the provided asynchronous test action built using the specified builder.
    /// <param name="builder">The builder responsible for constructing the asynchronous test action to be executed.</param>
    /// <typeparam name="T">The type of the asynchronous test action to be executed.</typeparam>
    /// <returns>A task representing the asynchronous operation that yields the executed test action.</returns>
    async Task<T> Given<T>(IAsyncTestActionBuilder<T> builder) where T : IAsyncTestAction
    {
        return await Run(builder);
    }

    /// Behavior-driven style alias for run method.
    /// @param action The test action.
    /// @return The executed test action.
    /// /
    async Task<T> When<T>(T action) where T : IAsyncTestAction
    {
        return await When(() => action);
    }

    /// Executes the "When" stage of a behavior-driven development (BDD) test using the specified test action builder.
    /// Represents the condition or event that triggers the behavior being tested.
    /// <param name="builder">The builder used to construct the asynchronous test action.</param>
    /// <typeparam name="T">The type of the asynchronous test action.</typeparam>
    /// <return>A task that represents the asynchronous execution of the test action, returning the constructed test action.</return>
    async Task<T> When<T>(IAsyncTestActionBuilder<T> builder) where T : IAsyncTestAction
    {
        return await Run(builder);
    }

    /// Behavior-driven style alias for run method.
    /// @param builder The function that builds the test action.
    /// @return The executed test action.
    /// /
    async Task<T> When<T>(Func<T> builder) where T : IAsyncTestAction
    {
        return await Run(builder);
    }

    /// Behavior-driven style alias for run method.
    /// @param action The test action.
    /// @return The executed test action.
    /// /
    async Task<T> Then<T>(T action) where T : IAsyncTestAction
    {
        return await Then(() => action);
    }

    /// Behavior-driven style alias for run method.
    /// @param builder The test action builder function.
    /// @return The executed test action.
    /// /
    async Task<T> Then<T>(Func<T> builder) where T : IAsyncTestAction
    {
        return await Run(builder);
    }

    /// Executes a test action builder in the "Then" stage of a behavior-driven development (BDD) process.
    /// Represents the outcome verification or assertion stage of the Gherkin syntax.
    /// <param name="builder">The asynchronous test action builder to execute.</param>
    /// <typeparam name="T">The type of the asynchronous test action.</typeparam>
    /// <returns>The executed asynchronous test action.</returns>
    async Task<T> Then<T>(IAsyncTestActionBuilder<T> builder) where T : IAsyncTestAction
    {
        return await Run(builder);
    }

    /// Behavior-driven style alias for run method.
    /// @param action The test action to be executed.
    /// @return The executed test action.
    /// /
    async Task<T> And<T>(T action) where T : IAsyncTestAction
    {
        return await And(() => action);
    }

    /// Behavior-driven style alias for run method.
    /// @param builder Function that builds the test action.
    /// @return The executed test action.
    /// /
    async Task<T> And<T>(Func<T> builder) where T : IAsyncTestAction
    {
        return await Run(builder);
    }

    /// Executes a subsequent action in the context of a behavior-driven development (BDD) "And" step.
    /// <param name="builder">The builder that defines the asynchronous test action.</param>
    /// <typeparam name="T">The type of the asynchronous test action.</typeparam>
    /// <return>The task representing the asynchronous execution of the test action.</return>
    async Task<T> And<T>(IAsyncTestActionBuilder<T> builder) where T : IAsyncTestAction
    {
        return await Run(builder);
    }
}
