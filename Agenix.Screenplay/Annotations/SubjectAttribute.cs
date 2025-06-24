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

namespace Agenix.Screenplay.Annotations;

/// <summary>
///     Represents a custom attribute used to describe the subject of a class or method.
/// </summary>
/// <remarks>
///     This attribute is intended to annotate classes or methods with a subject, for example,
///     to provide contextual information or to enhance readability and organization
///     within the codebase. The subject is provided as a string value.
/// </remarks>
/// <example>
///     The usage of this attribute can help in categorizing or grouping
///     classes and methods based on a specific theme or topic.
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SubjectAttribute : Attribute
{
    public SubjectAttribute(string value = "")
    {
        Value = value;
    }

    public string Value { get; }
}
