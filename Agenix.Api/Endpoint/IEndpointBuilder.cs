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

using Agenix.Api.Annotations;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Endpoint;

/// <summary>
///     Defines the contract for an endpoint builder that includes methods to build and manage endpoints.
/// </summary>
/// <typeparam name="T">
///     The type of endpoint being constructed, constrained to implement the <see cref="IEndpoint" /> interface.
/// </typeparam>
public interface IEndpointBuilder<out T> where T : IEndpoint
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(IEndpointBuilder<T>).Name);

    /// <summary>
    ///     Resolver for associating resource paths with types. Used to interpret and map
    ///     resource paths to associated types or properties within the endpoint builder context.
    /// </summary>
    private static readonly ResourcePathTypeResolver TypeResolver = new(ResourcePath);

    /// <summary>
    ///     Represents the resource path where endpoint builder configurations
    ///     and metadata are located.
    /// </summary>
    static string ResourcePath => "Extension/agenix/endpoint/builder";

    /// <summary>
    ///     Builds the endpoint.
    /// </summary>
    /// <returns></returns>
    T Build();

    /// <summary>
    ///     Determines whether the specified endpoint type is supported by the builder.
    /// </summary>
    /// <param name="endpointType">
    ///     The type of the endpoint to check for support.
    /// </param>
    /// <returns>
    ///     True if the specified endpoint type is supported; otherwise, false.
    /// </returns>
    bool Supports(Type endpointType);

    /// <summary>
    ///     Retrieves a dictionary of endpoint builders, with endpoint names as keys and endpoint builders as values.
    /// </summary>
    /// <returns>A dictionary containing mappings of endpoint names to their corresponding builders.</returns>
    public static Dictionary<string, IEndpointBuilder<T>> Lookup()
    {
        var validators = new Dictionary<string, IEndpointBuilder<T>>
        (
            TypeResolver.ResolveAll<IEndpointBuilder<T>>("", ITypeResolver.TYPE_PROPERTY_WILDCARD)
        );

        if (!Log.IsEnabled(LogLevel.Debug))
        {
            return validators;
        }

        foreach (var kvp in validators)
        {
            Log.LogDebug("Found endpoint builder '{KvpKey}' as {Name}", kvp.Key, kvp.Value.GetType().Name);
        }

        return validators;
    }

    /// <summary>
    ///     Performs a lookup for all available endpoint builders in the system.
    /// </summary>
    /// <returns>
    ///     A dictionary containing the endpoint builder names as keys and their respective
    ///     <see cref="IEndpointBuilder{T}" /> implementations as values.
    /// </returns>
    public static Optional<IEndpointBuilder<T>> Lookup(string builder)
    {
        try
        {
            IEndpointBuilder<T> instance;
            if (builder.Contains('.'))
            {
                var separatorIndex = builder.LastIndexOf('.');
                instance = TypeResolver.Resolve<IEndpointBuilder<T>>(builder[..separatorIndex],
                    builder[(separatorIndex + 1)..]);
            }
            else
            {
                instance = TypeResolver.Resolve<IEndpointBuilder<T>>(builder);
            }

            return Optional<IEndpointBuilder<T>>.Of(instance);
        }
        catch (AgenixSystemException)
        {
            Log.LogWarning("Failed to resolve endpoint builder from resource '{Builder}'", builder);
        }

        return Optional<IEndpointBuilder<T>>.Empty;
    }

    /// <summary>
    ///     Builds an endpoint instance with properties set from the given annotation and reference resolver.
    /// </summary>
    /// <param name="endpointAnnotation">
    ///     An instance of <see cref="AgenixEndpointAttribute" /> containing the endpoint
    ///     configuration.
    /// </param>
    /// <param name="referenceResolver">
    ///     An instance of <see cref="IReferenceResolver" /> used for resolving references in the
    ///     endpoint properties.
    /// </param>
    /// <returns>
    ///     An instance of <see cref="IEndpoint" /> with properties set according to the given annotation and reference
    ///     resolver.
    /// </returns>
    public T Build(AgenixEndpointAttribute endpointAnnotation, IReferenceResolver referenceResolver)
    {
        var nameSetter = ReflectionHelper.FindMethod(GetType(), "Name", typeof(string));
        if (nameSetter != null)
        {
            ReflectionHelper.InvokeMethod(nameSetter, this, endpointAnnotation.Name);
        }

        foreach (var endpointProperty in endpointAnnotation.Properties)
        {
            var propertyMethod = ReflectionHelper.FindMethod(GetType(), endpointProperty.Name, endpointProperty.Type);
            if (propertyMethod != null)
            {
                if (endpointProperty.Type != typeof(string) && referenceResolver.IsResolvable(endpointProperty.Value))
                {
                    var resolvedValue = referenceResolver.Resolve<T>(endpointProperty.Value);
                    ReflectionHelper.InvokeMethod(propertyMethod, this, resolvedValue);
                }
                else
                {
                    var convertedValue =
                        TypeConversionUtils.ConvertStringToType<dynamic>(endpointProperty.Value, endpointProperty.Type);
                    ReflectionHelper.InvokeMethod(propertyMethod, this, convertedValue);
                }
            }
        }

        return Build();
    }
}
