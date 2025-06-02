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

using System.Reflection;
using Agenix.Screenplay.Annotations;
using Agenix.Screenplay.Utils;

namespace Agenix.Screenplay;

/// <summary>
///     Represents a class that associates a specific question type with its corresponding subject.
///     Allows for retrieval of a question's subject, deriving its value from the associated question interface,
///     annotations, or a humanized form of the question class name when no other data is provided.
/// </summary>
/// <typeparam name="T">
///     The type parameter representing the expected answer type for the question being described.
/// </typeparam>
public class QuestionSubject<T>(Type questionClass)
{
    private IQuestion<T> _question;

    /// <summary>
    ///     Creates a new instance of the <see cref="QuestionSubject{T}" /> class using the specified question class type.
    ///     This method allows associating a question's class with its corresponding subject representation.
    /// </summary>
    /// <param name="questionClass">
    ///     The <see cref="Type" /> of the question class to be used for deriving the subject or associating with a question
    ///     object.
    /// </param>
    /// <returns>
    ///     A new instance of <see cref="QuestionSubject{T}" /> initialized with the specified question class type.
    /// </returns>
    public static QuestionSubject<T> FromClass(Type questionClass)
    {
        return new QuestionSubject<T>(questionClass);
    }

    /// <summary>
    ///     Adds a new question to the current question subject, allowing for chaining multiple questions.
    ///     This method sets the specified question as the current question for the question subject instance.
    /// </summary>
    /// <param name="question">
    ///     The question of type <see cref="IQuestion{T}" /> to be added to the current question subject.
    /// </param>
    /// <returns>
    ///     The current instance of <see cref="QuestionSubject{T}" /> with the added question, enabling method chaining.
    /// </returns>
    public QuestionSubject<T> AndQuestion(IQuestion<T> question)
    {
        _question = question;
        return this;
    }

    /// <summary>
    ///     Retrieves the value of the subject annotation associated with the question class.
    ///     This includes checking annotations on the "AnsweredBy" method or the class-level "SubjectAttribute",
    ///     selecting the most applicable annotation to provide context for the question subject.
    /// </summary>
    /// <returns>
    ///     A string containing the processed subject annotation if present on the associated class or its method;
    ///     otherwise, null if no relevant annotation is found.
    /// </returns>
    private string? AnnotatedSubject()
    {
        var methodAnnotation = AnnotationOnMethodOf(questionClass);
        if (methodAnnotation != null)
        {
            return methodAnnotation;
        }

        return AnnotatedSubjectFromClass(questionClass);
    }

    /// <summary>
    ///     Retrieves the value of the <c>SubjectAttribute</c> associated with the specified question class,
    ///     or its base types if applicable. If the class or its base types are decorated with a <c>SubjectAttribute</c>,
    ///     its value is processed to provide a subject annotation.
    /// </summary>
    /// <param name="questionClass">
    ///     The <c>Type</c> of the class to check for a <c>SubjectAttribute</c>.
    /// </param>
    /// <returns>
    ///     A string containing the processed subject annotation if a <c>SubjectAttribute</c> is found
    ///     on the specified class or its base types; otherwise, null.
    /// </returns>
    private string? AnnotatedSubjectFromClass(Type questionClass)
    {
        var subjectAttribute = questionClass.GetCustomAttribute<SubjectAttribute>();
        if (subjectAttribute != null)
        {
            return AnnotationOnClass(questionClass);
        }

        if (questionClass.BaseType != null)
        {
            return AnnotatedSubjectFromClass(questionClass.BaseType);
        }

        return null;
    }

    /// <summary>
    ///     Retrieves the value of the <c>SubjectAttribute</c> associated with the "AnsweredBy" method
    ///     of the specified question class.
    ///     If the "AnsweredBy" method is decorated with a <c>SubjectAttribute</c>, its value
    ///     is processed to inject field values from the current question context.
    /// </summary>
    /// <param name="questionClass">
    ///     The <c>Type</c> of the class containing the "AnsweredBy" method to check for the <c>SubjectAttribute</c>.
    /// </param>
    /// <returns>
    ///     A string containing the processed subject annotation with relevant field values injected,
    ///     or null if the "AnsweredBy" method does not have a <c>SubjectAttribute</c>.
    /// </returns>
    private string? AnnotationOnMethodOf(Type questionClass)
    {
        try
        {
            var answeredBy = questionClass.GetMethod("AnsweredBy", [typeof(Actor)]);
            var subjectAttribute = answeredBy?.GetCustomAttribute<SubjectAttribute>();
            if (subjectAttribute != null)
            {
                var annotatedTitle = subjectAttribute.Value;
                return AnnotatedTitle.InjectFieldsInto(annotatedTitle).Using(_question);
            }
        }
        catch (Exception)
        {
            return null;
        }

        return null;
    }

    /// <summary>
    ///     Retrieves the subject annotation associated with the specified class type.
    ///     If the provided class has a <c>SubjectAttribute</c>, its value is processed
    ///     to inject field values from the current question context.
    /// </summary>
    /// <param name="questionClass">
    ///     The <c>Type</c> of the class to retrieve the subject annotation from.
    /// </param>
    /// <returns>
    ///     A string containing the processed subject annotation with any relevant field values injected,
    ///     or null if the class does not have a <c>SubjectAttribute</c>.
    /// </returns>
    private string? AnnotationOnClass(Type questionClass)
    {
        var subjectAttribute = questionClass.GetCustomAttribute<SubjectAttribute>();
        if (subjectAttribute == null)
        {
            return null;
        }

        var annotatedTitle = subjectAttribute.Value;
        return AnnotatedTitle.InjectFieldsInto(annotatedTitle).Using(_question);
    }

    /// <summary>
    ///     Extracts the subject defined in the question interface for the current question.
    ///     If the subject is null or an empty string, returns null.
    /// </summary>
    /// <returns>
    ///     A string representing the subject provided by the question interface if it exists; otherwise, null.
    /// </returns>
    private string? SubjectFromQuestionInterface()
    {
        var subject = _question.Subject;
        return string.IsNullOrEmpty(subject) ? null : subject;
    }

    /// <summary>
    ///     Returns a string representation of the subject associated with the current question.
    ///     The method prioritizes the subject from the question interface, followed by annotated data
    ///     from the question class or method, and ultimately falls back to a humanized version
    ///     of the question class name if no other representations are available.
    /// </summary>
    /// <returns>
    ///     A string value representing the subject, either derived from the question interface,
    ///     annotations, or a humanized form of the question class name.
    /// </returns>
    public string Subject()
    {
        return SubjectFromQuestionInterface() ??
               AnnotatedSubject() ?? NameConverter.Humanize(questionClass.Name).ToLower();
    }
}
