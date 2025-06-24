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
using Agenix.Api.Message.Correlation;

namespace Agenix.Core.Message.Correlation;

/// <summary>
///     Default object store implementation works on simple in memory dictionary map.
/// </summary>
/// <typeparam name="T"></typeparam>
public class DefaultObjectStore<T> : IObjectStore<T>
{
    private readonly ConcurrentDictionary<string, T> _store = new();

    /// <summary>
    ///     Adds a new object with the correlation key.
    /// </summary>
    /// <param name="correlationKey">The correlation key.</param>
    /// <param name="obj">The object to add.</param>
    public void Add(string correlationKey, T obj)
    {
        _store[correlationKey] = obj;
    }

    /// <summary>
    ///     Removes the object with the correlation key.
    /// </summary>
    /// <param name="correlationKey">The correlation key.</param>
    /// <returns>The removed object.</returns>
    public T Remove(string correlationKey)
    {
        _store.TryRemove(correlationKey, out var removedObject);
        return removedObject;
    }
}
