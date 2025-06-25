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

using Agenix.Api.Context;
using Agenix.Api.Log;
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Endpoint;

/// <summary>
///     Endpoint component registers with bean name in Spring application context and is then responsible to create proper
///     endpoints dynamically from endpoint uri values. Creates an endpoint instance by parsing the dynamic endpoint uri
///     with
///     special properties and parameters. Creates a proper endpoint configuration instance on the fly.
/// </summary>
public interface IEndpointComponent
{
    public static string EndpointName = "endpointName";

    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(IEndpointComponent));

    /// <summary>
    ///     Path used to locate resources associated with the endpoint component.
    /// </summary>
    private static readonly string ResourcePath = "Extension/agenix/endpoint/component";

    /// <summary>
    ///     Lazy-initialized resolver for dynamically loading and resolving resources based on a specified resource path.
    /// </summary>
    private static readonly Lazy<ResourcePathTypeResolver> TypeResolver =
        new(() => new ResourcePathTypeResolver(ResourcePath));

    /// <summary>
    ///     Lazy-initialized cache of endpoint components for improved performance and thread safety.
    /// </summary>
    private static readonly Lazy<IDictionary<string, IEndpointComponent>> ComponentsCache =
        new(LoadEndpointComponents);

    /// <summary>
    ///     Creates an endpoint instance by parsing the given dynamic endpoint URI with specific properties and parameters.
    /// </summary>
    /// <param name="endpointUri">The URI of the endpoint to be created.</param>
    /// <param name="context">The context in which the endpoint is to be created.</param>
    /// <returns>A new instance of an IEndpoint configured according to the specified URI and context.</returns>
    IEndpoint CreateEndpoint(string endpointUri, TestContext context);

    /// <summary>
    ///     Gets the name of this endpoint component.
    /// </summary>
    /// <returns>The name of the endpoint component.</returns>
    string GetName();

    /// <summary>
    ///     Constructs endpoint parameters from the endpoint URI.
    /// </summary>
    /// <param name="endpointUri">The endpoint URI.</param>
    /// <returns>A dictionary of parameters extracted from the endpoint URI.</returns>
    IDictionary<string, string> GetParameters(string endpointUri);

    /// <summary>
    ///     Loads all available endpoint components from the resource path.
    /// </summary>
    /// <returns>A dictionary containing all loaded endpoint components.</returns>
    private static IDictionary<string, IEndpointComponent> LoadEndpointComponents()
    {
        var components = TypeResolver.Value.ResolveAll<IEndpointComponent>();

        if (Log.IsEnabled(LogLevel.Debug))
        {
            foreach (var kvp in components)
            {
                Log.LogDebug("Found endpoint component '{KvpKey}' as {Name}", kvp.Key, kvp.Value.GetType().Name);
            }
        }

        return components;
    }

    /// <summary>
    ///     Provides a lookup of available endpoint components, indexed by their respective types.
    /// </summary>
    /// <returns>
    ///     A dictionary where the key is a string representing the endpoint type and the value is an instance of
    ///     IEndpointComponent.
    /// </returns>
    public static IDictionary<string, IEndpointComponent> Lookup()
    {
        return ComponentsCache.Value;
    }

    /// <summary>
    ///     Retrieves an <see cref="IEndpointComponent" /> instance based on the specified validator string.
    ///     If the validator is recognized, a corresponding <see cref="IEndpointComponent" /> instance is instantiated and
    ///     returned.
    ///     Otherwise, an empty <see cref="Optional{T}" /> is returned.
    /// </summary>
    /// <param name="validator">The validator string used to look up the appropriate <see cref="Optional{T}" /> instance.</param>
    /// <returns>
    ///     An <see cref="Optional{T}" /> containing the <see cref="IEndpointComponent" /> instance if found, otherwise an
    ///     empty <see cref="Optional{T}" />.
    /// </returns>
    public static Optional<IEndpointComponent> Lookup(string validator)
    {
        try
        {
            return Optional<IEndpointComponent>.Of(TypeResolver.Value.Resolve<IEndpointComponent>(validator));
        }
        catch (TypeLoadException)
        {
            Log.LogWarning("Failed to resolve annotation config parser from resource '{Validator}'", validator);
        }

        return Optional<IEndpointComponent>.Empty;
    }
}
