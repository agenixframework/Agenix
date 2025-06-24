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

public class CopyNonNullProperties
{
    private readonly object source;

    private CopyNonNullProperties(object source)
    {
        this.source = source;
    }

    public static CopyNonNullProperties From(object task)
    {
        return new CopyNonNullProperties(task);
    }

    public void To(object target)
    {
        GetFields(source.GetType())
            .Where(field => !field.IsDefined(typeof(CompilerGeneratedAttribute), false))
            .Where(field => !field.IsStatic)
            .ToList()
            .ForEach(field => CopyFieldValue(field, source, target));
    }

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
