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
using Agenix.Api.Log;
using Agenix.Api.Spi;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Validation.Matcher;

/// <summary>
///     Represents an interface for implementing validation matchers in the validation framework.
/// </summary>
public interface IValidationMatcher
{
    /// <summary>
    ///     A static instance of the logging mechanism used for capturing and managing log events within the system.
    /// </summary>
    /// <remarks>
    ///     This variable is initialized using the logging framework and is primarily used to log diagnostic
    ///     and operational information to assist with debugging and monitoring of the validation processes.
    ///     It is tied to the <see cref="IValidationMatcher" /> interface for context-specific logging.
    ///     Typical use cases include logging validation events, warnings, errors, or informational messages.
    /// </remarks>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(IValidationMatcher));

    /// <summary>
    ///     Represents the path used to identify and locate the resource associated with the ValidationMatcher implementation.
    /// </summary>
    static string ResourcePath => "Extension/agenix/validation/matcher";

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
    ///     Lazy-initialized cache of validation matchers for improved performance and thread safety.
    /// </summary>
    /// <remarks>
    ///     This collection serves as a central repository for registering and retrieving instances of classes
    ///     that provide validation logic, implementing the <see cref="IValidationMatcher" /> interface.
    ///     It supports dynamic resolution and reuse of validation matchers within the system.
    /// </remarks>
    private static readonly Lazy<IDictionary<string, IValidationMatcher>> ValidatorsCache =
        new(() => LoadValidationMatchers());

    /// <summary>
    ///     Loads all available validation matchers from the type resolver.
    /// </summary>
    /// <returns>A dictionary containing all loaded validation matchers.</returns>
    private static IDictionary<string, IValidationMatcher> LoadValidationMatchers()
    {
        var validators = new ConcurrentDictionary<string, IValidationMatcher>();

        try
        {
            var resolvedValidators = TypeResolver.Value.ResolveAll<dynamic>();

            foreach (var kvp in resolvedValidators)
            {
                validators[kvp.Key] = kvp.Value;
            }

            if (Log.IsEnabled(LogLevel.Debug))
            {
                foreach (var kvp in validators)
                {
                    Log.LogDebug("Found validation matcher '{Key}' as {Type}", kvp.Key, kvp.Value.GetType());
                }
            }
        }
        catch (Exception ex)
        {
            Log.LogWarning("Failed to load validation matchers from resource path: {Error}", ex.Message);
        }

        return validators;
    }

    /// <summary>
    ///     Method called on validation.
    /// </summary>
    /// <param name="fieldName">The field name for logging purposes.</param>
    /// <param name="value">The value to be validated.</param>
    /// <param name="controlParameters">The control parameters for validation.</param>
    /// <param name="context">The test context providing validation environment.</param>
    void Validate(string fieldName, string value, List<string>? controlParameters, TestContext context);

    /// <summary>
    ///     Resolves all available validation matchers from the resource path lookup.
    ///     Scans assemblies for validator meta-information and instantiates those validators.
    /// </summary>
    /// <returns>A dictionary containing the registered validation matchers.</returns>
    static IDictionary<string, IValidationMatcher> Lookup()
    {
        return ValidatorsCache.Value;
    }
}
