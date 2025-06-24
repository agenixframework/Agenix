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

namespace Agenix.Screenplay.Cast;

/// <summary>
///     Provides simple support for managing Screenplay actors in SpecFlow/ Reqnroll or other BDD frameworks
/// </summary>
public class Casting
{
    private readonly List<Action<Actor>> _abilityProviders;
    private readonly Dictionary<string, Actor> _actors = new();
    private readonly List<IAbility> _standardAbilities;

    public Casting(params IAbility[] abilities)
    {
        _standardAbilities = abilities.ToList();
        _abilityProviders = [];
    }

    public Casting(params Action<Actor>[] providers)
    {
        _standardAbilities = [];
        _abilityProviders = providers.ToList();
    }

    /// <summary>
    ///     Create a Cast object initialized with no predefined abilities.
    /// </summary>
    /// <returns>A new instance of the <see cref="Casting" /> class initialized with no standard abilities.</returns>
    public static Casting OfStandardActors()
    {
        return new Casting(Array.Empty<IAbility>());
    }

    /// <summary>
    ///     Create a Cast object with a list of predefined abilities
    /// </summary>
    public static Casting WhereEveryoneCan(params IAbility[] abilities)
    {
        return new Casting(abilities);
    }

    /// <summary>
    ///     Create a Cast object where each actor is configured using the provided function.
    ///     Example:
    ///     Cast globeTheatreCast = Cast.WhereEveryoneCan(actor => actor.WhoCan(Fetch.Some("Coffee")));
    /// </summary>
    public static Casting WhereEveryoneCan(params Action<Actor>[] abilities)
    {
        return new Casting(abilities);
    }

    /// <summary>
    ///     Retrieves an existing actor with the given name or creates a new one with the specified abilities.
    /// </summary>
    /// <param name="actorName">The name of the actor to retrieve or create.</param>
    /// <param name="abilities">The set of abilities to assign to the actor if newly created.</param>
    /// <returns>An <see cref="Actor" /> instance corresponding to the specified name, with the provided abilities.</returns>
    public Actor ActorNamed(string actorName, params IAbility[] abilities)
    {
        if (_actors.TryGetValue(actorName, out var named))
        {
            return named;
        }

        var newActor = Actor.Named(actorName);

        foreach (var doSomething in abilities)
        {
            newActor.Can(doSomething);
        }

        AssignGeneralAbilitiesTo(newActor);

        _actors[actorName] = newActor;
        return _actors[actorName];
    }

    /// <summary>
    ///     Retrieves a read-only list of all actors managed by the current Cast instance.
    /// </summary>
    /// <returns>
    ///     A read-only list of <see cref="Actor" /> objects.
    /// </returns>
    public IReadOnlyList<Actor> GetActors()
    {
        return _actors.Values.ToList().AsReadOnly();
    }

    /// <summary>
    ///     Dismisses all actors in the cast by invoking their wrap-up actions and clearing the collection of actors.
    /// </summary>
    public void DismissAll()
    {
        foreach (var actor in _actors.Values)
        {
            actor.WrapUp();
        }

        _actors.Clear();
    }

    /// <summary>
    ///     Assigns a set of standard abilities, defined during the initialization of the Cast,
    ///     to the specified actor. Also apply any ability providers defined for the Cast.
    /// </summary>
    /// <param name="newActor">The actor to whom the standard abilities and providers will be assigned.</param>
    protected void AssignGeneralAbilitiesTo(Actor newActor)
    {
        foreach (var ability in _standardAbilities)
        {
            newActor.WhoCan(ability);
        }

        foreach (var provider in _abilityProviders)
        {
            provider(newActor);
        }
    }
}
