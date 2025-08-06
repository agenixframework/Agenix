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
///     Represents a question that retrieves the minimum value from a collection of items.
/// </summary>
/// <typeparam name="T">The type of items in the collection. This type must implement the IComparable interface.</typeparam>
public class MinQuestion<T>(IQuestion<ICollection<T>> listQuestion, IComparer<T>? comparer)
    : IQuestion<T>
    where T : IComparable<T>
{
    /// <summary>
    ///     Retrieves the minimum value from a collection of items based on the specified question and optional comparer.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection. This type must implement the IComparable interface.</typeparam>
    /// <param name="listQuestion">The question that provides the collection of items.</param>
    /// <param name="comparer">The comparer to use when determining the minimum value, or null to use the default comparer.</param>
    public MinQuestion(IQuestion<ICollection<T>> listQuestion)
        : this(listQuestion, Comparer<T>.Default)
    {
    }

    /// <summary>
    ///     Evaluates the given question on the specified actor to determine the minimum value
    ///     from the collection retrieved by the question.
    /// </summary>
    /// <param name="actor">The actor who will answer the question.</param>
    /// <returns>The minimum value from the collection returned by the question.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the collection returned by the question is null or when the collection
    ///     is empty and no minimum value can be determined.
    /// </exception>
    public async Task<T> AnsweredBy(Actor actor)
    {
        var items = await listQuestion.AnsweredBy(actor);
        if (items is null)
        {
            throw new InvalidOperationException("The list question returned null.");
        }

        using var it = items.GetEnumerator();
        if (!it.MoveNext())
        {
            throw new InvalidOperationException("Sequence contains no elements.");
        }

        var min = it.Current;
        var cmp = comparer ?? Comparer<T>.Default;

        while (it.MoveNext())
        {
            if (cmp.Compare(it.Current, min) < 0)
            {
                min = it.Current;
            }
        }

        return min;
    }
}
