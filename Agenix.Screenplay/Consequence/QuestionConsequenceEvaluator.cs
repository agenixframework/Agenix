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

using Agenix.Api.Util;
using Agenix.Screenplay.Events;
using Agenix.Screenplay.Formatting;
using Agenix.Screenplay.Questions;
using Agenix.Validation.NHamcrest.Validation.Matcher;
using NHamcrest;

namespace Agenix.Screenplay.Consequence;

/// <summary>
///     Represents a consequence in the Screenplay Pattern where the result of answering a question
///     is evaluated against an expected condition. This class facilitates validating that an
///     actor's answer to a specific question meets an expected requirement or condition.
/// </summary>
/// <typeparam name="T">
///     The type of the answer produced by the question being evaluated.
/// </typeparam>
public class QuestionConsequence<T> : BaseConsequence<T>
{
    protected readonly IMatcher<T> expected;
    protected readonly IQuestion<T> question;
    protected readonly string subject;

    /// <summary>
    ///     Represents a consequence in the screenplay pattern where the result of answering a question
    ///     is evaluated against an expected condition.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the result produced by the question being evaluated.
    /// </typeparam>
    public QuestionConsequence(IQuestion<T> actual, IMatcher<T> expected)
        : this(null, actual, expected)
    {
    }

    /// <summary>
    ///     Represents a specific consequence in the "Given/When/Then" Screenplay pattern,
    ///     where a question and its expected result are evaluated for an actor.
    ///     This is used to verify that the actual answer to a question matches the expected condition.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the answer expected from the question.
    /// </typeparam>
    public QuestionConsequence(string subjectText, IQuestion<T> actual, IMatcher<T> expected)
    {
        question = actual;
        this.expected = expected;
        subject = QuestionSubject<T>.FromClass(actual.GetType()).AndQuestion(actual).Subject();
        SubjectText = Optional<string>.OfNullable(subjectText);
    }

    /// <summary>
    ///     Evaluates the expected condition against the result of a question answered by an actor.
    ///     Ensures that the actor's response conforms to the provided matcher condition, facilitating
    ///     validation within the Screenplay Pattern.
    /// </summary>
    /// <param name="actor">
    ///     The actor who answers the question being evaluated.
    /// </param>
    public override async Task EvaluateFor(Actor actor)
    {
        await actor.EventBus.Publish(new ActorAsksQuestion(question.Subject, actor.Name));

        try
        {
            await PerformSetupActionsAs(actor);

            QuestionHints.AddHints(QuestionHints.FromAssertion(expected)).To(question);

            MatcherAssert.AssertThat(await question.AnsweredBy(actor), expected);
        }
        catch (Exception actualError)
        {
            ThrowComplaintTypeErrorIfSpecified(actualError);
            ThrowDiagnosticErrorIfProvided(actualError);
            throw;
        }
    }

    private void ThrowDiagnosticErrorIfProvided(Exception actualError)
    {
        if (question is IQuestionDiagnostics diagnostics)
        {
            throw Complaint.From(diagnostics.OnError(), actualError);
        }
    }

    /// <summary>
    ///     Returns a string representation of the QuestionConsequence instance,
    ///     including its subject and expected value in a formatted manner.
    /// </summary>
    /// <returns>
    ///     A string describing the consequence in terms of its subject and expected value,
    ///     with any recorded input values appended.
    /// </returns>
    public override string ToString()
    {
        var template = Explanation.OrElse("Then %s should be %s");
        var expectedExpression = StripRedundantTerms.From(expected.ToString());
        return AddRecordedInputValuesTo(string.Format(template, SubjectText.OrElse(subject), expectedExpression));
    }
}
