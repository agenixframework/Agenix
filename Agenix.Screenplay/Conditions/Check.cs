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

using Agenix.Screenplay.Questions;
using NHamcrest;

namespace Agenix.Screenplay.Conditions;

/// <summary>
///     Provides utility methods for creating conditional performable actions based on simple
///     boolean conditions, questions, or questions matched against specific expectations.
///     This class facilitates branching logic for actor tasks in screenplay patterns.
/// </summary>
public static class Check
{
    /// <summary>
    ///     Creates a conditional performable action based on a boolean condition.
    /// </summary>
    /// <param name="condition">The boolean condition to be evaluated for determining the performable action.</param>
    /// <returns>An instance of <see cref="ConditionalPerformable" /> that performs tasks based on the given condition.</returns>
    public static ConditionalPerformable Whether(bool condition)
    {
        return new ConditionalPerformableOnBoolean(condition);
    }

    /// <summary>
    ///     Creates a conditional performable action based on the evaluation of a boolean question.
    /// </summary>
    /// <param name="condition">The question that evaluates to a boolean and determines the performable action.</param>
    /// <returns>
    ///     An instance of <see cref="ConditionalPerformable" /> to execute tasks based on the evaluation of the provided
    ///     question.
    /// </returns>
    public static ConditionalPerformable Whether(IQuestion<bool> condition)
    {
        return new ConditionalPerformableOnQuestion(condition);
    }

    /// <summary>
    ///     Creates a conditional performable action based on evaluating a question against a specified matcher.
    /// </summary>
    /// <param name="question">The question to be answered by an actor, whose result will be matched against the given matcher.</param>
    /// <param name="matcher">The matcher that determines whether the question's result meets the expected condition.</param>
    /// <typeparam name="T">The type of the answer produced by the question.</typeparam>
    /// <returns>
    ///     An instance of <see cref="ConditionalPerformable" /> that evaluates the question's answer and performs
    ///     conditional tasks accordingly.
    /// </returns>
    public static ConditionalPerformable Whether<T>(IQuestion<T> question, IMatcher<T> matcher)
    {
        var condition = IQuestion<bool>.Create(actor =>
        {
            var actual = question.AnsweredBy(actor).GetAwaiter().GetResult();
            return matcher.Matches(actual);
        });

        return new ConditionalPerformableOnQuestion(condition);
    }
}
