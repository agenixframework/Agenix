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

using System.Collections.Concurrent;
using Agenix.Api.Log;
using Agenix.Core;
using Agenix.Screenplay.Abilities;
using Agenix.Screenplay.Events;
using Agenix.Screenplay.Fact;
using MediatR;
using MediatR.NotificationPublishers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agenix.Screenplay;

/// <summary>
///     Represents an actor in a screenplay-based testing framework or domain-driven design context.
///     An actor is an entity that can perform tasks, interact with abilities, and remember information.
/// </summary>
public class Actor : IPerformsTasks
{
    /// <summary>
    ///     Specifies an error handling mode where exceptions are ignored,
    ///     allowing the execution of operations or tasks to continue despite failures.
    /// </summary>
    public enum ErrorHandlingMode
    {
        /// <summary>
        ///     Specifies an error handling mode where an exception is thrown if a failure occurs,
        ///     stopping the execution of the current sequence of operations. This mode is useful
        ///     when errors need to be explicitly addressed, ensuring that the operation does not
        ///     continue after encountering a failure.
        /// </summary>
        THROW_EXCEPTION_ON_FAILURE,

        /// <summary>
        ///     Specifies an error handling mode where exceptions are ignored,
        ///     allowing the execution of operations or tasks to continue despite
        ///     failures. This mode is suitable for scenarios where uninterrupted
        ///     progression is preferred over stopping due to errors.
        /// </summary>
        IGNORE_EXCEPTIONS
    }

    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(Actor));

    private static IMediator? _mediator;
    private readonly ConcurrentDictionary<Type, IAbility> _abilities = new();
    private readonly ConcurrentBag<FactLifecycleListener> _factListeners = [];
    private readonly ConcurrentDictionary<string, object> _notepad = new();
    private string? _preferredPronoun;

    /// <summary>
    ///     Represents an actor in the screenplay pattern who interacts with the system under test.
    ///     Actors can perform tasks, ask questions, and use abilities within a test scenario.
    /// </summary>
    public Actor(string name)
    {
        Name = name;
        _mediator = InitializeMediator();
    }

    /// <summary>
    ///     Provides access to the event bus used for mediating messages
    ///     and communication between various components or handlers.
    /// </summary>
    public IMediator EventBus =>
        _mediator ??
        throw new InvalidOperationException(
            "Mediator is not initialized"); // NOSONAR: instance proxy kept for compatibility


    /// <summary>
    ///     Gets the name of the actor, which uniquely identifies them
    ///     within the screenplay or stage context.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    ///     Gets a description of the actor, which may provide additional context
    ///     or details about their role, characteristics, or attributes.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    ///     Gets the name of the actor or their preferred pronoun, if specified.
    ///     If a preferred pronoun is set, it will return the pronoun; otherwise, it defaults to the actor's name.
    /// </summary>
    public string NameOrPronoun => _preferredPronoun ?? Name;

    /// <summary>
    ///     Allows the actor to ask a specified question and retrieve the answer provided by it.
    /// </summary>
    /// <typeparam name="TAnswer">The type of the answer expected from the question.</typeparam>
    /// <param name="question">The question to be answered by the actor.</param>
    /// <returns>The answer provided by answering the specified question.</returns>
    public async Task<TAnswer> AsksFor<TAnswer>(IQuestion<TAnswer> question)
    {
        await BeginPerformance();
        var answer = await question.AnsweredBy(this);
        await EndPerformance();

        return answer;
    }

    /// <summary>
    ///     Allows the actor to perform a series of tasks sequentially.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the tasks to be performed, which must implement the <see cref="IPerformable" />
    ///     interface.
    /// </typeparam>
    /// <param name="todos">The tasks to be performed by the actor.</param>
    public Task AttemptsTo<T>(params T[] todos) where T : IPerformable
    {
        return AttemptsToAsync(ErrorHandlingMode.THROW_EXCEPTION_ON_FAILURE, todos.Cast<IPerformable>().ToArray());
    }

    private static IMediator InitializeMediator()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddLogging();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Actor).Assembly);
            cfg.NotificationPublisher = new TaskWhenAllPublisher();
        });

        IServiceProvider provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IMediator>();
    }

    /// <summary>
    ///     Add all the remembered items for the current actor to the other actor's memory
    /// </summary>
    /// <param name="otherActor">The actor to brief</param>
    public void Brief(Actor otherActor)
    {
        foreach (var item in _notepad)
        {
            otherActor._notepad.TryAdd(item.Key, item.Value);
            UseTheAgenixTestContext.As(otherActor).TestContext.SetVariable(item.Key, item.Value);
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
    ///     Checks if the actor has the specified ability.
    /// </summary>
    /// <typeparam name="T">The type of ability to check for.</typeparam>
    /// <returns>True if the actor has the ability; otherwise, false.</returns>
    public bool HasAbility<T>() where T : class, IAbility
    {
        return AbilityTo<T>() != null;
    }

    /// <summary>
    ///     Tries to get the specified ability from the actor.
    /// </summary>
    /// <typeparam name="T">The type of ability to get.</typeparam>
    /// <param name="ability">The ability if found; otherwise, null.</param>
    /// <returns>True if the ability was found; otherwise, false.</returns>
    public bool TryGetAbility<T>(out T ability) where T : class, IAbility
    {
        ability = AbilityTo<T>();
        return ability != null;
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
        // For now it's empty
    }

    private async Task BeginPerformance()
    {
        await EventBus.Publish(new ActorBeginsPerformanceEvent(Name));
    }

    private async Task EndPerformance()
    {
        await EventBus.Publish(new ActorEndsPerformanceEvent(Name));
    }

    private async Task NotifyPerformanceOf<T>(T todo) where T : IPerformable
    {
        await EventBus.Publish(new ActorPerforms(todo, Name));
    }

    private async Task StartConsequenceCheck()
    {
        await BeginPerformance();
        await EventBus.Publish(new ActorBeginsConsequenceCheckEvent(Name));
    }

    private async Task EndConsequenceCheck()
    {
        await EventBus.Publish(new ActorEndsConsequenceCheckEvent(Name));
        await EndPerformance();
    }

    /// <summary>
    ///     Indicates that the actor has certain tasks to perform.
    /// </summary>
    /// <param name="todos">The tasks to perform</param>
    public async Task Has(params IPerformable[] todos)
    {
        await AttemptsTo(todos);
    }

    /// <summary>
    ///     Allows an actor to be associated with a series of facts that define or prepare the actor's state or context within
    ///     a scenario. This method sets up the provided facts for the actor and registers lifecycle listeners to track
    ///     their execution and cleanup.
    /// </summary>
    /// <param name="facts">
    ///     A collection of facts to be associated with the actor. Each fact will be set up for the actor,
    ///     and a corresponding lifecycle listener will be created to monitor the fact's execution phases.
    /// </param>
    /// <remarks>
    ///     For each fact provided:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>The fact is set up for this actor by calling its Setup method</description>
    ///         </item>
    ///         <item>
    ///             <description>A FactLifecycleListener is created to monitor the fact's lifecycle</description>
    ///         </item>
    ///         <item>
    ///             <description>The listener is added to the actor's internal collection for cleanup purposes</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 If an AgenixInstanceManager is available, the listener is registered globally for test
    ///                 execution tracking
    ///             </description>
    ///         </item>
    ///     </list>
    ///     This approach ensures proper cleanup and monitoring of facts throughout their lifecycle within the test execution
    ///     context.
    /// </remarks>
    public async Task Has(params IFact[] facts)
    {
        foreach (var fact in facts)
        {
            await fact.Setup(this);
            var listener = new FactLifecycleListener(this, fact);
            _factListeners.Add(listener);

            if (AgenixInstanceManager.HasInstance())
            {
                AgenixInstanceManager.GetOrDefault().AddTestListener(listener);
            }
        }
    }


    /// <summary>
    ///     Returns a list of all abilities that implement IHasTeardown.
    /// </summary>
    /// <returns>A list of abilities that can be tear down</returns>
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
    public async Task WasAbleTo(params IPerformable[] todos)
    {
        await AttemptsTo(todos);
    }

    /// <summary>
    ///     Remembers the answer to a question by storing it in the actor's notepad.
    /// </summary>
    /// <typeparam name="TAnswer">The type of the answer to store</typeparam>
    /// <param name="key">The key under which to store the answer</param>
    /// <param name="question">The question to ask</param>
    public async Task Remember<TAnswer>(string key, IQuestion<TAnswer> question)
    {
        await BeginPerformance();
        var answer = await AsksFor(question);

        // Single ability lookup with a try-get pattern
        if (TryGetAbility<UseTheAgenixTestContext>(out var testContextAbility))
        {
            var resolvedKey = testContextAbility.TestContext.ReplaceDynamicContentInString(key);

            if (answer is string)
            {
                var resolved = testContextAbility.TestContext.ReplaceDynamicContentInString(answer.ToString());
                testContextAbility.TestContext.SetVariable(resolvedKey, resolved);
            }
            else
            {
                testContextAbility.TestContext.SetVariable(resolvedKey, answer);
            }
        }
        else
        {
            _notepad[key] = answer;
        }

        await EndPerformance();
    }

    /// <summary>
    ///     Stores a value in the actor's notepad under the specified key.
    /// </summary>
    public async Task Remember(string key, object value)
    {
        if (TryGetAbility<UseTheAgenixTestContext>(out var testContextAbility))
        {
            var resolvedKey = testContextAbility.TestContext.ReplaceDynamicContentInString(key);
            if (value is string)
            {
                var resolved = testContextAbility.TestContext.ReplaceDynamicContentInString(value.ToString());
                testContextAbility.TestContext.SetVariable(resolvedKey, resolved);
            }
            else
            {
                testContextAbility.TestContext.SetVariable(resolvedKey, value);
            }
        }
        else
        {
            _notepad[key] = value;
        }

        await Task.CompletedTask;
    }


    /// <summary>
    ///     Retrieves a value from the actor's notepad by key.
    /// </summary>
    public Task<T?> Recall<T>(string key)
    {
        // Single ability lookup
        if (TryGetAbility<UseTheAgenixTestContext>(out var testContextAbility))
        {
            var resolvedKey = testContextAbility.TestContext.ReplaceDynamicContentInString(key);
            return Task.FromResult(testContextAbility.TestContext.GetVariables().ContainsKey(resolvedKey)
                ? testContextAbility.TestContext.GetVariable<T>(resolvedKey)
                : default);
        }

        return _notepad.TryGetValue(key, out var value) ? Task.FromResult((T)value) : Task.FromResult<T?>(default);
    }

    /// <summary>
    ///     Returns a copy of all stored key-value pairs from the notepad.
    /// </summary>
    public IDictionary<string, object> RecallAll()
    {
        return TryGetAbility<UseTheAgenixTestContext>(out var testContextAbility)
            ? testContextAbility.TestContext.GetVariables()
            : new Dictionary<string, object>(_notepad);
    }

    /// <summary>
    ///     Removes and returns a value from the actor's notepad by key.
    /// </summary>
    public Task Forget(string key)
    {
        if (TryGetAbility<UseTheAgenixTestContext>(out var testContextAbility))
        {
            testContextAbility.TestContext.GetVariables().Remove(key);
        }
        else
        {
            _notepad.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Alternative syntax for recalling what the actor saw.
    /// </summary>
    public async Task<T?> SawAsThe<T>(string key)
    {
        return await Recall<T>(key);
    }

    /// <summary>
    ///     Alternative syntax for recalling what the actor gave.
    /// </summary>
    public async Task<T?> GaveAsThe<T>(string key)
    {
        return await Recall<T>(key);
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
    public async Task WrapUp()
    {
        foreach (var teardown in GetTeardowns())
        {
            await teardown.TearDown();
        }

        _factListeners.Clear();

        if (!AgenixInstanceManager.HasInstance())
        {
            return;
        }

        var count = AgenixInstanceManager.GetOrDefault().AgenixContext.TestListeners
            .RemoveTestListenersOfType<FactLifecycleListener>();

        Log.LogDebug("Removed {Count} FactLifecycleListeners from AgenixInstanceManager", count);
    }

    /// <summary>
    ///     Assigns a new name to the actor.
    /// </summary>
    /// <param name="name">The new name to be assigned to the actor.</param>
    public void AssignName(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     Executes a series of asynchronous tasks as the actor, with specified error handling behavior.
    /// </summary>
    /// <param name="mode">The error handling mode to apply when executing tasks.</param>
    /// <param name="tasks">The list of asynchronous tasks to be performed by the actor.</param>
    /// <returns>A task that represents the asynchronous operation of executing the specified tasks.</returns>
    public async Task AttemptsToAsync(ErrorHandlingMode mode, params IPerformable[] tasks)
    {
        await BeginPerformance();
        foreach (var task in tasks)
        {
            await PerformTaskAsync(InstrumentedTask.Of(task));
        }

        await EndPerformance();
    }

    /// <summary>
    ///     Executes the specified tasks asynchronously.
    ///     This method allows an actor to perform a series of tasks while optionally applying error-handling mechanisms.
    /// </summary>
    /// <typeparam name="T">The type of the tasks to be executed, which must implement <see cref="IPerformable" />.</typeparam>
    /// <param name="todos">An array of tasks for the actor to attempt to execute.</param>
    /// <returns>A task representing the asynchronous operation of performing the specified tasks.</returns>
    public async Task AttemptsToAsync<T>(params T[] todos) where T : IPerformable
    {
        await AttemptsToAsync(ErrorHandlingMode.THROW_EXCEPTION_ON_FAILURE, todos.Cast<IPerformable>().ToArray());
    }

    /// <summary>
    ///     Executes a specific task asynchronously by notifying its performance and performing it as the actor.
    /// </summary>
    /// <typeparam name="T">The type of the task to be performed, which must implement <see cref="IPerformable" />.</typeparam>
    /// <returns>A task that represents the asynchronous execution of the performance.</returns>
    private async Task PerformTaskAsync<T>(T todo) where T : IPerformable
    {
        await NotifyPerformanceOf(todo);
        await todo.PerformAsAsync(this);
    }

    /// <summary>
    ///     Verifies that the specified consequences are met for the actor.
    ///     Evaluates all provided consequences and ensures they do not violate any expectations.
    /// </summary>
    /// <typeparam name="T">The type of context or subject being verified by the consequences.</typeparam>
    /// <param name="consequences">An array of consequences to be checked for compliance or validation.</param>
    public async Task Should<T>(params IConsequence<T>[] consequences)
    {
        var errorTally = new ErrorTally<T>();

        await StartConsequenceCheck();

        foreach (var consequence in consequences)
        {
            await Check(consequence, errorTally);
        }

        await EndConsequenceCheck();

        errorTally.ReportAnyErrors();
    }

    private async Task Check<T>(IConsequence<T> consequence, ErrorTally<T> errorTally)
    {
        try
        {
            await consequence.EvaluateFor(this);
        }
        catch (Exception e)
        {
            errorTally.RecordError(consequence, e);
        }
    }
}
