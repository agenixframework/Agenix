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
using Agenix.Api.Message;
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Agenix.Api.Validation.Context;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Validation;

/// <summary>
///     Defines an interface for schema validation, enabling the validation of messages
///     against specific schemas and providing support for different message types and validation
///     contexts.
/// </summary>
/// <typeparam name="T">
///     The context type that implements <see cref="ISchemaValidationContext" /> and
///     provides additional data or configuration required for schema validation.
/// </typeparam>
public interface ISchemaValidator<in T> where T : ISchemaValidationContext
{
    /// <summary>
    ///     Schema validator resource lookup path
    /// </summary>
    private const string ResourcePath = "Extension/agenix/message/schemaValidator";

    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(ISchemaValidator<T>).Name);

    /// <summary>
    ///     Lazy-initialized type resolver for runtime class reference mapping in schema validation processes.
    /// </summary>
    private static readonly Lazy<ResourcePathTypeResolver> TypeResolver =
        new(() => new ResourcePathTypeResolver(ResourcePath));

    /// <summary>
    ///     Lazy-initialized cache of schema validators for improved performance and thread safety.
    /// </summary>
    private static readonly Lazy<IDictionary<string, ISchemaValidator<T>>> ValidatorsCache =
        new(LoadSchemaValidators);

    /// <summary>
    ///     Lazy-initialized cache for individual validator lookups to avoid repeated resolution attempts.
    /// </summary>
    private static readonly Lazy<ConcurrentDictionary<string, Optional<ISchemaValidator<T>>>> IndividualLookupCache =
        new(() => new ConcurrentDictionary<string, Optional<ISchemaValidator<T>>>());

    /// <summary>
    ///     Loads all available schema validators from the type resolver.
    /// </summary>
    /// <returns>A dictionary containing all loaded schema validators.</returns>
    private static IDictionary<string, ISchemaValidator<T>> LoadSchemaValidators()
    {
        var validators = new ConcurrentDictionary<string, ISchemaValidator<T>>();

        try
        {
            var resolvedSchemas = TypeResolver.Value.ResolveAll<ISchemaValidator<T>>("", ITypeResolver.DEFAULT_TYPE_PROPERTY, "name");

            foreach (var kvp in resolvedSchemas)
            {
                validators[kvp.Key] = kvp.Value;
            }

            if (Log.IsEnabled(LogLevel.Debug))
            {
                foreach (var kvp in validators)
                {
                    Log.LogDebug("Found schema validator '{Key}' as {Type}", kvp.Key, kvp.Value.GetType());
                }
            }
        }
        catch (Exception ex)
        {
            Log.LogWarning("Failed to load schema validators from resource path: {Error}", ex.Message);
        }

        return validators;
    }

    /// <summary>
    ///     Resolves all available validators from the defined resource path.
    ///     Scans assemblies for schema validator metadata and creates instances
    ///     of the associated types.
    /// </summary>
    /// <returns>
    ///     A dictionary containing the instantiated schema validators,
    ///     keyed by their identifiers.
    /// </returns>
    static IDictionary<string, ISchemaValidator<T>> Lookup()
    {
        return ValidatorsCache.Value;
    }

    /// <summary>
    ///     Resolves all available schema validators from the defined resource path lookup.
    ///     Scans assemblies for validator meta-information and returns instantiated validators as a collection.
    /// </summary>
    /// <param name="validator">The name of the validator to lookup.</param>
    /// <returns>An Optional containing the validator if found, otherwise an empty Optional.</returns>
    public static Optional<ISchemaValidator<T>> Lookup(string validator)
    {
        return IndividualLookupCache.Value.GetOrAdd(validator, key =>
        {
            try
            {
                var instance = TypeResolver.Value.Resolve<ISchemaValidator<T>>(key, ITypeResolver.DEFAULT_TYPE_PROPERTY);
                return Optional<ISchemaValidator<T>>.Of(instance);
            }
            catch (AgenixSystemException)
            {
                Log.LogWarning("Failed to resolve validator from resource '{Validator}'", key);
                return Optional<ISchemaValidator<T>>.Empty;
            }
        });
    }

    /// <summary>
    ///     Performs validation of the specified message within the given test and schema validation contexts.
    /// </summary>
    /// <param name="message">The instance of <see cref="IMessage" /> to be validated.</param>
    /// <param name="context">The context of the current test execution, providing runtime variables and utilities.</param>
    /// <param name="validationContext">The schema validation context specific to the validation environment.</param>
    void Validate(IMessage message, TestContext context, T validationContext);

    /// <summary>
    ///     Determines whether the specified message type is supported by this schema validator.
    ///     This is typically used to check if a specific message type and its associated message
    ///     can be validated with the current schema validator implementation.
    /// </summary>
    /// <param name="messageType">The type of the message to validate support for.</param>
    /// <param name="message">The message instance to validate support against.</param>
    /// <returns>True if the specified message type and message are supported; otherwise, false.</returns>
    bool SupportsMessageType(string messageType, IMessage message);

    /// <summary>
    ///     Determines if the provided message can be validated, considering the schema validation
    ///     configuration and message context.
    /// </summary>
    /// <param name="message">The message to be evaluated for validation.</param>
    /// <param name="schemaValidationEnabled">A flag indicating whether schema validation is enabled.</param>
    /// <returns>True if the message can be validated; otherwise, false.</returns>
    bool CanValidate(IMessage message, bool schemaValidationEnabled);

    /// <summary>
    ///     Validates a given message against the specified schema and context.
    ///     Ensures that the message adheres to the defined validation rules
    ///     and schema requirements.
    /// </summary>
    /// <param name="message">The message to validate.</param>
    /// <param name="context">The test context providing state and configuration for the validation process.</param>
    /// <param name="schemaRepository">The identifier for the repository containing the relevant schemas.</param>
    /// <param name="schema">The specific schema to validate the message against.</param>
    void Validate(IMessage message, TestContext context, string schemaRepository, string schema);
}
