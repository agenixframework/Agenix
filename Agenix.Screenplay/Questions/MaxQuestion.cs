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

namespace Agenix.Screenplay.Questions;

/// <summary>
///     Represents a question that retrieves the maximum element from a collection of items answered by an actor.
/// </summary>
/// <typeparam name="T">
///     The type of the elements in the collection. It must implement <see cref="IComparable{T}" />.
/// </typeparam>
public class MaxQuestion<T>(IQuestion<ICollection<T>> listQuestion, IComparer<T>? comparer)
    : IQuestion<T>
    where T : IComparable<T>
{
    /// <summary>
    ///     Represents a question that retrieves the maximum element from a collection of items answered by an actor.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the elements in the collection. It must implement <see cref="IComparable{T}" />.
    /// </typeparam>
    public MaxQuestion(IQuestion<ICollection<T>> listQuestion)
        : this(listQuestion, Comparer<T>.Default)
    {
    }

    /// <summary>
    ///     Retrieves the maximum element from a collection, as answered by the specified actor.
    /// </summary>
    /// <param name="actor">
    ///     The actor who will answer the question and provide the collection to evaluate.
    /// </param>
    /// <returns>
    ///     The maximum element from the collection, using the specified comparer or the default comparer if none is provided.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the collection provided by the actor is null or contains no elements.
    /// </exception>
    public async Task<T> AnsweredBy(Actor actor)
    {
        var collection = await listQuestion.AnsweredBy(actor);

        if (collection == null)
        {
            throw new InvalidOperationException("The list question returned null.");
        }

        using var it = collection.GetEnumerator();
        if (!it.MoveNext())
        {
            throw new InvalidOperationException("Sequence contains no elements.");
        }

        var max = it.Current;
        var cmp = comparer ?? Comparer<T>.Default;

        while (it.MoveNext())
        {
            if (cmp.Compare(it.Current, max) > 0)
            {
                max = it.Current;
            }
        }

        return max;
    }
}
