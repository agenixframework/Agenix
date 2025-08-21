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
///     Represents a concrete implementation of a performable action that is conditionally executed
///     based on the evaluation of a given question. This class allows specifying the desired tasks
///     to perform if the condition evaluates to true or false.
/// </summary>
/// <remarks>
///     The provided question is evaluated for a given actor, and based on the result, one or more
///     specified actions are performed. This class helps in creating dynamic, conditional behavior
///     in the context of the Screenplay pattern.
/// </remarks>
public class ConditionalPerformableOnQuestion : ConditionalPerformable
{
    private readonly IQuestion<bool> _condition;

    /// <summary>
    ///     Represents a performable action conditionally executed based on the result
    ///     of evaluating a question against a specific actor.
    /// </summary>
    public ConditionalPerformableOnQuestion(IQuestion<bool> condition)
    {
        _condition = condition;
    }

    /// <summary>
    ///     Evaluates a condition based on a question for a given actor.
    /// </summary>
    /// <param name="actor">The actor for whom the condition is evaluated.</param>
    /// <returns>A boolean value indicating if the condition is satisfied for the actor.</returns>
    protected override bool EvaluatedConditionFor(Actor actor)
    {
        return _condition.AnsweredBy(actor).GetAwaiter().GetResult();
    }
}
