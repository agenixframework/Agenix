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
using Agenix.Api.Log;
using Agenix.Api.Spi;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Util;

/// <summary>
///     Represents a service that provides type conversion operations.
///     Allows objects to be converted to the desired type and supports lookup for converters.
/// </summary>
public interface ITypeConverter
{
    const string Default = "default";

    /// <summary>
    ///     Resource path for type converter lookup.
    /// </summary>
    private const string ResourcePath = "Extension/agenix/converter";

    /// <summary>
    ///     Logger for type converter operations.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(ITypeConverter));

    /// <summary>
    ///     Lazy-initialized type resolver for loading converter implementations.
    /// </summary>
    private static readonly Lazy<ResourcePathTypeResolver> TypeResolver =
        new(() => new ResourcePathTypeResolver(ResourcePath));

    /// <summary>
    ///     Lazy-initialized cache of type converters for improved performance and thread safety.
    /// </summary>
    private static readonly Lazy<ConcurrentDictionary<string, ITypeConverter>> ConvertersCache =
        new(LoadTypeConverters);

    /// <summary>
    ///     Loads all available type converters from the resource path and adds the default converter if none are found.
    /// </summary>
    /// <returns>A dictionary containing all loaded type converters.</returns>
    private static ConcurrentDictionary<string, ITypeConverter> LoadTypeConverters()
    {
        var converters = new ConcurrentDictionary<string, ITypeConverter>();

        try
        {
            // Try to load converters from a resource path
            var resolvedConverters = TypeResolver.Value.ResolveAll<ITypeConverter>();
            foreach (var kvp in resolvedConverters)
            {
                converters[kvp.Key] = kvp.Value;
            }
        }
        catch (Exception ex)
        {
            Log.LogWarning("Failed to load type converters from resource path: {Error}", ex.Message);
        }

        // Add a default converter if no converters were loaded
        if (converters.IsEmpty)
        {
            converters.TryAdd(Default, DefaultTypeConverter.Instance);
        }

        if (Log.IsEnabled(LogLevel.Debug))
        {
            foreach (var kvp in converters)
            {
                Log.LogDebug("Found type converter '{Key}' as {Type}", kvp.Key, kvp.Value.GetType());
            }
        }

        return converters;
    }

    /// <summary>
    ///     Resolves all available converters from the resource path lookup. Scans classpath for converter meta-information and
    ///     instantiates those converters.
    /// </summary>
    /// <returns>A dictionary containing all available type converters.</returns>
    public static ConcurrentDictionary<string, ITypeConverter> Lookup()
    {
        return ConvertersCache.Value;
    }

    /// <summary>
    ///     Converts a target object to the required type if necessary.
    /// </summary>
    /// <typeparam name="T">The target type to convert to.</typeparam>
    /// <param name="target">The object to convert.</param>
    /// <param name="type">The type to convert to.</param>
    /// <returns>The converted object of type T.</returns>
    T ConvertIfNecessary<T>(object target, Type type);

    /// <summary>
    ///     Converts String value object to given type.
    /// </summary>
    /// <typeparam name="T">The target type to convert to.</typeparam>
    /// <param name="value">The string value to convert.</param>
    /// <param name="type">The type to convert to.</param>
    /// <returns>The converted object of type T.</returns>
    T ConvertStringToType<T>(string value, Type type);

    /// <summary>
    ///     Lookup default type converter specified by the resource path lookup and/ or environment settings.
    ///     In case only a single type converter is loaded via resource path, lookup this converter is used regardless of any
    ///     environment settings.
    ///     If there are multiple converter implementations on the classpath, the environment settings must specify the
    ///     default.
    ///     If no converter implementation is given via resource path lookup the default implementation is returned.
    /// </summary>
    /// <returns>the type converter to use by default.</returns>
    public static ITypeConverter LookupDefault()
    {
        return LookupDefault(DefaultTypeConverter.Instance);
    }

    /// <summary>
    ///     Lookup default type converter specified by the resource path lookup and/ or environment settings.
    ///     In case only a single type converter is loaded via a resource path, lookup this converter is used regardless of any
    ///     environment settings.
    ///     If there are multiple converter implementations on the classpath, the environment settings must specify the
    ///     default.
    ///     If no converter implementation is given via resource path lookup the default implementation is returned.
    /// </summary>
    /// <param name="defaultTypeConverter">The fallback type converter to use if no suitable converter is found.</param>
    /// <returns>the type converter to use by default.</returns>
    public static ITypeConverter LookupDefault(ITypeConverter defaultTypeConverter)
    {
        var name = AgenixSettings.GetTypeConverter();
        var converters = Lookup();

        // If only one converter is available, use it regardless of settings
        if (converters.Count == 1)
        {
            return converters.Values.First();
        }

        // Try to find converter by name from settings
        if (converters.TryGetValue(name, out var converter))
        {
            return converter;
        }

        // Log warning if a specific converter was requested but not found
        if (!AgenixSettings.TypeConverterDefault.Equals(name))
        {
            Log.LogWarning("Missing type converter for name '{Name}' - using default type converter", name);
        }

        return defaultTypeConverter;
    }
}
