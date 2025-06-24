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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reqnroll;
using Reqnroll.BoDi;

namespace Agenix.ReqnrollPlugin;

/// Provides functionality to manage and retrieve binding instances of various types.
public class BindingInstanceProvider
{
    private readonly Dictionary<Type, object> _bindingInstances = new();

    // Count of cached bindings
    public int BindingCount => _bindingInstances.Count;

    // Directly access the ObjectContainer
    public IObjectContainer Container { get; private set; }

    // Set the object container reference
    public void SetObjectContainer(IObjectContainer objectContainer)
    {
        Container = objectContainer;
    }

    public void RegisterBinding<T>(T instance) where T : class
    {
        _bindingInstances[typeof(T)] = instance;
    }

    public T GetBinding<T>() where T : class
    {
        // First try our cache
        if (_bindingInstances.TryGetValue(typeof(T), out var instance))
        {
            return (T)instance;
        }

        // Then try to resolve from the container if available
        if (Container != null)
        {
            try
            {
                return Container.Resolve<T>();
            }
            catch
            {
                // Type is not registered in container
                return null;
            }
        }

        return null;
    }

    // Get all registered bindings from our dictionary
    public IReadOnlyDictionary<Type, object> GetAllCachedBindings()
    {
        return _bindingInstances;
    }

    // Find all types with the [Binding] attribute and resolve them from the container
    public IEnumerable<object> GetAllBindingInstances()
    {
        if (Container == null)
        {
            return [];
        }

        // Get all loaded assemblies
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // Find all types with the [Binding] attribute
        var bindingTypes = assemblies
            .SelectMany(a =>
            {
                try
                {
                    return a.GetTypes();
                }
                catch
                {
                    return Type.EmptyTypes;
                }
            })
            .Where(t => t.GetCustomAttribute<BindingAttribute>() != null);

        // Resolve each binding type from the container
        var result = new List<object>();
        foreach (var type in bindingTypes)
        {
            try
            {
                var instance = Container.Resolve(type);

                if (instance == null)
                {
                    continue;
                }

                result.Add(instance);
                // Optionally cache the instance
                _bindingInstances[type] = instance;
            }
            catch
            {
                // Skip if can't resolve
            }
        }

        return result;
    }

    // Get all binding types from all loaded assemblies
    public IEnumerable<Type> GetAllBindingTypes()
    {
        // Get all loaded assemblies
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // Find all types with the [Binding] attribute
        return assemblies
            .SelectMany(a =>
            {
                try
                {
                    return a.GetTypes();
                }
                catch
                {
                    return Type.EmptyTypes;
                }
            })
            .Where(t => t.GetCustomAttribute<BindingAttribute>() != null);
    }

    // Get bindings from our cache by predicate
    public IEnumerable<object> GetBindingsWhere(Func<Type, object, bool> predicate)
    {
        return _bindingInstances
            .Where(kvp => predicate(kvp.Key, kvp.Value))
            .Select(kvp => kvp.Value);
    }

    // Clear our binding cache
    public void ClearBindings()
    {
        _bindingInstances.Clear();
    }
}
