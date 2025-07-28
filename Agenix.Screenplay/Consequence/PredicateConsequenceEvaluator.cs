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

using Agenix.Api.Exceptions;
using Agenix.Api.Util;
using Agenix.Screenplay.Events;
using Agenix.Screenplay.Formatting;

namespace Agenix.Screenplay.Consequence;

/// <summary>
///     The PredicateConsequence class is a specialized consequence designed
///     to evaluate the outcome of an actor's interaction with a specific question
///     against an expected condition, provided as a predicate.
/// </summary>
/// <typeparam name="T">
///     The type of the result or answer produced by the question being evaluated.
/// </typeparam>
public class PredicateConsequence<T> : BaseConsequence<T>
{
    /// <summary>
    ///     Defines a predicate function that evaluates whether a given value meets a specified condition.
    ///     Used to verify the result returned by a question against an expected outcome within a consequence evaluation.
    /// </summary>
    protected readonly Func<T, bool> Expected;

    /// <summary>
    ///     Represents a question that the actor interacts with to return an expected result.
    ///     Used within the context of evaluating predicates on a consequence.
    /// </summary>
    protected readonly IQuestion<T> Question;

    /// <summary>
    ///     Represents the subject of the consequence, typically derived from the associated question and its context.
    ///     Used to format and describe what aspect of the actor's interaction is being evaluated.
    /// </summary>
    protected readonly string Subject;

    /// <summary>
    ///     Represents a consequence that evaluates whether the answer to a given question satisfies a specific predicate
    ///     condition.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the answer or result provided by the question being evaluated.
    /// </typeparam>
    public PredicateConsequence(IQuestion<T> actual, Func<T, bool> expected)
        : this(null, actual, expected)
    {
    }

    /// <summary>
    ///     Represents a consequence that evaluates a predicate condition based on the answer
    ///     to a specified question.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the answer to the question that the predicate evaluates.
    /// </typeparam>
    public PredicateConsequence(string subjectText, IQuestion<T> actual, Func<T, bool> expected)
    {
        Question = actual;
        Expected = expected;
        Subject = QuestionSubject<T>.FromClass(actual.GetType()).AndQuestion(actual).Subject();
        SubjectText = Optional<string>.OfNullable(subjectText);
    }

    /// <summary>
    ///     Evaluates the consequence associated with the specified actor by verifying
    ///     if the predicate condition is satisfied based on the question's answer.
    /// </summary>
    /// <param name="actor">
    ///     The actor performing the evaluation, which provides answers to the question being examined.
    /// </param>
    /// <exception cref="AgenixSystemException">
    ///     Thrown if the predicate condition fails.
    /// </exception>
    /// <exception cref="Exception">
    ///     Thrown when other unexpected errors occur during the evaluation process.
    /// </exception>
    public override void EvaluateFor(Actor actor)
    {
        actor.EventBus!.Publish(new ActorAsksQuestion(Question.Subject, actor.Name));

        try
        {
            PerformSetupActionsAs(actor);
            if (!Expected(Question.AnsweredBy(actor)))
            {
                throw new AgenixSystemException("predicate failed");
            }
        }
        catch (Exception actualError)
        {
            ThrowComplaintTypeErrorIfSpecified(ErrorFrom(actualError));

            ThrowDiagnosticErrorIfProvided(ErrorFrom(actualError));

            throw;
        }
    }

    private void ThrowDiagnosticErrorIfProvided(Exception actualError)
    {
        if (Question is IQuestionDiagnostics diagnostics)
        {
            throw Complaint.From(diagnostics.OnError(), actualError);
        }
    }

    /// <summary>
    ///     Converts the current object to its string representation.
    /// </summary>
    /// <returns>
    ///     A string representation of the object, formatted based on the subject and the expected predicate.
    /// </returns>
    public override string ToString()
    {
        var template = Explanation.OrElse("Then {0} should be {1}");
        var expectedExpression = StripRedundantTerms.From(Expected.ToString());
        return AddRecordedInputValuesTo(string.Format(template, SubjectText.OrElse(Subject), expectedExpression));
    }
}
