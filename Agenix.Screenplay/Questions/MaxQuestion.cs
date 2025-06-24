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
    public MaxQuestion(IQuestion<ICollection<T>> listQuestion)
        : this(listQuestion, Comparer<T>.Default)
    {
    }

    public T AnsweredBy(Actor actor)
    {
        var collection = listQuestion.AnsweredBy(actor);
        return comparer != null
            ? collection.Max(comparer)
            : collection.Max();
    }
}
