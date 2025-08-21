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
using Agenix.Api.Exceptions;
using NUnit.Framework;

namespace Agenix.Screenplay.Parallel;

/// <summary>
///     Enables multiple actors to perform tasks in parallel, providing mechanisms for concurrent execution
///     of screenplay actions and proper error handling across multiple threads.
///     This class is thread-safe and can be used safely from multiple threads.
/// </summary>
public class InParallel
{
    private readonly Actor[] _cast;

    private InParallel(Actor[] actors)
    {
        _cast = actors.ToArray() ?? throw new ArgumentNullException(nameof(actors)); // Create defensive copy
    }

    /// <summary>
    ///     Creates an InParallel instance for the specified actors.
    /// </summary>
    /// <param name="actors">The actors that will perform tasks in parallel.</param>
    /// <returns>An InParallel instance configured with the provided actors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when actors is null.</exception>
    /// <exception cref="ArgumentException">Thrown when actors array is empty.</exception>
    public static InParallel TheActors(params Actor[] actors)
    {
        ArgumentNullException.ThrowIfNull(actors);
        if (actors.Length == 0)
        {
            throw new ArgumentException("At least one actor must be provided", nameof(actors));
        }

        return actors.Any(actor => actor == null)
            ? throw new ArgumentException("All actors must be non-null", nameof(actors))
            : new InParallel(actors);
    }

    /// <summary>
    ///     Creates an InParallel instance for a collection of actors.
    /// </summary>
    /// <param name="actors">The collection of actors that will perform tasks in parallel.</param>
    /// <returns>An InParallel instance configured with the provided actors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when actors is null.</exception>
    /// <exception cref="ArgumentException">Thrown when actors collection is empty or contains null values.</exception>
    public static InParallel TheActors(ICollection<Actor> actors)
    {
        ArgumentNullException.ThrowIfNull(actors);
        if (actors.Count == 0)
        {
            throw new ArgumentException("At least one actor must be provided", nameof(actors));
        }

        return actors.Any(actor => actor == null)
            ? throw new ArgumentException("All actors must be non-null", nameof(actors))
            : new InParallel(actors.ToArray());
    }

    /// <summary>
    ///     Performs the specified tasks in parallel.
    /// </summary>
    /// <param name="tasks">The list of tasks to perform in parallel.</param>
    /// <exception cref="ArgumentNullException">Thrown when tasks is null.</exception>
    public void Perform(List<Action> tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        Perform(tasks.ToArray());
    }

    /// <summary>
    ///     Performs the specified tasks in parallel.
    /// </summary>
    /// <param name="tasks">The tasks to perform in parallel.</param>
    /// <exception cref="ArgumentNullException">Thrown when tasks is null.</exception>
    public void Perform(params Action[] tasks)
    {
        Perform("{0}", tasks);
    }

    /// <summary>
    ///     Performs the specified tasks in parallel with a custom step name.
    ///     This method is thread-safe and handles exceptions properly across all parallel tasks.
    /// </summary>
    /// <param name="stepName">The name of the step being performed.</param>
    /// <param name="tasks">The tasks to perform in parallel.</param>
    /// <exception cref="ArgumentNullException">Thrown when stepName or tasks is null.</exception>
    /// <exception cref="ArgumentException">Thrown when tasks array is empty or contains null values.</exception>
    public void Perform(string stepName, params Action[] tasks)
    {
        ArgumentNullException.ThrowIfNull(stepName);
        ArgumentNullException.ThrowIfNull(tasks);
        if (tasks.Length == 0)
        {
            throw new ArgumentException("At least one task must be provided", nameof(tasks));
        }

        if (tasks.Any(task => task == null))
        {
            throw new ArgumentException("All tasks must be non-null", nameof(tasks));
        }

        var exceptions = new ConcurrentBag<Exception>();

        try
        {
            // Create tasks with proper exception handling
            var taskList = tasks.Select(task => Task.Run(() =>
            {
                try
                {
                    task();
                }
                catch (Exception ex)
                {
                    // Store the exception in thread-safe collection
                    exceptions.Add(ex);
                }
            })).ToArray();

            // Wait for all tasks to complete - this is thread-safe and guarantees completion
            Task.WaitAll(taskList);

            // Handle any exceptions that occurred
            HandleExceptions(exceptions);
        }
        catch (AggregateException ae)
        {
            // Handle AggregateException from Task.WaitAll
            HandleAggregateException(ae);
        }
    }

    /// <summary>
    ///     Have several actors perform the same tasks in parallel.
    ///     This method is thread-safe and ensures each actor performs the tasks independently.
    /// </summary>
    /// <param name="tasks">The tasks that each actor will attempt to perform.</param>
    /// <exception cref="ArgumentNullException">Thrown when tasks is null.</exception>
    /// <exception cref="ArgumentException">Thrown when tasks array is empty or contains null values.</exception>
    public void EachAttemptTo(params IPerformable[] tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);
        if (tasks.Length == 0)
        {
            throw new ArgumentException("At least one task must be provided", nameof(tasks));
        }

        if (tasks.Any(task => task == null))
        {
            throw new ArgumentException("All tasks must be non-null", nameof(tasks));
        }

        // Create thread-safe actions for each actor
        var runnableTasks = _cast
            .Select(actor => new Action(void () =>
            {
                // Each actor gets their own copy of the task array to avoid race conditions
                var actorTasks = tasks.ToArray();
                actor.AttemptsTo(actorTasks).GetAwaiter().GetResult();
            }))
            .ToArray();

        Perform(runnableTasks);
    }

    /// <summary>
    ///     Have several actors perform the same tasks in parallel.
    /// </summary>
    /// <param name="tasks">The collection of tasks that each actor will attempt to perform.</param>
    /// <exception cref="ArgumentNullException">Thrown when tasks is null.</exception>
    public void EachAttemptTo(ICollection<IPerformable> tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        EachAttemptTo(tasks.ToArray());
    }

    /// <summary>
    ///     Handles exceptions that occurred during parallel execution in a thread-safe manner.
    /// </summary>
    /// <param name="exceptions">The collection of exceptions that occurred.</param>
    private static void HandleExceptions(ConcurrentBag<Exception> exceptions)
    {
        if (exceptions.IsEmpty)
        {
            return;
        }

        var firstException = exceptions.FirstOrDefault();
        if (firstException == null)
        {
            return;
        }

        // Preserve the original exception type for assertion errors and system exceptions
        throw firstException switch
        {
            AssertionException or SystemException => firstException,
            _ => new AgenixSystemException("An error occurred in one of the parallel tasks", firstException)
        };
    }

    /// <summary>
    ///     Handles AggregateException from Task.WaitAll in a thread-safe manner.
    /// </summary>
    /// <param name="aggregateException">The aggregate exception to handle.</param>
    private static void HandleAggregateException(AggregateException aggregateException)
    {
        var innerException = aggregateException.InnerExceptions.FirstOrDefault();
        if (innerException == null)
        {
            throw aggregateException;
        }

        throw innerException switch
        {
            AssertionException or SystemException => innerException,
            _ => new AgenixSystemException("An error occurred in one of the parallel tasks", innerException)
        };
    }
}
