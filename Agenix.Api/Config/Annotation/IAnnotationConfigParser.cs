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
using Agenix.Api.Endpoint;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Config.Annotation;

/// <summary>
///     Interface for parsing annotation configurations.
/// </summary>
public interface IAnnotationConfigParser
{
    object Parse(Attribute annotation, IReferenceResolver referenceResolver);
}

/// Interface for parsing specific annotation configurations.
/// @typeparam TAttribute The type of the annotation attribute.
/// @typeparam TEndpoint The type of the endpoint created from the annotation.
/// /
public interface IAnnotationConfigParser<in TAttribute, out TEndpoint> : IAnnotationConfigParser
    where TAttribute : Attribute
    where TEndpoint : IEndpoint
{
    /// <summary>
    ///     Path to the resource used for endpoint parser lookup.
    /// </summary>
    private const string ResourcePath = "Extension/agenix/endpoint/parser";

    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger
        Log = LogManager.GetLogger(typeof(IAnnotationConfigParser<TAttribute, TEndpoint>).Name);

    /// <summary>
    ///     Lazy-initialized resolver for locating and handling types based on a specified resource path.
    /// </summary>
    private static readonly Lazy<ResourcePathTypeResolver> TypeResolver =
        new(() => new ResourcePathTypeResolver(ResourcePath));

    /// <summary>
    ///     Lazy-initialized cache of annotation config parsers for improved performance and thread safety.
    /// </summary>
    private static readonly Lazy<Dictionary<string, IAnnotationConfigParser>> ParsersCache =
        new(LoadAnnotationConfigParsers);

    /// <summary>
    ///     Lazy-initialized cache for individual parser lookups to avoid repeated resolution attempts.
    /// </summary>
    private static readonly Lazy<ConcurrentDictionary<string, Optional<IAnnotationConfigParser>>> IndividualLookupCache =
        new(() => new ConcurrentDictionary<string, Optional<IAnnotationConfigParser>>());

    /// <summary>
    ///     Parses the given annotation and resolves references to create an endpoint.
    /// </summary>
    /// <param name="annotation">The annotation attribute used for parsing.</param>
    /// <param name="referenceResolver">The reference resolver to resolve references during parsing.</param>
    /// <returns>The created endpoint based on the provided annotation and resolved references.</returns>
    TEndpoint Parse(TAttribute annotation, IReferenceResolver referenceResolver);

    /// <summary>
    ///     Loads all available annotation config parsers from the type resolver.
    /// </summary>
    /// <returns>A dictionary containing all loaded annotation config parsers.</returns>
    private static Dictionary<string, IAnnotationConfigParser> LoadAnnotationConfigParsers()
    {
        var parsers = new Dictionary<string, IAnnotationConfigParser>(
            TypeResolver.Value.ResolveAll<IAnnotationConfigParser>("", ITypeResolver.TYPE_PROPERTY_WILDCARD)
        );

        if (Log.IsEnabled(LogLevel.Debug))
        {
            foreach (var kvp in parsers)
            {
                Log.LogDebug("Found annotation config parser '{KvpKey}' as {Name}", kvp.Key, kvp.Value.GetType().Name);
            }
        }

        return parsers;
    }

    /// <summary>
    ///     Retrieves a dictionary of annotation config parsers, mapped by their name.
    /// </summary>
    /// <returns>
    ///     A dictionary where the key is the name of the annotation config parser
    ///     and the value is the corresponding IAnnotationConfigParser instance.
    /// </returns>
    public static Dictionary<string, IAnnotationConfigParser> Lookup()
    {
        return ParsersCache.Value;
    }

    /// <summary>
    ///     Retrieves an annotation config parser based on the given validator string.
    /// </summary>
    /// <param name="parser">The string key representing the desired annotation config parser.</param>
    /// <returns>
    ///     An Optional containing the corresponding IAnnotationConfigParser instance if found, otherwise an empty
    ///     Optional.
    /// </returns>
    public static Optional<IAnnotationConfigParser> Lookup(string parser)
    {
        return IndividualLookupCache.Value.GetOrAdd(parser, key =>
        {
            try
            {
                IAnnotationConfigParser instance;
                if (key.Contains('.'))
                {
                    var separatorIndex = key.LastIndexOf('.');
                    instance = TypeResolver.Value.Resolve<IAnnotationConfigParser>(key[..separatorIndex],
                        key[(separatorIndex + 1)..]);
                }
                else
                {
                    instance = TypeResolver.Value.Resolve<IAnnotationConfigParser>(key);
                }

                return Optional<IAnnotationConfigParser>.Of(instance);
            }
            catch (AgenixSystemException)
            {
                Log.LogWarning("Failed to resolve annotation config parser from resource '{Parser}'", key);
                return Optional<IAnnotationConfigParser>.Empty;
            }
        });
    }
}
