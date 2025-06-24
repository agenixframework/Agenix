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

namespace Agenix.Screenplay;

/// <summary>
///     Represents an actionable behavior or operation that can be performed within a screenplay context.
/// </summary>
public interface Interaction : IPerformable
{
    /// <summary>
    ///     Creates an instance of <see cref="AnonymousInteraction" /> that encapsulates a performable operation with a
    ///     specified title and a list of steps.
    /// </summary>
    /// <param name="title">The descriptive title of the interaction, representing its purpose or intent.</param>
    /// <param name="steps">An array of <see cref="IPerformable" /> tasks composing the interaction.</param>
    /// <returns>An instance of <see cref="AnonymousInteraction" /> configured with the specified title and steps.</returns>
    public static AnonymousInteraction Where(string title, params IPerformable[] steps)
    {
        return Instrumented.InstanceOf<AnonymousInteraction>()
            .WithProperties(title, steps.ToList());
    }

    /// <summary>
    ///     Creates an instance of <see cref="AnonymousInteraction" /> that encapsulates a performable operation with a
    ///     specified title and operation.
    /// </summary>
    /// <param name="title">The title of the interaction, representing its purpose or description.</param>
    /// <param name="performableOperation">The performable operation defined as an action involving an <see cref="Actor" />.</param>
    /// <returns>
    ///     An instance of <see cref="AnonymousInteraction" /> configured with the specified title and performable
    ///     operation.
    /// </returns>
    public static AnonymousInteraction Where(string title, Action<Actor> performableOperation)
    {
        return Instrumented.InstanceOf<AnonymousInteraction>()
            .WithProperties(title, performableOperation);
    }

    /// <summary>
    ///     Creates an instance of <see cref="AnonymousInteraction" /> that encapsulates a performable operation with a
    ///     specified title.
    /// </summary>
    /// <param name="title">The title of the interaction, representing its purpose or description.</param>
    /// <param name="performableOperation">The performable operation to be executed as part of the interaction.</param>
    /// <returns>
    ///     An instance of <see cref="AnonymousInteraction" /> configured with the specified title and performable
    ///     operation.
    /// </returns>
    public static AnonymousInteraction ThatPerforms(string title, Action performableOperation)
    {
        return Instrumented.InstanceOf<AnonymousInteraction>()
            .WithProperties(title, performableOperation);
    }
}
