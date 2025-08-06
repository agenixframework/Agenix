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

using System.Collections;

namespace Agenix.Screenplay;

/// <summary>
///     Represents a question that retrieves the count of elements in a collection.
/// </summary>
public class CountQuestion<T>(IQuestion<ICollection<T>> listQuestion) : IQuestion<int>
{
    /// <summary>
    ///     Retrieves the count of elements in the collection resolved by the specified question.
    /// </summary>
    /// <param name="actor">The actor interacting with the question.</param>
    /// <returns>The count of elements in the collection resolved by the question. Returns 0 if the collection is null.</returns>
    public async Task<int> AnsweredBy(Actor actor)
    {
        var result = await listQuestion.AnsweredBy(actor).ConfigureAwait(false);

        return result switch
        {
            null => 0,
            // Fast paths for common collection shapes
            ICollection nonGeneric => nonGeneric.Count,
            ICollection<object> genericCollection => genericCollection.Count,
            // Fallbacks for enumerable results
            IEnumerable enumerable => enumerable.Cast<object>().Count()
        };
    }
}
