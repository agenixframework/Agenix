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
using System.Runtime.CompilerServices;

namespace Agenix.Screenplay;

/// <summary>
///     Provides functionality to copy non-null fields from a source object to a target object.
/// </summary>
public class CopyNonNullProperties
{
    private readonly object source;

    private CopyNonNullProperties(object source)
    {
        this.source = source;
    }

    /// <summary>
    ///     Creates an instance of <see cref="CopyNonNullProperties" /> with the specified source object.
    /// </summary>
    /// <param name="task">The source object from which non-null properties will be copied.</param>
    /// <returns>A new instance of <see cref="CopyNonNullProperties" /> initialized with the specified source object.</returns>
    public static CopyNonNullProperties From(object task)
    {
        return new CopyNonNullProperties(task);
    }

    /// <summary>
    ///     Copies non-null fields from the source object to the specified target object.
    /// </summary>
    /// <param name="target">The target object to which non-null fields from the source object will be copied.</param>
    public void To(object target)
    {
        GetFields(source.GetType())
            .Where(field => !field.IsDefined(typeof(CompilerGeneratedAttribute), false))
            .Where(field => !field.IsStatic)
            .ToList()
            .ForEach(field => CopyFieldValue(field, source, target));
    }

    /// <summary>
    ///     Retrieves all fields from the specified type and its base types.
    /// </summary>
    /// <param name="type">The type from which to retrieve the fields.</param>
    /// <returns>A list of <see cref="FieldInfo" /> objects representing the fields of the specified type and its base types.</returns>
    public static List<FieldInfo> GetFields(Type type)
    {
        var fields = new List<FieldInfo>();
        var typeToInspect = type;

        while (typeToInspect != null)
        {
            fields.AddRange(typeToInspect.GetFields(BindingFlags.Instance |
                                                    BindingFlags.NonPublic |
                                                    BindingFlags.Public |
                                                    BindingFlags.DeclaredOnly));
            typeToInspect = typeToInspect.BaseType;
        }

        return fields;
    }

    /// <summary>
    ///     Copies the value of a specified field from the source object to the target object if the value is not null.
    /// </summary>
    /// <param name="field">The field whose value is to be copied.</param>
    /// <param name="source">The source object containing the field value to copy.</param>
    /// <param name="target">The target object to which the field value will be copied.</param>
    /// <exception cref="ArgumentException">Thrown when there is an error copying the field value.</exception>
    private void CopyFieldValue(FieldInfo field, object source, object target)
    {
        try
        {
            var sourceValue = field.GetValue(source);
            if (sourceValue != null)
            {
                field.SetValue(target, sourceValue);
            }
        }
        catch (Exception ex)
        {
            throw new ArgumentException("Failed to copy field value", ex);
        }
    }
}
