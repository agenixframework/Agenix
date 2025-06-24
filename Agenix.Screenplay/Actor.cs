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

public class Actor : IPerformsTasks
{
    /// <summary>
    ///     Specifies modes for handling errors during operations or task execution.
    /// </summary>
    public enum ErrorHandlingMode
    {
        THROW_EXCEPTION_ON_FAILURE,
        IGNORE_EXCEPTIONS
    }

    private readonly IDictionary<Type, IAbility> _abilities = new Dictionary<Type, IAbility>();
    private readonly IDictionary<string, object> _notepad = new Dictionary<string, object>();
    private string _preferredPronoun;

    public Actor(string name)
    {
        Name = name;
    }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public string NameOrPronoun => _preferredPronoun != null ? _preferredPronoun : Name;

    public void AttemptsTo<T>(params T[] todos) where T : IPerformable
    {
        AttemptsTo(ErrorHandlingMode.THROW_EXCEPTION_ON_FAILURE, todos.Cast<IPerformable>().ToArray());
    }

    public ANSWER AsksFor<ANSWER>(IQuestion<ANSWER> question)
    {
        return question.AnsweredBy(this);
    }

    /// <summary>
    ///     Add all the remembered items for the current actor to the other actor's memory
    /// </summary>
    /// <param name="otherActor">The actor to brief</param>
    public void Brief(Actor otherActor)
    {
        foreach (var item in _notepad)
        {
            otherActor._notepad.Add(item.Key, item.Value);
        }
    }

    /// <summary>
    ///     Creates a new actor with the specified name.
    ///     This actor will have no abilities initially, so you will need to assign some abilities using the whoCan() method.
    /// </summary>
    /// <param name="name">The name to assign to the new actor.</param>
    /// <returns>A new instance of the <see cref="Actor" /> class with the specified name.</returns>
    public static Actor Named(string name)
    {
        return new Actor(name);
    }

    /// <summary>
    ///     Sets a description for the actor.
    /// </summary>
    /// <param name="description">The description to assign to the actor.</param>
    /// <returns>The current actor instance with the updated description.</returns>
    public Actor DescribedAs(string description)
    {
        Description = description;
        return this;
    }

    /// <summary>
    ///     Assigns an ability to the actor, enabling the actor to use it in tasks or actions.
    /// </summary>
    /// <typeparam name="T">The type of ability to be assigned, implementing <see cref="IAbility" />.</typeparam>
    /// <param name="doSomething">The ability object to assign to the actor.</param>
    /// <returns>The current instance of the <see cref="Actor" /> class for chaining further actions.</returns>
    public Actor Can<T>(T doSomething) where T : IAbility
    {
        if (doSomething is IRefersToActor refersToActor)
        {
            refersToActor.AsActor<T>(this);
        }

        _abilities[doSomething.GetType()] = doSomething;

        return this;
    }

    /// <summary>
    ///     Assign an ability to an actor.
    /// </summary>
    public Actor WhoCan<T>(T doSomething) where T : IAbility
    {
        return Can(doSomething);
    }

    /// <summary>
    ///     Gets an ability of the specified type.
    ///     If not found directly, searches for an ability that extends the specified type.
    /// </summary>
    /// <typeparam name="T">The type of ability to retrieve must implement IAbility</typeparam>
    /// <returns>The requested ability or null if not found</returns>
    public T AbilityTo<T>() where T : class, IAbility
    {
        if (_abilities.TryGetValue(typeof(T), out var ability))
        {
            return ability as T;
        }

        return GetAbilityThatExtends<T>();
    }

    /// <summary>
    ///     A more readable way to access an actor's abilities.
    /// </summary>
    /// <typeparam name="T">The type of ability to retrieve must implement IAbility</typeparam>
    /// <returns>The requested ability</returns>
    public T UsingAbilityTo<T>() where T : class, IAbility
    {
        return AbilityTo<T>();
    }

    /// <summary>
    ///     A method used to declare that an actor is now the actor in the spotlight,
    ///     without having them perform any tasks.
    /// </summary>
    public void EntersTheScene()
    {
    }

    /// <summary>
    ///     Indicates that the actor has certain tasks to perform.
    /// </summary>
    /// <param name="todos">The tasks to perform</param>
    public void Has(params IPerformable[] todos)
    {
        AttemptsTo(todos);
    }

