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

using Agenix.Api.Context;

namespace Agenix.Api.Condition;

/// Tests whether a condition is satisfied.
public interface ICondition
{
    /// Gets the condition name.
    /// @return The name of the condition.
    /// /
    string GetName();

    /// Tests the condition and returns true if it is satisfied.
    /// <param name="context">The test context in which the condition is evaluated.</param>
    /// <return>True if the condition is satisfied; otherwise, false.</return>
    bool IsSatisfied(TestContext context);

    /// Constructs a success message for the current condition.
    /// <param name="context">The test context used to evaluate the condition.</param>
    /// <return>A success message indicating that the condition has been satisfied.</return>
    string GetSuccessMessage(TestContext context);

    /// Constructs an error message for the condition.
    /// <param name="context">The test context related to the condition.</param>
    /// <return>The error message specific to this condition.</return>
    string GetErrorMessage(TestContext context);
}
