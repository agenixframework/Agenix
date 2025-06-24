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
using Agenix.Api.Log;
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Api;

/// <summary>
///     Test action builder.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ITestActionBuilder<out T> where T : ITestAction
{
    /// <summary>
    ///     Builds a new test action instance.
    /// </summary>
    /// <returns>the built test action.</returns>
    T Build();


    public interface IDelegatingTestActionBuilder<out TU> : ITestActionBuilder<TU> where TU : ITestAction
    {
        /// <summary>
        ///     Gets the delegate test action builder.
        /// </summary>
        ITestActionBuilder<TU> Delegate { get; }
    }

    /// <summary>
    /// Logger for TestActionBuilder operations
    /// </summary>
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(ITestActionBuilder<ITestAction>));

    /// <summary>
    /// Endpoint builder resource lookup path
    /// </summary>
    public const string ResourcePath = "Extension/agenix/action/builder";

    /// <summary>
    /// Default Agenix test action builders from classpath resource properties
    /// </summary>
    private static readonly ResourcePathTypeResolver TypeResolver = new(ResourcePath);

    /// <summary>
    /// Resolves all available test action builders from resource path lookup. Scans classpath for test action builder meta information
    /// and instantiates those builders.
    /// </summary>
    /// <returns>Dictionary of action builder name to builder instance</returns>
    public static IDictionary<string, ITestActionBuilder<ITestAction>> Lookup()
    {
        var builders = TypeResolver.ResolveAll<ITestActionBuilder<ITestAction>>();

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
    /// Searches for available test action builders from the defined resource path.
    /// </summary>
    /// <returns>A dictionary containing the available test action builders, keyed by their names.</returns>
    public static Optional<ITestActionBuilder<ITestAction>> Lookup(string builder)
    {
        try
        {
            return Optional<ITestActionBuilder<ITestAction>>.Of(
                TypeResolver.Resolve<ITestActionBuilder<ITestAction>>(builder));
        }
        catch (AgenixSystemException ex)
        {
            Logger.LogWarning("Failed to resolve test action builder from resource '{ResourcePath}/{Builder}': {Error}",
                ResourcePath, builder, ex.Message);
        }

        return Optional<ITestActionBuilder<ITestAction>>.Empty;
    }
}
