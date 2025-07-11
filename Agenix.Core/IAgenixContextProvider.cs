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

using System.Collections.Generic;
using System.Linq;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Core;

/// <summary>
///     AgenixContextProvider is a delegate that defines a method signature for
///     creating instances of AgenixContext. It is used as a type for methods that
///     provide or configure an AgenixContext within the application, facilitating
///     context creation and management processes.
/// </summary>
public delegate AgenixContext AgenixContextProvider();

/// <summary>
///     Provides an interface for creating and managing Agenix contexts. This interface defines methods
///     for creating contexts and locating an Agenix context provider. It is primarily used to manage the
///     lifecycle and configuration of Agenix-related services.
/// </summary>
public interface IAgenixContextProvider
{
    /// <summary>
    ///     Log is a private static readonly instance of the log4net.ILogger interface used for logging
    ///     messages within the IAgenixContextProvider. It handles logging such as debug information
    ///     and warnings related to the context creation and provider lookup process.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(IAgenixContextProvider));

    /// <summary>
    ///     Create Agenix context with this provider.
    /// </summary>
    /// <returns></returns>
    AgenixContext Create();

    /// <summary>
    ///     Locate and provide an appropriate Agenix context provider. It first checks for available providers,
    ///     and if no specific provider is found, it defaults to the standard implementation.
    /// </summary>
    /// <returns>An IAgenixContextProvider that will be used to create Agenix contexts.</returns>
    public static IAgenixContextProvider Lookup()
    {
        Dictionary<string, IAgenixContextProvider> provider = [];

        switch (provider.Count)
        {
            case 0:
                Log.LogDebug("Using default Agenix context provider");
                return new DefaultAgenixContextProvider();
            case > 1:
                Log.LogWarning(
                    $"Found {provider.Count} Agenix context provider implementations. Please choose one of them.");
                break;
        }

        if (Log.IsEnabled(LogLevel.Debug))
        {
            foreach (var entry in provider)
            {
                Log.LogDebug($"Found Agenix context provider '{entry.Key}' as {entry.Value.GetType()}");
            }
        }

        var contextProvider = provider.Values.First();
        Log.LogDebug($"Using Agenix context provider '{provider.Keys.First()}' as {contextProvider}");
        return contextProvider;
    }

    /// <summary>
    ///     Looks up an IAgenixContextProvider by the specified name.
    /// </summary>
    /// <param name="name">The name of the Agenix context provider to look up.</param>
    /// <returns>An Optional containing the IAgenixContextProvider if found; otherwise, an empty Optional.</returns>
    public static Optional<IAgenixContextProvider> Lookup(string name)
    {
        try
        {
            if (name.Equals("default"))
            {
                var instance = new DefaultAgenixContextProvider();
                return Optional<IAgenixContextProvider>.Of(instance);
            }
        }
        catch (AgenixSystemException)
        {
            Log.LogWarning($"Failed to resolve Agenix context provider from resource '{name}'");
        }

        return Optional<IAgenixContextProvider>.Empty;
    }
}
