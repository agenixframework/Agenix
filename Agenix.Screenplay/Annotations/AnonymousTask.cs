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
///     Represents an anonymous task that can be performed by an actor.
/// </summary>
/// <remarks>
///     AnonymousTask extends the functionality of <see cref="AnonymousPerformable" />
///     by implementing the <see cref="ITask" /> interface. This allows defining tasks
///     with a title, steps, and custom field values to be used within a screenplay-style
///     testing framework.
/// </remarks>
public class AnonymousTask : AnonymousPerformable, ITask
{
    public AnonymousTask()
    {
    }

    public AnonymousTask(string title, List<IPerformable> steps)
        : base(title, steps)
    {
    }

    /// <summary>
    ///     Sets a custom field value for the current AnonymousTask instance.
    /// </summary>
    /// <param name="fieldName">The name of the field to be set.</param>
    /// <returns>
    ///     An instance of <see cref="AnonymousPerformableFieldSetter{AnonymousTask}" />
    ///     that enables chaining to specify the field value.
    /// </returns>
    public AnonymousPerformableFieldSetter<AnonymousTask> With(string fieldName)
    {
        return new AnonymousPerformableFieldSetter<AnonymousTask>(this, fieldName);
    }
}
