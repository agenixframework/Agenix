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
///     Convenience class to convert an object into a Question
/// </summary>
public static class TheValue
{
    /// <summary>
    ///     Creates a question that represents the given value.
    /// </summary>
    /// <typeparam name="TAnswer">The type of the value that the question will produce.</typeparam>
    /// <param name="value">The value to represent as a question.</param>
    /// <returns>An IQuestion instance containing the provided value.</returns>
    public static IQuestion<TAnswer> Of<TAnswer>(TAnswer value)
    {
        return new SimpleQuestion<TAnswer>(actor => value);
    }

    /// <summary>
    ///     Creates a question representing the given value.
    /// </summary>
    /// <typeparam name="TAnswer">The type of the value this question will produce.</typeparam>
    /// <param name="value">The value to represent as a question.</param>
    /// <returns>An IQuestion instance containing the supplied value.</returns>
    public static IQuestion<TAnswer> Of<TAnswer>(string subject, TAnswer value)
    {
        return new QuestionWithDefinedSubject<TAnswer>(Of(value), subject);
    }
}

internal class SimpleQuestion<TAnswer> : IQuestion<TAnswer>
{
    private readonly Func<Actor, TAnswer> _answerFunction;

    public SimpleQuestion(Func<Actor, TAnswer> answerFunction)
    {
        _answerFunction = answerFunction;
    }

    public TAnswer AnsweredBy(Actor actor)
    {
        return _answerFunction(actor);
    }
}
