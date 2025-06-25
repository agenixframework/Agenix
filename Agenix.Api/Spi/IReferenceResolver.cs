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

namespace Agenix.Api.Spi;

/// Provides methods to resolve references by name or type. This interface extends IReferenceRegistry and enables resolution
/// of objects that are bound to a registry. It can resolve single or multiple references based on names and types provided.
public interface IReferenceResolver : IReferenceRegistry
{
    /// Resolves a reference by its name and type.
    /// <param name="names">The name of the reference to resolve.</param>
    /// <returns>The resolved reference object.</returns>
    List<T> Resolve<T>(params string[] names)
    {
        return names.Length > 0 ? Resolve<T>(names, typeof(T)) : [.. ResolveAll<T>().Values];
    }

    /// Resolves a reference by its name and type.
    /// <param name="names">The name of the reference to resolve.</param>
    /// <param name="type">The type of the reference to resolve.</param>
    /// <returns>The resolved reference object.</returns>
    List<T> Resolve<T>(string[] names, Type type)
    {
        List<T> resolved = [];
        resolved.AddRange(names.Select(Resolve<T>));
        return resolved;
    }

    /// Resolves a reference by its name.
    /// <param name="name">The name of the reference to resolve.</param>
    /// <returns>The resolved reference object.</returns>
    object Resolve(string name)
    {
        return Resolve<object>(name);
    }

    /// Resolves a reference by its name and type.
    /// <returns>The resolved reference object.</returns>
    T Resolve<T>();

    /// Resolves a reference by its name and type.
    /// <param name="name">The name of the reference to resolve.</param>
    /// <returns>The resolved reference object.</returns>
    T Resolve<T>(string name);

    /// Resolves all references of a specified type.
    /// <typeparam name="T">The type of the references to resolve.</typeparam>
    /// <returns>A dictionary containing the resolved objects, with their names as keys.</returns>
    ConcurrentDictionary<string, T> ResolveAll<T>();

    /// Determines if a reference with the specified name can be resolved.
    /// <param name="name">The name of the reference to check.</param>
    /// <returns>True if the reference can be resolved; otherwise, false.</returns>
    bool IsResolvable(string name);

    /// Determines if a reference with the specified type can be resolved.
    /// <param name="type">The type of the reference to check.</param>
    /// <returns>True if the reference can be resolved; otherwise, false.</returns>
    bool IsResolvable(Type type);

    /// Determines if a reference with the specified name and type can be resolved.
    /// <param name="name">The name of the reference to check.</param>
    /// <param name="type">The type of the reference to check.</param>
    /// <returns>True if the reference can be resolved; otherwise, false.</returns>
    bool IsResolvable(string name, Type type);
}
