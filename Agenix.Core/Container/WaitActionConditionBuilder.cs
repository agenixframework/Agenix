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
using Agenix.Core.Condition;

namespace Agenix.Core.Container;

/// Provides a builder for constructing a WaitActionCondition. Inherits from WaitConditionBuilder
/// and specializes the process for handling ActionConditions. This class is used to define
/// the action that will be tested and waited on, similar to other condition-based wait builders in the system.
public class WaitActionConditionBuilder : WaitConditionBuilder<ActionCondition, WaitActionConditionBuilder>
{
    /// Constructs an instance of WaitActionConditionBuilder using the specified builder.
    /// @param builder The builder for constructing the WaitActionConditionBuilder instance.
    /// /
    public WaitActionConditionBuilder(Wait.Builder<ActionCondition> builder) : base(builder)
    {
    }

    /// Sets the test action to execute and wait for.
    /// @param action The test action to be set.
    /// @return The updated WaitActionConditionBuilder with the specified test action set.
    /// /
    public WaitActionConditionBuilder Action(ITestAction action)
    {
        GetCondition().SetAction(action);
        return this;
    }

    /// Sets the test action using the specified action builder and returns the updated WaitActionConditionBuilder.
    /// @param action The test action builder that will provide the test action to be set.
    /// @return The updated WaitActionConditionBuilder with the specified test action set.
    /// /
    public WaitActionConditionBuilder Action(ITestActionBuilder<ITestAction> action)
    {
        GetCondition().SetAction(action.Build());
        return this;
    }
}
