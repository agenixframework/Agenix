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
///     Specifies that a method represents a step in a screenplay-like pattern.
///     This attribute can be applied to methods, which are treated as logical steps
///     in a workflow or behavior.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class StepAttribute : Attribute
{
    /// <summary>
    ///     An attribute used to mark a method as a "step" in the screenplay pattern,
    ///     representing a logical unit of work or behavior within a workflow.
    /// </summary>
    public StepAttribute()
    {
    }

    /// <summary>
    ///     An attribute used to mark a method as a "step" in the screenplay pattern,
    ///     representing a logical unit of work or behavior within a workflow.
    /// </summary>
    public StepAttribute(string value)
    {
        Value = value;
    }

    /// <summary>
    ///     Gets or sets the value associated with the step attribute, representing
    ///     a label or description for the logical step in the screenplay-like pattern.
    /// </summary>
    public string Value { get; set; } = "";
}
