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
using Agenix.Api.Validation.Context;
using Agenix.Validation.Json.Json;
using Agenix.Validation.Json.Json.Schema;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Json.Validation.Schema;

/// This class is responsible for filtering `SimpleJsonSchema` objects based on the provided `IMessageValidationContext`.
/// /
public class JsonSchemaFilter
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(nameof(JsonSchemaFilter));

    /// Filters all schema repositories based on the configuration in the provided `IMessageValidationContext`
    /// and returns a list of relevant `SimpleJsonSchema` objects for the validation process.
    /// <param name="schemaRepositories">The repositories containing available schemas to be filtered</param>
    /// <param name="jsonMessageValidationContext">The context that contains configuration for message validation</param>
    /// <param name="referenceResolver">The reference resolver used for retrieving bean references during lookup</param>
    /// <return>A list of filtered `SimpleJsonSchema` objects that match the validation configuration</return>
    /// /
    public virtual List<SimpleJsonSchema> Filter(List<JsonSchemaRepository> schemaRepositories,
        ISchemaValidationContext jsonMessageValidationContext,
        IReferenceResolver referenceResolver)
    {
        if (IsSchemaRepositorySpecified(jsonMessageValidationContext))
        {
            return FilterByRepositoryName(schemaRepositories, jsonMessageValidationContext);
        }

        return IsSchemaSpecified(jsonMessageValidationContext)
            ? GetSchemaFromContext(jsonMessageValidationContext, referenceResolver)
            : MergeRepositories(schemaRepositories);
    }

    private static List<SimpleJsonSchema> GetSchemaFromContext(ISchemaValidationContext jsonMessageValidationContext,
        IReferenceResolver referenceResolver)
    {
        var simpleJsonSchema =
            referenceResolver.Resolve<SimpleJsonSchema>(jsonMessageValidationContext.Schema);

        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug("Found specified schema: \"{Schema}\".", jsonMessageValidationContext.Schema);
        }

        return new List<SimpleJsonSchema> { simpleJsonSchema };
    }

    private static List<SimpleJsonSchema> FilterByRepositoryName(List<JsonSchemaRepository> schemaRepositories,
        ISchemaValidationContext jsonMessageValidationContext)
    {
        foreach (var jsonSchemaRepository in schemaRepositories.Where(jsonSchemaRepository => string.Equals(
                     jsonSchemaRepository.Name, jsonMessageValidationContext.SchemaRepository,
                     StringComparison.Ordinal)))
        {
            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug("Found specified schema-repository: \"{SchemaRepository}\".",
                    jsonMessageValidationContext.SchemaRepository);
            }

            return jsonSchemaRepository.Schemas;
        }

        throw new AgenixSystemException(
            $"Could not find the specified schema repository: \"{jsonMessageValidationContext.SchemaRepository}\".");
    }

    /// Merges the schemas from the provided list of JSON schema repositories into a single list of SimpleJsonSchema.
    /// <param name="schemaRepositories">The list of JSON schema repositories containing schemas to be merged</param>
    /// <return>A consolidated list of SimpleJsonSchema from all provided repositories</return>
    /// /
    private static List<SimpleJsonSchema> MergeRepositories(List<JsonSchemaRepository> schemaRepositories)
    {
        return schemaRepositories
            .SelectMany(repo => repo.Schemas)
            .ToList();
    }

    /// Determines if a schema is specified within the given {@link IMessageValidationContext}.
    /// <param name="context">The context containing schema-related information.</param>
    /// <return>True if the schema is specified in the context; otherwise, false.</return>
    /// /
    private static bool IsSchemaSpecified(ISchemaValidationContext context)
    {
        return !string.IsNullOrEmpty(context.Schema) && !string.IsNullOrWhiteSpace(context.Schema);
    }

    /// Determines if a schema repository is specified in the given message validation context.
    /// <param name="context">The validation context that contains schema repository-related configuration.</param>
    /// <return><c>true</c> if a schema repository is specified; otherwise, <c>false</c>.</return>
    /// /
    private static bool IsSchemaRepositorySpecified(ISchemaValidationContext context)
    {
        return !string.IsNullOrEmpty(context.SchemaRepository) && !string.IsNullOrWhiteSpace(context.SchemaRepository);
    }
}
