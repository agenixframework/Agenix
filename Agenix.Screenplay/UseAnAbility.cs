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

using Agenix.Screenplay.Exceptions;

namespace Agenix.Screenplay;

/// <summary>
///     A factory to get an Ability of an Actor, when we don't care what it is specifically, just
///     that it can do what we expect it to.
///     Example Usage:
///     UseAnAbility.Of(actor).That&lt;ICanLevitate&gt;().Hover()
/// </summary>
public class UseAnAbility
{
    private readonly Actor _actor;

    private UseAnAbility(Actor actor)
    {
        _actor = actor;
    }

    /// <summary>
    ///     Creates an instance of the UseAnAbility class for the specified actor, allowing access to abilities the actor
    ///     possesses.
    /// </summary>
    /// <param name="actor">The actor whose abilities are to be accessed.</param>
    /// <returns>An instance of the UseAnAbility class associated with the specified actor.</returns>
    public static UseAnAbility Of(Actor actor)
    {
        return new UseAnAbility(actor);
    }

    /// <summary>
    ///     If the actor has an Ability that implements the Interface, return that. If
    ///     there are multiple candidate Abilities, the first one found will be returned.
    /// </summary>
    /// <typeparam name="T">The interface type that we expect to find</typeparam>
    /// <returns>The Ability that implements the interface</returns>
    /// <exception cref="NoMatchingAbilityException">Thrown when no matching ability is found</exception>
    public T That<T>() where T : class
    {
        var ability = _actor.GetAbilityThatExtends<T>();

        if (ability == null)
        {
            throw new NoMatchingAbilityException(
                $"{_actor} does not have an Ability that extends {typeof(T).Name}");
        }

        return ability;
    }
}
