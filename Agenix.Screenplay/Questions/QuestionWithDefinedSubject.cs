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

namespace Agenix.Screenplay.Questions;

/// <summary>
///     Represents a question that includes a specifically defined subject.
/// </summary>
/// <typeparam name="T">The type of the answer provided by this question.</typeparam>
public class QuestionWithDefinedSubject<T>(IQuestion<T> theQuestion, string subject, List<IPerformable> precedingTasks)
    : IQuestion<T>
{
    public QuestionWithDefinedSubject(IQuestion<T> theQuestion, string subject)
        : this(theQuestion, subject, [])
    {
    }

    /// <summary>
    ///     Provides the answer to the question for the given actor after executing any preceding tasks.
    /// </summary>
    /// <param name="actor">The actor who will answer the question and execute any preceding tasks.</param>
    /// <returns>The answer to the question provided by the given actor.</returns>
    public T AnsweredBy(Actor actor)
    {
        actor.AttemptsTo(Actor.ErrorHandlingMode.IGNORE_EXCEPTIONS, precedingTasks.ToArray());
        return theQuestion.AnsweredBy(actor);
    }

    /// <summary>
    ///     Represents the subject or title of the question being asked.
    /// </summary>
    /// <remarks>
    ///     The property provides a description or label that defines the focus of the question.
    ///     It is typically used to make the question more understandable or specific when logging, debugging,
    ///     or presenting it to users.
    /// </remarks>
    public string Subject { get; } = subject;
}
