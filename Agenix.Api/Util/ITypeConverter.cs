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

using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Util;

/// <summary>
///     Represents a service that provides type conversion operations.
///     Allows objects to be converted to the desired type and supports lookup for converters.
/// </summary>
public interface ITypeConverter
{
    const string Default = "default";
    static readonly Dictionary<string, ITypeConverter> Converters = [];

    /// Logger
    /// /
    private static readonly ILogger Log = LogManager.GetLogger(typeof(ITypeConverter));

    /// <summary>
    ///     Resolves all available converters from the resource path lookup. Scans classpath for converter meta-information and
    ///     instantiates those converters.
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, ITypeConverter> Lookup()
    {
        if (Converters.Count == 0)
        {
            /*converters = new ResourcePathTypeResolver().resolveAll(RESOURCE_PATH).ToDictionary(entry => entry.Key,
                                           entry => entry.Value);*/
            if (Converters.Count == 0)
            {
                Converters.Add(Default, DefaultTypeConverter.Instance);
            }

            Converters.ToList()
                .ForEach(x => Log.LogDebug("Found type converter '{ObjKey}' as {Type}", x.Key, x.Value.GetType()));
        }

        return Converters;
    }

    /// <summary>
    ///     Converts a target object to the required type if necessary.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    T ConvertIfNecessary<T>(object target, Type type);


    /// <summary>
    ///     Converts String value object to given type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <returns></returns>
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
    /// <returns>the type converter to use by default.</returns>
    public static ITypeConverter LookupDefault(ITypeConverter defaultTypeConverter)
    {
        var name = AgenixSettings.GetTypeConverter();

        var converters = Lookup();
        if (converters.Count == 1)
        {
            var converterEntry = converters.First();
            return converterEntry.Value;
        }

        if (converters.TryGetValue(name, out var value))
        {
            return value;
        }

        if (!AgenixSettings.TypeConverterDefault.Equals(name))
        {
            Console.WriteLine($"Missing type converter for name '{name}' - using default type converter");
        }

        return defaultTypeConverter;
    }
}
