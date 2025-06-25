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
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Validation;

/// <summary>
/// Provides a mechanism for matching values based on specified validation rules and conditions.
/// </summary>
/// <remarks>
/// Implementations of the IValueMatcher interface are responsible for evaluating if a received value
/// meets specific criteria defined by a control value and validating it within the context of a test.
/// It also provides functionality to identify its compatibility with a given control type.
/// </remarks>
public interface IValueMatcher
{
    /// <summary>
    ///     A logger instance used to log operations and diagnostics within the <c>IValueMatcher</c> interface.
    /// </summary>
    /// <remarks>
    ///     This static logger is configured via the logging framework and is used to record debug and
    ///     operational messages, aiding in identifying the state and behavior of value-matcher-related processes.
    /// </remarks>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(IValueMatcher));

    /// <summary>
    ///     Represents the path used to identify and locate the resource associated with the ValueMatcher implementation.
    /// </summary>
    static string ResourcePath => "Extension/agenix/value/matcher";

    /// <summary>
    ///     Lazy-initialized type resolver for resolving resource paths and retrieving type-related information during runtime.
    /// </summary>
    /// <remarks>
    ///     This resolver provides methods to dynamically locate and instantiate types or resources
    ///     based on a specified resource path and configuration. It is used for tasks like
    ///     loading type implementations or validating types in context-specific scenarios.
    /// </remarks>
    private static readonly Lazy<ResourcePathTypeResolver> TypeResolver =
        new(() => new ResourcePathTypeResolver(ResourcePath));

    /// <summary>
    ///     Lazy-initialized cache of value matchers for improved performance and thread safety.
    /// </summary>
    /// <remarks>
    ///     The collection maps a string identifier to an implementation of the <c>IValueMatcher</c> interface.
    ///     It is used to retrieve or register value matcher instances within the application. This dictionary
    ///     stores all the known implementations that support value validation functionality.
    /// </remarks>
    private static readonly Lazy<IDictionary<string, IValueMatcher>> ValidatorsCache =
        new(LoadValueMatchers);

    /// <summary>
    ///     Lazy-initialized cache for individual matcher lookups to avoid repeated resolution attempts.
    /// </summary>
    private static readonly Lazy<ConcurrentDictionary<string, Optional<IValueMatcher>>> IndividualLookupCache =
        new(() => new ConcurrentDictionary<string, Optional<IValueMatcher>>());

    /// <summary>
    ///     Loads all available value matchers from the type resolver.
    /// </summary>
    /// <returns>A dictionary containing all loaded value matchers.</returns>
    private static IDictionary<string, IValueMatcher> LoadValueMatchers()
    {
        var validators = new ConcurrentDictionary<string, IValueMatcher>();

        try
        {
            var resolvedValidators = TypeResolver.Value.ResolveAll<IValueMatcher>();

            foreach (var kvp in resolvedValidators)
            {
                validators[kvp.Key] = kvp.Value;
            }

            if (Log.IsEnabled(LogLevel.Debug))
            {
                foreach (var kvp in validators)
                {
                    Log.LogDebug("Found value matcher '{Key}' as {Type}", kvp.Key, kvp.Value.GetType());
                }
            }
        }
        catch (Exception ex)
        {
            Log.LogWarning("Failed to load value matchers from resource path: {Error}", ex.Message);
        }

        return validators;
    }

    /// <summary>
    ///     Value matcher verifies the match of given received and control values.
    /// </summary>
    /// <param name="received">The received value to validate.</param>
    /// <param name="control">The control value to validate against.</param>
    /// <param name="context">The test context providing validation environment.</param>
    /// <returns>True if the values match according to the matcher's logic; otherwise, false.</returns>
    bool Validate(object received, object control, TestContext context);

    /// <summary>
    ///     Determines whether the matcher supports the specified control type.
    /// </summary>
    /// <param name="controlType">The control type to be evaluated.</param>
    /// <returns>True if the control type is supported; otherwise, false.</returns>
    bool Supports(Type controlType);

    /// <summary>
    ///     Resolves all available validators from the resource path lookup.
    ///     Scans assemblies for validator meta-information and instantiates those validators.
    /// </summary>
    /// <returns>A dictionary containing the registered value matchers.</returns>
    static IDictionary<string, IValueMatcher> Lookup()
    {
        return ValidatorsCache.Value;
    }

    /// <summary>
    ///     Resolves validator from resource path lookup with given validator resource name.
    ///     Scans assemblies for validator meta-information with the given name and returns instance of validator.
    ///     Returns optional instead of throwing an exception when no validator could be found.
    /// </summary>
    /// <param name="validator">The name of the validator to lookup.</param>
    /// <returns>An Optional containing the validator if found, otherwise an empty Optional.</returns>
    public static Optional<IValueMatcher> Lookup(string validator)
    {
        return IndividualLookupCache.Value.GetOrAdd(validator, key =>
        {
            try
            {
                var instance = TypeResolver.Value.Resolve<IValueMatcher>(key);
                return Optional<IValueMatcher>.Of(instance);
            }
            catch (AgenixSystemException)
            {
                Log.LogWarning("Failed to resolve value matcher from resource '{Validator}'", key);
                return Optional<IValueMatcher>.Empty;
            }
        });
    }
}
