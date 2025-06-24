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
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Agenix.Api.Validation.Context;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Validation;

/// <summary>
///     Represents a validator for headers in a given context. Implementations of this interface
///     provide functionality for validating headers based on specific rules or logic.
/// </summary>
public interface IHeaderValidator
{
    /// <summary>
    ///     Logger instance used for capturing and managing logging information
    ///     within the IHeaderValidator interface and its implementations.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(IHeaderValidator));

    static readonly ResourcePathTypeResolver TypeResolver = new(ResourcePath);

    /// <summary>
    ///     Dictionary to store header validators.
    /// </summary>
    private static readonly IDictionary<string, IHeaderValidator> Validators =
        new Dictionary<string, IHeaderValidator>();

    /// <summary>
    ///     Represents the path used to identify and locate the resource associated with the HeaderValidator implementation.
    /// </summary>
    static string ResourcePath => "Extension/agenix/header/validator";

    /// <summary>
    ///     Validates the provided header information against control values within the context of a test.
    /// </summary>
    /// <param name="name">The name of the header to validate.</param>
    /// <param name="received">The received value of the header for validation.</param>
    /// <param name="control">The control value to validate against.</param>
    /// <param name="context">The context of the current test.</param>
    /// <param name="validationContext">The context of header validation containing additional settings or data.</param>
    void ValidateHeader(string name, object received, object control, TestContext context,
        HeaderValidationContext validationContext);

    /// <summary>
    ///     Filters supported headers by name and value type.
    /// </summary>
    /// <param name="headerName">The name of the header.</param>
    /// <param name="type">The type of the header value.</param>
    /// <returns>A boolean indicating whether the header is supported.</returns>
    bool Supports(string headerName, Type type);

    /// <summary>
    ///     Resolves all available validators from the resource path lookup.
    ///     Scans assemblies for validator meta-information and instantiates those validators.
    /// </summary>
    /// <returns>A dictionary containing the registered header validators.</returns>
    static IDictionary<string, IHeaderValidator> Lookup()
    {
        if (Validators.Count != 0)
        {
            return Validators;
        }

        var resolvedValidators =
            TypeResolver.ResolveAll<dynamic>(ResourcePath, ITypeResolver.DEFAULT_TYPE_PROPERTY, "name");

        foreach (var kvp in resolvedValidators)
        {
            Validators[kvp.Key] = kvp.Value;
        }

        if (!Log.IsEnabled(LogLevel.Debug))
        {
            return Validators;
        }

        {
            foreach (var kvp in Validators)
            {
                Log.LogDebug("Found header validator '{KvpKey}' as {Type}", kvp.Key, kvp.Value.GetType());
            }
        }
        return Validators;
    }

    /// <summary>
    ///     Resolves validator from resource path lookup with given validator resource name.
    ///     Scans assemblies for validator meta-information with the given name and returns instance of validator.
    ///     Returns optional instead of throwing an exception when no validator could be found.
    /// </summary>
    /// <param name="validator"></param>
    /// <returns></returns>
    public static Optional<IHeaderValidator> Lookup(string validator)
    {
        try
        {
            var instance = TypeResolver.Resolve<dynamic>(validator);
            return Optional<IHeaderValidator>.Of(instance);
        }
        catch (AgenixSystemException)
        {
            Log.LogWarning("Failed to resolve header validator from resource '{Validator}'", validator);
        }

        return Optional<IHeaderValidator>.Empty;
    }
}
