#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
