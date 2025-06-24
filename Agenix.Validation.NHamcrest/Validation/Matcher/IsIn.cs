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
///     A matcher that checks if a given item is present in a collection of elements.
/// </summary>
/// <typeparam name="T">The type of items that this matcher handles.</typeparam>
public class IsIn<T> : IMatcher<T>
{
    private readonly ICollection<T> _collection;

    // Constructor that accepts a collection
    public IsIn(ICollection<T> collection)
    {
        // Use the original collection if no nested collection structure is detected
        _collection = collection;
    }

    // Constructor that accepts an array and converts it into a collection
    public IsIn(params T[] elements)
    {
        // Flatten nested collections into a single list
        _collection = elements.ToList();
    }

    public void DescribeTo(IDescription description)
    {
        description.AppendText("one of ");
        description.AppendValueList("{", ", ", "}", _collection);
    }

    public bool Matches(T actual)
    {
        return _collection.Contains(actual);
    }

    public void DescribeMismatch(T item, IDescription mismatchDescription)
    {
        mismatchDescription.AppendText("was ").AppendValue(item);
    }
}
