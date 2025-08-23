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

using Agenix.Api.Context;
using Agenix.Screenplay.Exceptions;

namespace Agenix.Screenplay.Abilities;

/// <summary>
///     An ability that provides actors with access to the Agenix TestContext,
///     enabling them to interact with the test framework's context and utilities.
/// </summary>
public class UseTheAgenixTestContext : AbilityWithDefaultDescription, IRefersToActor
{
    /// <summary>
    ///     Initializes a new instance of UseTheAgenixTestContext with the specified TestContext.
    /// </summary>
    /// <param name="testContext">The TestContext to use.</param>
    public UseTheAgenixTestContext(TestContext testContext)
    {
        TestContext = testContext ?? throw new ArgumentNullException(nameof(testContext));
    }

    /// <summary>
    ///     Gets the TestContext associated with this ability.
    /// </summary>
    /// <returns>The TestContext instance.</returns>
    public TestContext TestContext { get; }

    /// <summary>
    ///     Returns this ability as the specified type for the given actor.
    /// </summary>
    /// <typeparam name="T">The type of ability to return.</typeparam>
    /// <param name="actor">The actor using this ability.</param>
    /// <returns>This ability cast to the specified type.</returns>
    public T AsActor<T>(Actor actor) where T : IAbility
    {
        return (T)(IAbility)this;
    }

    /// <summary>
    ///     Gets the UseTheAgenixTestContext ability from the specified actor.
    /// </summary>
    /// <param name="actor">The actor to get the ability from.</param>
    /// <returns>The UseTheAgenixTestContext ability.</returns>
    /// <exception cref="NoMatchingAbilityException">Thrown when the actor doesn't have this ability.</exception>
    public static UseTheAgenixTestContext As(Actor actor)
    {
        var ability = actor.AbilityTo<UseTheAgenixTestContext>();
        if (ability == null)
        {
            throw new NoMatchingAbilityException(
                $"Actor '{actor.Name}' does not have the UseTheAgenixTestContext ability");
        }

        return ability.AsActor<UseTheAgenixTestContext>(actor);
    }
}
