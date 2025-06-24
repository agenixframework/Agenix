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

/// <summary>
///     Represents a composite of multiple tasks or actions that can be performed by an actor.
/// </summary>
/// <remarks>
///     A composite performable enables the combination of multiple actions or tasks into a single,
///     cohesive unit that can be executed sequentially by an actor.
/// </remarks>
public class CompositePerformable : IPerformable
{
    private readonly List<IPerformable> _todoList;

    private CompositePerformable(List<IPerformable> todoList)
    {
        _todoList = todoList;
    }

    /// <summary>
    ///     Performs all actions or tasks in the composite list using the provided actor.
    /// </summary>
    /// <typeparam name="T">The type of actor performing the actions.</typeparam>
    /// <param name="actor">The actor that will execute each performable in the composite list.</param>
    public void PerformAs<T>(T actor) where T : Actor
    {
        foreach (var todo in _todoList)
        {
            actor.AttemptsTo(todo);
        }
    }

    /// <summary>
    ///     Combines two performables into a composite performable.
    /// </summary>
    /// <param name="firstPerformable">The first performable to be included in the composite.</param>
    /// <param name="nextPerformable">The next performable to be included in the composite.</param>
    /// <returns>
    ///     A new composite performable containing the combined actions of the specified performables.
    /// </returns>
    public static IPerformable From(IPerformable firstPerformable, IPerformable nextPerformable)
    {
        var todoList = new List<IPerformable>();
        todoList.AddRange(Flattened(firstPerformable));
        todoList.AddRange(Flattened(nextPerformable));
        return new CompositePerformable(todoList);
    }

    /// <summary>
    ///     Flattens the given performable into a list of individual performables,
    ///     expanding composite performables into their constituent actions.
    /// </summary>
    /// <param name="performable">
    ///     The performable to be flattened. If it is a composite performable, its actions will be
    ///     extracted.
    /// </param>
    /// <returns>
    ///     A list of individual performables. If the input is not a composite performable, the list will contain the
    ///     input performable itself.
    /// </returns>
    private static List<IPerformable> Flattened(IPerformable performable)
    {
        if (performable is CompositePerformable composite)
        {
            return composite._todoList;
        }

        return [performable];
    }
}
