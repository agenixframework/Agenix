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
using NUnit.Framework;

namespace Agenix.Screenplay.Consequence;

/// <summary>
///     Represents a consequence based on a boolean question, derived from the Screenplay pattern.
/// </summary>
/// <typeparam name="T">The type associated with the consequence.</typeparam>
public class BooleanQuestionConsequence<T> : BaseConsequence<T>
{
    private readonly IQuestion<bool> _question;
    private readonly string _subject;

    /// <summary>
    ///     Represents a consequence derived from a boolean question within the Screenplay pattern.
    /// </summary>
    /// <typeparam name="T">The type associated with the consequence, representing the context or target entity.</typeparam>
    public BooleanQuestionConsequence(IQuestion<bool> actual)
        : this(null, actual)
    {
    }

    /// <summary>
    ///     Represents a consequence based on a boolean question, deriving from a base consequence
    ///     and integrating the Screenplay pattern to evaluate actions or assertions.
    /// </summary>
    /// <typeparam name="T">The type associated with the consequence, representing the context or target entity.</typeparam>
    public BooleanQuestionConsequence(string subjectText, IQuestion<bool> actual)
    {
        _question = actual;
        _subject = QuestionSubject<T>.FromClass(actual.GetType())
            .AndQuestion(actual as IQuestion<T>)
            .Subject();
        SubjectText = Optional<string>.Of(subjectText);
    }

    /// <summary>
    ///     Evaluates the consequence for the given actor, verifying the boolean condition
    ///     and throwing appropriate errors if the condition is not met.
    /// </summary>
    /// <param name="actor">The actor for whom the consequence is being evaluated.</param>
    public override void EvaluateFor(Actor actor)
    {
        actor.EventBus!.Publish(new ActorAsksQuestion(_question.Subject, actor.Name));

        try
        {
            PerformSetupActionsAs(actor);
            Assert.That(_question.AnsweredBy(actor), Is.True, Reason());
        }
        catch (Exception actualError)
        {
            var error = ErrorFrom(actualError);
            ThrowComplaintTypeErrorIfSpecified(error);
            ThrowDiagnosticErrorIfProvided(error);
            throw;
        }
    }

    private string Reason()
    {
        return "Expected " + QuestionSubject<T>.FromClass(_question.GetType())
            .AndQuestion(_question as IQuestion<T>)
            .Subject();
    }

    private void ThrowDiagnosticErrorIfProvided(Exception actualError)
    {
        if (_question is IQuestionDiagnostics diagnostics)
        {
            throw Complaint.From(diagnostics.OnError(), actualError);
        }
    }

    /// <summary>
    ///     Returns a string representation of the BooleanQuestionConsequence including
    ///     the subject text or a default template if the subject text is not defined.
    /// </summary>
    /// <returns>A string representation of the consequence with relevant details.</returns>
    public override string ToString()
    {
        var template = Explanation.OrElse("Then {0}");
        return AddRecordedInputValuesTo(string.Format(template, SubjectText.OrElse(_subject)));
    }
}
