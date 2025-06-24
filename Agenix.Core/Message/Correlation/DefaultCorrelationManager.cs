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
using Agenix.Api.Context;
using Agenix.Api.Log;
using Agenix.Api.Message.Correlation;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Message.Correlation;

/// <summary>
///     Default correlation manager implementation works on simple in memory map for storing objects. Correlation key is
///     the map key. Clients can access objects in the store using the correlation key.
/// </summary>
/// <typeparam name="T"></typeparam>
public class DefaultCorrelationManager<T> : ICorrelationManager<T>
{
    private static readonly ILogger Log = LogManager.GetLogger("DefaultCorrelationManager");

    private IObjectStore<T> _objectStore = new DefaultObjectStore<T>();

    /// <summary>
    ///     Saves the specified correlation key into the given context under the provided correlation key name.
    /// </summary>
    /// <param name="correlationKeyName">The name used to save the correlation key.</param>
    /// <param name="correlationKey">The correlation key to be saved.</param>
    /// <param name="context">The context in which the correlation key will be saved.</param>
    public void SaveCorrelationKey(string correlationKeyName, string correlationKey, TestContext context)
    {
        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug($"Saving correlation key for '{correlationKeyName}'");
        }

        context.SetVariable(correlationKeyName, correlationKey);
    }

    /// <summary>
    ///     Retrieves the correlation key associated with the specified correlation key name from the given context.
    /// </summary>
    /// <param name="correlationKeyName">The name of the correlation key to retrieve.</param>
    /// <param name="context">The context containing the correlation key.</param>
    /// <returns>The correlation key associated with the specified name.</returns>
    /// <exception cref="Exception">Thrown if the correlation key could not be found in the context.</exception>
    public virtual string GetCorrelationKey(string correlationKeyName, TestContext context)
    {
        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug($"Get correlation key for '{correlationKeyName}'");
        }

        if (context.GetVariables().ContainsKey(correlationKeyName))
        {
            return context.GetVariable(correlationKeyName);
        }

        throw new Exception($"Failed to get correlation key for '{correlationKeyName}'");
    }

    /// <summary>
    ///     Stores the specified object associated with the given correlation key.
    /// </summary>
    /// <param name="correlationKey">The correlation key associated with the object.</param>
    /// <param name="obj">The object to be stored.</param>
    public void Store(string correlationKey, T obj)
    {
        if (obj == null)
        {
            Log.LogWarning($"Ignore correlated null object for '{correlationKey}'");
            return;
        }

        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug($"Saving correlated object for '{correlationKey}'");
        }

        _objectStore.Add(correlationKey, obj);
    }

    /// <summary>
    ///     Finds and removes the object associated with the specified correlation key.
    /// </summary>
    /// <param name="correlationKey">The correlation key associated with the object.</param>
    /// <param name="timeout">The maximum amount of time to wait for the object to be found, in milliseconds.</param>
    /// <returns>The object associated with the specified correlation key, or the default value if not found.</returns>
    public virtual T Find(string correlationKey, long timeout)
    {
        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug($"Finding correlated object for '{correlationKey}'");
        }

        return _objectStore.Remove(correlationKey);
    }

    /// <summary>
    ///     Sets the object store to be used by the correlation manager.
    /// </summary>
    /// <param name="store">The object store to be set.</param>
    public void SetObjectStore(IObjectStore<T> store)
    {
        _objectStore = store;
    }

    /// <summary>
    ///     Retrieves the current object store used by the correlation manager.
    /// </summary>
    /// <returns>The object store currently in use.</returns>
    public IObjectStore<T> GetObjectStore()
    {
        return _objectStore;
    }
}
