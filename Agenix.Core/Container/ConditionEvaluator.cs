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

namespace Agenix.Core.Container;

/// <summary>
///     The <c>ConditionEvaluator</c> class is responsible for evaluating condition expressions against a given context.
/// </summary>
public class ConditionEvaluator(ConditionEvaluator.ConditionExpression condition)
{
    /// <summary>
    ///     Represents a delegate that defines a condition expression for evaluation.
    /// </summary>
    /// <param name="context">The context within which the condition expression is evaluated.</param>
    /// <returns>Returns true if the condition is met, otherwise false.</returns>
    public delegate bool ConditionExpression(TestContext context);

    /// <summary>
    ///     Represents a condition expression that can be evaluated within a specified context.
    /// </summary>
    /// <remarks>
    ///     This delegate is used to encapsulate a method that takes a <see cref="TestContext" /> object as a parameter
    ///     and returns a boolean value indicating whether the condition is met.
    /// </remarks>
    private ConditionExpression Condition { get; } = condition;

    /// <summary>
    ///     Evaluates the given condition expression using the provided context.
    /// </summary>
    /// <param name="context">The context in which the condition is evaluated.</param>
    /// <returns>Returns true if the condition is met, otherwise false.</returns>
    public bool Evaluate(TestContext context)
    {
        return Condition(context);
    }
}
