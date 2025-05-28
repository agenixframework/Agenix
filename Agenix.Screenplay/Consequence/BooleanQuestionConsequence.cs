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

using Agenix.Api.Util;
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

    public BooleanQuestionConsequence(IQuestion<bool> actual)
        : this(null, actual)
    {
    }

    public BooleanQuestionConsequence(string subjectText, IQuestion<bool> actual)
    {
        _question = actual;
        _subject = QuestionSubject<T>.FromClass(actual.GetType())
            .AndQuestion(actual as IQuestion<T>)
            .Subject();
        SubjectText = Optional<string>.Of(subjectText);
    }

    public override void EvaluateFor(Actor actor)
    {
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
        if (_question is IQuestionDiagnostics diagnostics) throw Complaint.From(diagnostics.OnError(), actualError);
    }

    public override string ToString()
    {
        var template = Explanation.OrElse("Then {0}");
        return AddRecordedInputValuesTo(string.Format(template, SubjectText.OrElse(_subject)));
    }
}
