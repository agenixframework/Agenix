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
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Api;

/// <summary>
///     Test action builder.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IAsyncTestActionBuilder<out T> where T : IAsyncTestAction
{
    /// <summary>
    ///     Endpoint builder resource lookup path
    /// </summary>
    const string ResourcePath = "Extension/agenix/action/builder";

    /// <summary>
    ///     Logger for TestActionBuilder operations
    /// </summary>
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(IAsyncTestActionBuilder<IAsyncTestAction>));

    /// <summary>
    ///     Lazy-initialized resolver for test action builders
    /// </summary>
    private static readonly Lazy<ResourcePathTypeResolver> TypeResolver =
        new(() => new ResourcePathTypeResolver(ResourcePath));

    /// <summary>
    ///     Lazy-initialized cache of test action builders for improved performance and thread safety
    /// </summary>
    private static readonly Lazy<IDictionary<string, IAsyncTestActionBuilder<IAsyncTestAction>>> BuildersCache =
        new(LoadTestActionBuilders);

    /// <summary>
    ///     Lazy-initialized cache for individual builder lookups to avoid repeated resolution attempts
    /// </summary>
    private static readonly Lazy<ConcurrentDictionary<string, Optional<IAsyncTestActionBuilder<IAsyncTestAction>>>>
        IndividualLookupCache =
            new(() => new ConcurrentDictionary<string, Optional<IAsyncTestActionBuilder<IAsyncTestAction>>>());

    /// <summary>
    ///     Builds a new test action instance.
    /// </summary>
    /// <returns>the built test action.</returns>
    T Build();

    /// <summary>
    ///     Loads all available test action builders from the type resolver.
    /// </summary>
    /// <returns>A dictionary containing all loaded test action builders.</returns>
    private static IDictionary<string, IAsyncTestActionBuilder<IAsyncTestAction>> LoadTestActionBuilders()
    {
        var builders = TypeResolver.Value.ResolveAll<IAsyncTestActionBuilder<IAsyncTestAction>>();

        if (Logger.IsEnabled(LogLevel.Debug))
        {
            foreach (var (key, builder) in builders)
            {
                Logger.LogDebug("Found test action builder '{Key}' as {BuilderType}", key, builder.GetType());
            }
        }

        return builders;
    }

    /// <summary>
    ///     Resolves all available test action builders from resource path lookup. Scans classpath for test action builder meta
    ///     information
    ///     and instantiates those builders.
    /// </summary>
    /// <returns>Dictionary of action builder name to builder instance</returns>
    static IDictionary<string, IAsyncTestActionBuilder<IAsyncTestAction>> Lookup()
    {
        return BuildersCache.Value;
    }

    /// <summary>
    ///     Searches for available test action builders from the defined resource path.
    /// </summary>
    /// <returns>An Optional containing the test action builder if found, otherwise an empty Optional.</returns>
    static Optional<IAsyncTestActionBuilder<IAsyncTestAction>> Lookup(string builder)
    {
        return IndividualLookupCache.Value.GetOrAdd(builder, key =>
        {
            try
            {
                return Optional<IAsyncTestActionBuilder<IAsyncTestAction>>.Of(
                    TypeResolver.Value.Resolve<IAsyncTestActionBuilder<IAsyncTestAction>>(key));
            }
            catch (AgenixSystemException ex)
            {
                Logger.LogWarning(
                    "Failed to resolve test action builder from resource '{ResourcePath}/{Builder}': {Error}",
                    ResourcePath, key, ex.Message);
                return Optional<IAsyncTestActionBuilder<IAsyncTestAction>>.Empty;
            }
        });
    }

    interface IDelegatingTestActionBuilder<out TU> : IAsyncTestActionBuilder<TU> where TU : IAsyncTestAction
    {
        /// <summary>
        ///     Gets the delegate test action builder.
        /// </summary>
        IAsyncTestActionBuilder<TU> Delegate { get; }
    }
}
