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

using System.Collections;
using System.Collections.Generic;
using System.IO;
using Agenix.Api.Context;
using Agenix.Api.IO;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Util;
using Agenix.Api.Variable.Dictionary;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Variable.Dictionary;

/// <summary>
///     Abstract data dictionary implementation provides global scope handling.
/// </summary>
public abstract class AbstractDataDictionary<T> : AbstractMessageProcessor, IDataDictionary<T>
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(AbstractDataDictionary<T>));

    /// <summary>
    ///     Scope defines where a dictionary should be applied (explicit or global)
    /// </summary>
    private bool _globalScope = true;

    /// <summary>
    ///     Known mappings to this dictionary
    /// </summary>
    protected Dictionary<string, string> _mappings = new();

    /// <summary>
    ///     Data dictionary name
    /// </summary>
    private string _name;

    /// <summary>
    ///     Kind of mapping strategy how to identify dictionary item
    /// </summary>
    private PathMappingStrategy _pathMappingStrategy = PathMappingStrategy.EXACT;

    /// <summary>
    ///     Mapping file resource
    /// </summary>
    protected IResource MappingFile;

    public IResource MappingFileResource
    {
        get => MappingFile;
        set => MappingFile = value;
    }

    public Dictionary<string, string> Mappings
    {
        get => _mappings;
        set => _mappings = value;
    }

    /// <summary>
    ///     Gets or sets the global scope property.
    /// </summary>
    public bool IsGlobalScope
    {
        get => _globalScope;
        set => _globalScope = value;
    }

    /// <summary>
    ///     Gets the data dictionary name.
    /// </summary>
    public string Name
    {
        get => _name ?? GetType().Name;
        set => _name = value;
    }

    /// <summary>
    ///     Gets the path mapping strategy.
    /// </summary>
    public PathMappingStrategy PathMappingStrategy
    {
        get => _pathMappingStrategy;
        set => _pathMappingStrategy = value;
    }

    /// <summary>
    ///     Initialize the data dictionary by loading mappings from file if specified.
    /// </summary>
    public virtual void Initialize()
    {
        if (MappingFile != null)
        {
            Log.LogDebug("Reading data dictionary mapping: {Location}", MappingFile.Description);

            var properties = LoadProperties(MappingFile);
            foreach (DictionaryEntry entry in properties)
            {
                var key = entry.Key as string;
                var s = entry.Value as string;


                Log.LogDebug("Loading data dictionary mapping: {Key}={Value}", key, s);

                if (Log.IsEnabled(LogLevel.Debug) && Mappings.TryGetValue(key, out var value))
                {
                    Log.LogWarning(
                        "Overwriting data dictionary mapping '{Key}'; old value: {OldValue} new value: {NewValue}",
                        key, value, s);
                }

                Mappings[key] = s;
            }

            Log.LogInformation("Loaded data dictionary mapping: {Location}", MappingFile.Description);
        }
    }

    /// <summary>
    ///     Abstract method to translate values - must be implemented by derived classes.
    /// </summary>
    /// <typeparam name="R">The return type</typeparam>
    /// <param name="key">The key element in message content</param>
    /// <param name="value">Current value</param>
    /// <param name="context">The current test context</param>
    /// <returns>Translated value</returns>
    public abstract R Translate<R>(T key, R value, TestContext context);

    bool IScoped.IsGlobalScope()
    {
        return IsGlobalScope;
    }

    /// <summary>
    ///     Convert to original value type if necessary.
    /// </summary>
    /// <typeparam name="V">The value type</typeparam>
    /// <param name="value">The string value to convert</param>
    /// <param name="originalValue">The original value for type reference</param>
    /// <param name="context">The test context</param>
    /// <returns>Converted value</returns>
    protected V ConvertIfNecessary<V>(string value, V originalValue, TestContext context)
    {
        if (originalValue == null)
        {
            return (V)(object)context.ReplaceDynamicContentInString(value);
        }

        return context.TypeConverter.ConvertIfNecessary<V>(
            context.ReplaceDynamicContentInString(value),
            typeof(V));
    }

    /// <summary>
    ///     Load properties from a resource file.
    /// </summary>
    /// <param name="resource">The resource to load from</param>
    /// <returns>Dictionary of key-value pairs</returns>
    protected Properties LoadProperties(IResource resource)
    {
        var properties = new Properties();
        if (!resource.Exists)
        {
            throw new FileNotFoundException(resource.Description);
        }

        properties.Load(resource.InputStream);
        return properties;
    }
}
