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

namespace Agenix.Screenplay.Conditions;

/// <summary>
///     Represents a conditional performable action based on a boolean value.
///     This class evaluates the provided boolean condition and determines
///     whether associated actions should be executed.
/// </summary>
public class ConditionalPerformableOnBoolean : ConditionalPerformable
{
    private readonly bool _condition;

    /// <summary>
    ///     Represents a conditional action that can be performed based on a provided boolean value.
    ///     This class checks the boolean condition and performs tasks accordingly.
    /// </summary>
    public ConditionalPerformableOnBoolean(bool condition)
    {
        _condition = condition;
    }

    /// <summary>
    ///     Evaluates the condition for a specified actor to determine whether an action should be performed.
    /// </summary>
    /// <param name="actor">The actor for whom the condition is being evaluated.</param>
    /// <returns>A boolean value indicating whether the condition is met for the provided actor.</returns>
    protected override bool EvaluatedConditionFor(Actor actor)
    {
        return _condition;
    }
}