    /// <summary>
    ///     Returns a list of all abilities that implement IHasTeardown.
    /// </summary>
    /// <returns>A list of abilities that can be torn down</returns>
    public IList<IHasTeardown> GetTeardowns()
    {
        return _abilities.Values
            .OfType<IHasTeardown>()
            .ToList();
    }

    /// <summary>
    ///     Return an ability that extends the given class. Can be a Superclass or an Interface.
    ///     If there are multiple candidate Abilities, the first one found will be returned.
    /// </summary>
    /// <typeparam name="T">The type of ability to find</typeparam>
    /// <param name="extendedType">The Interface type that we expect to find</param>
    /// <returns>The matching Ability cast to extendedType or null if none match</returns>
    public T? GetAbilityThatExtends<T>() where T : class
    {
        // See if any ability extends or implements T
        return (from entry in _abilities
                let abilityType = entry.Key
                where typeof(T).IsAssignableFrom(abilityType)
                select entry.Value as T).FirstOrDefault();
    }


    /// <summary>
    ///     A tense-neutral synonym for AddFact() for use with Given() clauses
    /// </summary>
    /// <param name="todos">The tasks that the actor was able to perform</param>
    public void WasAbleTo(params IPerformable[] todos)
    {
        AttemptsTo(todos);
    }

    /// <summary>
    ///     Remembers the answer to a question by storing it in the actor's notepad.
    /// </summary>
    /// <typeparam name="TAnswer">The type of the answer to store</typeparam>
    /// <param name="key">The key under which to store the answer</param>
    /// <param name="question">The question to ask</param>
    public void Remember<TAnswer>(string key, IQuestion<TAnswer> question)
    {
        var answer = AsksFor(question);
        _notepad[key] = answer;
    }

    /// <summary>
    ///     Stores a value in the actor's notepad under the specified key.
    /// </summary>
    public void Remember(string key, object value)
    {
        _notepad[key] = value;
    }

    /// <summary>
    ///     Retrieves a value from the actor's notepad by key.
    /// </summary>
    public T Recall<T>(string key)
    {
        return (T)_notepad[key];
    }

    /// <summary>
    ///     Returns a copy of all stored key-value pairs from the notepad.
    /// </summary>
    public IDictionary<string, object> RecallAll()
    {
        return new Dictionary<string, object>(_notepad);
    }

    /// <summary>
    ///     Removes and returns a value from the actor's notepad by key.
    /// </summary>
    public T Forget<T>(string key)
    {
        var value = (T)_notepad[key];
        _notepad.Remove(key);
        return value;
    }

    /// <summary>
    ///     Alternative syntax for recalling what the actor saw.
    /// </summary>
    public T SawAsThe<T>(string key)
    {
        return Recall<T>(key);
    }

    /// <summary>
    ///     Alternative syntax for recalling what the actor gave.
    /// </summary>
    public T GaveAsThe<T>(string key)
    {
        return Recall<T>(key);
    }

    /// <summary>
    ///     Sets the preferred pronoun for the actor.
    /// </summary>
    /// <param name="pronoun">The pronoun to assign to the actor.</param>
    /// <returns>The current actor instance with the updated pronoun.</returns>
    public Actor UsingPronoun(string pronoun)
    {
        _preferredPronoun = pronoun;
        return this;
    }

    /// <summary>
    ///     Removes any preferred pronoun for the actor.
    /// </summary>
    /// <returns>The current actor instance with no preferred pronoun.</returns>
    public Actor WithNoPronoun()
    {
        _preferredPronoun = null;
        return this;
    }

    /// <summary>
    ///     Performs cleanup operations by running teardowns.
    /// </summary>
    public void WrapUp()
    {
        foreach (var teardown in GetTeardowns())
        {
            teardown.TearDown();
        }
    }

    /// <summary>
    ///     Assigns a new name to the actor.
    /// </summary>
    /// <param name="name">The new name to be assigned to the actor.</param>
    public void AssignName(string name)
    {
        Name = name;
    }

    public void AttemptsTo(ErrorHandlingMode mode, params IPerformable[] tasks)
    {
        foreach (var task in tasks)
        {
            PerformTask(InstrumentedTask.Of(task));
        }
    }

    private void PerformTask<T>(T todo) where T : IPerformable
    {
        todo.PerformAs(this);
    }
}
