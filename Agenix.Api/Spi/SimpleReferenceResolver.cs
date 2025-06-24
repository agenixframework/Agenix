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

using Agenix.Api.Exceptions;

namespace Agenix.Api.Spi;

/// SimpleReferenceResolver provides methods to resolve and bind objects by their names or types from an object store.
public class SimpleReferenceResolver : IReferenceResolver
{
    private readonly Dictionary<string, object> _objectStore = new();

    /// Resolves an object of the specified type from the object store.
    /// <typeparam name="T">The type of the object to resolve.</typeparam>
    /// <returns>The resolved object of the specified type.</returns>
    /// <exception cref="AgenixSystemException">Thrown when no object of the specified type is found in the object store.</exception>
    public T Resolve<T>()
    {
        try
        {
            return (T)_objectStore.Values.First(v => v is T);
        }
        catch (InvalidOperationException)
        {
            throw new AgenixSystemException($"Unable to find bean reference for type '{typeof(T)}'");
        }
    }

    /// Resolves an object by its name from the object store.
    /// <param name="name">The name of the object to resolve.</param>
    /// <returns>The resolved object associated with the specified name.</returns>
    public object Resolve(string name)
    {
        if (!_objectStore.TryGetValue(name, out var value))
        {
            throw new AgenixSystemException($"Unable to find bean reference for name '{name}'");
        }

        return value;
    }

    /// Resolves an object of a specified type from the object store.
    /// <typeparam name="T">The type of the object to resolve.</typeparam>
    /// <returns>The resolved object of type <typeparamref name="T" />.</returns>
    public T Resolve<T>(string name)
    {
        try
        {
            return (T)_objectStore.First(kvp => kvp.Key == name && kvp.Value is T).Value;
        }
        catch (InvalidOperationException)
        {
            throw new AgenixSystemException($"Unable to find bean reference for name '{name}'");
        }
    }

    /// Resolves all objects of a specified type in the object store.
    /// <typeparam name="T">The type of objects to resolve.</typeparam>
    /// <return>A dictionary containing the resolved objects, with their names as keys.</return>
    public Dictionary<string, T> ResolveAll<T>()
    {
        return _objectStore
            .Where(kvp => kvp.Value is not null)
            .Where(kvp => kvp.Value is T)
            .ToDictionary(kvp => kvp.Key, kvp => (T)kvp.Value);
    }

    /// Checks if an object with the given name can be resolved.
    /// <param name="name">The name of the object to check for resolution.</param>
    /// <return>True if an object with the given name can be resolved; otherwise, false.</return>
    public bool IsResolvable(string name)
    {
        return _objectStore.ContainsKey(name);
    }

    /// Checks if an object of the given type can be resolved.
    /// <param name="type">The type of the object to check for resolution.</param>
    /// <return>True if an object of the given type can be resolved; otherwise, false.</return>
    public bool IsResolvable(Type type)
    {
        return _objectStore.Values.Any(v => v.GetType() == type);
    }

    /// Checks if an object with the given name and type can be resolved.
    /// <param name="name">The name of the object to check for resolution.</param>
    /// <param name="type">The type of the object to check for resolution.</param>
    /// <return>True if the object can be resolved by both name and type; otherwise, false.</return>
    public bool IsResolvable(string name, Type type)
    {
        return _objectStore.TryGetValue(name, out var value) && value.GetType() == type;
    }

    /// Binds an object to a given name in the internal object store.
    /// <param name="name">The name to bind the object to.</param>
    /// <param name="value">The object to be bound.</param>
    public void Bind(string name, object value)
    {
        _objectStore[name] = value;
    }
}
