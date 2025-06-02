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
using Agenix.Api.Exceptions;

namespace Agenix.Screenplay.Annotations;

/// <summary>
///     Represents a utility class that processes and substitutes placeholders within a given text
///     with corresponding field values from an object.
/// </summary>
public class AnnotatedTitle
{
    private readonly string _text;

    private AnnotatedTitle(string text)
    {
        _text = text;
    }

    /// <summary>
    ///     Creates a new instance of <see cref="AnnotatedTitle" /> with the provided text, which can then be processed further
    ///     to substitute placeholders.
    /// </summary>
    /// <param name="text">The text containing placeholders to be substituted using field values of an object.</param>
    /// <returns>A new instance of <see cref="AnnotatedTitle" /> initialized with the given text.</returns>
    public static AnnotatedTitle InjectFieldsInto(string text)
    {
        return new AnnotatedTitle(text);
    }

    /// <summary>
    ///     Substitutes placeholders in the contained text with corresponding values from the fields of a given object.
    /// </summary>
    /// <param name="question">The object whose field values will be injected into the text.</param>
    /// <returns>The updated text with placeholders replaced by the corresponding field values.</returns>
    public string Using(object question)
    {
        var fields = question.GetType()
            .GetFields(BindingFlags.Instance |
                       BindingFlags.NonPublic |
                       BindingFlags.Public)
            .ToHashSet();

        var updatedText = _text;
        foreach (var field in fields)
        {
            var fieldName = FieldVariableFor(field.Name);
            var value = GetValueFrom(question, field);
            if (updatedText.Contains(fieldName) && value != null)
            {
                updatedText = updatedText.Replace(fieldName, value.ToString());
            }
        }

        return updatedText;
    }

    /// <summary>
    ///     Retrieves the value of a specified field from a given object.
    /// </summary>
    /// <param name="question">The object instance to retrieve the field value from.</param>
    /// <param name="field">The field information from which the value will be fetched.</param>
    /// <returns>The value of the specified field as an object, or throws an exception if retrieval fails.</returns>
    private object GetValueFrom(object question, FieldInfo field)
    {
        try
        {
            return field.GetValue(question);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            throw new AgenixSystemException($"Question label could not be instantiated for {_text}");
        }
    }

    private string FieldVariableFor(string field)
    {
        return "#" + field;
    }
}
