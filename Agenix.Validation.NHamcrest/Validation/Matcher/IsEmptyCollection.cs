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

using NHamcrest;

namespace Agenix.Validation.NHamcrest.Validation.Matcher;

/// <summary>
///     Represents a matcher that evaluates whether a given collection of type <typeparamref name="T" /> is empty.
/// </summary>
/// <typeparam name="T">
///     The type of elements contained within the collection being evaluated.
/// </typeparam>
/// <remarks>
///     This matcher is used to ensure that a collection does not contain any elements.
/// </remarks>
public class IsEmptyCollection<T> : IMatcher<IEnumerable<T>>
{
    public void DescribeTo(IDescription description)
    {
        description.AppendText("an empty collection");
    }

    public bool Matches(IEnumerable<T> collection)
    {
        return collection is not null && !collection.Any();
    }

    public void DescribeMismatch(IEnumerable<T> collection, IDescription mismatchDescription)
    {
        if (collection == null)
        {
            mismatchDescription.AppendText("was null");
        }
        else
        {
            mismatchDescription.AppendText($"was a collection with {collection.Count()} items: ");
            mismatchDescription.AppendValue(collection);
        }
    }
}
