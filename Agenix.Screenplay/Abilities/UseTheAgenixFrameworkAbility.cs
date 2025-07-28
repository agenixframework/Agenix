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

using Agenix.Core;
using Agenix.Screenplay.Exceptions;

namespace Agenix.Screenplay.Abilities;

/// <summary>
///     An ability that provides actors with access to the Agenix framework,
///     enabling them to interact with the Agenix context, instance manager, and core functionality.
/// </summary>
public class UseTheAgenixFramework : AbilityWithDefaultDescription, IRefersToActor
{
    /// <summary>
    ///     Initializes a new instance of UseTheAgenixFramework with the default or current Agenix instance.
    /// </summary>
    public UseTheAgenixFramework()
    {
        AgenixInstance = AgenixInstanceManager.GetOrDefault();
    }

    /// <summary>
    ///     Initializes a new instance of UseTheAgenixFramework with a specific Agenix instance.
    /// </summary>
    /// <param name="agenixInstance">The Agenix instance to use.</param>
    public UseTheAgenixFramework(Core.Agenix agenixInstance)
    {
        AgenixInstance = agenixInstance ?? throw new ArgumentNullException(nameof(agenixInstance));
    }

    /// <summary>
    ///     Gets the Agenix framework instance.
    /// </summary>
    public Core.Agenix AgenixInstance { get; }

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
    ///     Gets the UseTheAgenixFramework ability from the specified actor.
    /// </summary>
    /// <param name="actor">The actor to get the ability from.</param>
    /// <returns>The UseTheAgenixFramework ability.</returns>
    /// <exception cref="NoMatchingAbilityException">Thrown when the actor doesn't have this ability.</exception>
    public static UseTheAgenixFramework As(Actor actor)
    {
        var ability = actor.AbilityTo<UseTheAgenixFramework>();
        if (ability == null)
        {
            throw new NoMatchingAbilityException(
                $"Actor '{actor.Name}' does not have the UseTheAgenixFramework ability");
        }

        return ability.AsActor<UseTheAgenixFramework>(actor);
    }

    /// <summary>
    ///     Creates a new UseTheAgenixFramework ability with the default Agenix instance.
    /// </summary>
    /// <returns>A new UseTheAgenixFramework ability.</returns>
    public static UseTheAgenixFramework WithDefaultInstance()
    {
        return new UseTheAgenixFramework();
    }

    /// <summary>
    ///     Creates a new UseTheAgenixFramework ability with a specific Agenix instance.
    /// </summary>
    /// <param name="agenixInstance">The Agenix instance to use.</param>
    /// <returns>A new UseTheAgenixFramework ability.</returns>
    public static UseTheAgenixFramework WithInstance(Core.Agenix agenixInstance)
    {
        return new UseTheAgenixFramework(agenixInstance);
    }
}
