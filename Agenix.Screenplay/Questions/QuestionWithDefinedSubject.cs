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
