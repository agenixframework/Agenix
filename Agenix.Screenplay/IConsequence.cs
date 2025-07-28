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
///     Represents a consequence that can be evaluated within the context of the Screenplay pattern.
/// </summary>
/// <typeparam name="T">The type of value associated with the consequence.</typeparam>
public interface IConsequence<T>
{
    /// <summary>
    ///     Evaluates the consequence for the specified actor within the context of a screenplay interaction.
    /// </summary>
    /// <param name="actor">The actor for whom the consequence will be evaluated.</param>
    void EvaluateFor(Actor actor);

    /// <summary>
    ///     Specifies an alternative complaint type to use when the consequence is evaluated to an error.
    /// </summary>
    /// <param name="complaintType">The type of complaint or exception to be used when the consequence fails.</param>
    /// <returns>An updated consequence with the specified complaint type configured.</returns>
    IConsequence<T> OrComplainWith(Type complaintType);

    /// <summary>
    ///     Configures the consequence to trigger a complaint of the specified type and includes optional complaint details
    ///     upon evaluation failure.
    /// </summary>
    /// <param name="complaintType">The type of complaint to be triggered, typically represented as an exception type.</param>
    /// <param name="complaintDetails">An optional detailed message or additional information associated with the complaint.</param>
    /// <return>
    ///     The current consequence instance configured with the specified complaint type and details.
    /// </return>
    IConsequence<T> OrComplainWith(Type complaintType, string complaintDetails);

    /// <summary>
    ///     Adds a specified performable action to the list of setup actions for the consequence.
    ///     This action will be attempted during the evaluation of the consequence in the screenplay context.
    /// </summary>
    /// <param name="performable">The performable action to be attempted as part of the consequence.</param>
    /// <returns>
    ///     The current instance of the consequence, enabling chaining of additional setup actions.
    /// </returns>
    IConsequence<T> WhenAttemptingTo(IPerformable performable);

    /// <summary>
    ///     Provides a reason or explanation for the current consequence's evaluation.
    /// </summary>
    /// <param name="explanation">The explanation detailing the reasoning behind the consequence.</param>
    /// <returns>The current consequence instance configured with the provided explanation.</returns>
    IConsequence<T> Because(string explanation);

    /// <summary>
    ///     Evaluate the consequence only after performing the specified tasks.
    /// </summary>
    IConsequence<T> After(params IPerformable[] setupActions);
}
