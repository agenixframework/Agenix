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

namespace Agenix.Screenplay;

/// <summary>
///     Represents a function that creates a question answer based on an actor.
/// </summary>
/// <typeparam name="T">The type of answer returned by the question.</typeparam>
/// <param name="actor">The actor who will answer the question.</param>
/// <returns>The answer to the question.</returns>
public delegate T QuestionAnswerProvider<T>(Actor actor);

/// <summary>
///     Builder class for constructing questions with a specific subject in a Screenplay pattern.
/// </summary>
public class QuestionBuilder(string subject)
{
    /// <summary>
    ///     Associates a subject to a question and returns a new question instance with the defined subject.
    /// </summary>
    /// <typeparam name="T">The type of the answer returned by the question.</typeparam>
    /// <param name="questionToAsk">The question to be associated with the subject.</param>
    /// <returns>A new question instance with the defined subject.</returns>
    public IQuestion<T> AnsweredBy<T>(IQuestion<T> questionToAsk)
    {
        return new QuestionWithDefinedSubject<T>(questionToAsk, subject);
    }

    /// <summary>
    ///     Creates a question from a delegate function that provides the answer.
    /// </summary>
    /// <typeparam name="T">The type of the answer returned by the question.</typeparam>
    /// <param name="answerProvider">A delegate function that provides the answer when given an actor.</param>
    /// <returns>A new question instance with the defined subject.</returns>
    public IQuestion<T> AnsweredBy<T>(QuestionAnswerProvider<T> answerProvider)
    {
        return new DelegateQuestion<T>(answerProvider, subject);
    }

    /// <summary>
    ///     Represents a question implemented using a delegate function.
    /// </summary>
    /// <typeparam name="T">The type of the answer provided by this question.</typeparam>
    private class DelegateQuestion<T> : IQuestion<T>
    {
        private readonly QuestionAnswerProvider<T> _answerProvider;

        /// <summary>
        ///     Initializes a new instance of the DelegateQuestion class.
        /// </summary>
        /// <param name="answerProvider">The delegate function that provides the answer.</param>
        /// <param name="subject">The subject of the question.</param>
        public DelegateQuestion(QuestionAnswerProvider<T> answerProvider, string subject)
        {
            _answerProvider = answerProvider ?? throw new ArgumentNullException(nameof(answerProvider));
            Subject = subject ?? string.Empty;
        }

        /// <summary>
        ///     Gets the answer to the question for the given actor.
        /// </summary>
        /// <param name="actor">The actor who will answer the question.</param>
        /// <returns>The answer to the question.</returns>
        public T AnsweredBy(Actor actor)
        {
            return _answerProvider(actor);
        }

        /// <summary>
        ///     Gets the subject of the question.
        /// </summary>
        public string Subject { get; }
    }
}
