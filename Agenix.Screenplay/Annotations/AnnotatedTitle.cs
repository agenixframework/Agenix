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
