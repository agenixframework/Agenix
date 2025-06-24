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
///     Represents an evaluator that determines the outcome of iterations based on specified conditions.
/// </summary>
/// <remarks>
///     This class uses a delegate to encapsulate the logic for determining whether a particular iteration meets the
///     specified condition.
/// </remarks>
public class IterationEvaluator(IterationEvaluator.IteratingConditionExpression condition)
{
    /// <summary>
    ///     Delegate that defines the condition used for evaluating each iteration.
    ///     Accepts an index and a <see cref="TestContext" /> and returns a boolean indicating whether the condition is met.
    /// </summary>
    /// <param name="index">The current iteration index.</param>
    /// <param name="context">The context in which the evaluation is performed.</param>
    /// <returns>True if the condition is met, otherwise false.</returns>
    public delegate bool IteratingConditionExpression(int index, TestContext context);

    /// <summary>
    ///     Gets or sets the condition used to evaluate each iteration.
    ///     This condition is a delegate that accepts an index and a <see cref="TestContext" />
    ///     and returns a boolean indicating whether the condition is met.
    /// </summary>
    private IteratingConditionExpression Condition { get; } = condition;

    /// <summary>
    ///     Evaluates a condition based on the provided index and context.
    /// </summary>
    /// <param name="index">The index to use in the condition evaluation.</param>
    /// <param name="context">The context in which the condition is evaluated.</param>
    /// <returns>True if the condition is met, otherwise false.</returns>
    public bool EvaluateCondition(int index, TestContext context)
    {
        return Condition(index, context);
    }
}
