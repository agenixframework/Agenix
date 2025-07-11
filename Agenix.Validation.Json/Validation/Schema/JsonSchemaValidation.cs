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

using System.Text;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Core.Validation.Json;
using Agenix.Validation.Json.Json;
using Agenix.Validation.Json.Json.Schema;
using Agenix.Validation.Json.Validation.Report;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace Agenix.Validation.Json.Validation.Schema;

/// Represents a validator for JSON messages, ensuring compliance with JSON schemas or schema repositories.
/// This class leverages filtering and validation mechanisms for efficient and accurate processing.
/// /
public class JsonSchemaValidation(JsonSchemaFilter jsonSchemaFilter) : ISchemaValidator<ISchemaValidationContext>
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(nameof(JsonSchemaValidation));

    public JsonSchemaValidation() : this(new JsonSchemaFilter())
    {
    }

    public void Validate(IMessage message, TestContext context, ISchemaValidationContext validationContext)
    {
        Log.LogDebug("Starting Json schema validation ...");

        var report = Validate(message,
            FindSchemaRepositories(context),
            validationContext,
            context.ReferenceResolver);

        if (!report.IsSuccess)
        {
            if (Log.IsEnabled(LogLevel.Error))
            {
                Log.LogError("Failed to validate Json schema for message:\n{GetPayload}", message.GetPayload<string>());
            }

            throw new ValidationException(ConstructErrorMessage(report));
        }

        Log.LogDebug("Json schema validation successful: All values OK");
    }

    /// Checks whether the specified message supports the provided message type.
    /// @param messageType the type of the message to check
    /// @param message the message instance to evaluate
    /// @return true if this validator supports the message or its type, otherwise false
    /// /
    public bool SupportsMessageType(string messageType, IMessage? message)
    {
        return "JSON".Equals(messageType)
               || (message != null && IsJsonPredicate.Instance.Test(message.GetPayload<string>()));
    }

    /// Determines whether the specified message can be validated based on the provided schema validation flag
    /// and internal JSON schema validation status.
    /// @param message The message to be validated
    /// @param schemaValidationEnabled A flag indicating whether schema validation is enabled
    /// @return True if the message can be validated; otherwise, false
    /// /
    public bool CanValidate(IMessage message, bool schemaValidationEnabled)
    {
        return (IsJsonSchemaValidationEnabled() || schemaValidationEnabled)
               && IsJsonPredicate.Instance.Test(message.GetPayload<string>());
    }

    /// Validates the given message using the provided test context, schema, and schema repository settings.
    /// @param message The message to be validated
    /// @param context The test context used for validation
    /// @param schemaRepository The repository containing the schema definitions
    /// @param schema The schema to validate the message against
    /// /
    public void Validate(IMessage message, TestContext context, string schemaRepository, string schema)
    {
        var validationContext = JsonMessageValidationContext.Builder.Json()
            .SchemaValidation(true)
            .Schema(schema)
            .SchemaRepository(schemaRepository).Build();

        Validate(message, context, validationContext);
    }

    /// Constructs the error message of a failed validation based on the processing report.
    /// @param report The processing report containing the validation error details.
    /// @return A string representation of all validation error messages in the report.
    /// /
    private static string ConstructErrorMessage(GraciousProcessingReport report)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("Json validation failed: ");
        foreach (var processingMessage in report.ValidationErrors)
        {
            stringBuilder.Append("\n\t").Append(processingMessage.Message);
        }

        return stringBuilder.ToString();
    }

    /// Finds all JSON schema repositories available in the provided test context.
    /// @param context The test context from which to retrieve the JSON schema repositories.
    /// @return A list of JSON schema repositories found in the test context.
    /// /
    private static List<JsonSchemaRepository> FindSchemaRepositories(TestContext context)
    {
        if (context.ReferenceResolver == null)
        {
            return [];
        }

        var repositories = context.ReferenceResolver.ResolveAll<JsonSchemaRepository>();
        return repositories != null ? repositories.Values.ToList() : [];
    }

    /// Validates the given message against a list of JSON schema repositories within the provided validation context.
    /// @param message The message to be validated.
    /// @param schemaRepositories The list of schema repositories used for validation.
    /// @param validationContext The validation context to be applied during the validation process.
    /// @param referenceResolver The reference resolver used for resolving external references during validation.
    /// @return A report containing the results of the validation.
    /// /
    public GraciousProcessingReport Validate(IMessage message,
        List<JsonSchemaRepository> schemaRepositories,
        ISchemaValidationContext validationContext,
        IReferenceResolver referenceResolver)
    {
        return Validate(message,
            jsonSchemaFilter.Filter(schemaRepositories, validationContext, referenceResolver));
    }

    /// Validates the specified message against all JSON schemas provided in the list.
    /// @param message     The message to be validated
    /// @param jsonSchemas A collection of JSON schemas to validate the message against
    /// @return A processing report indicating the results of the validation
    /// /
    private GraciousProcessingReport Validate(IMessage message, List<SimpleJsonSchema> jsonSchemas)
    {
        if (jsonSchemas.Count == 0)
        {
            return new GraciousProcessingReport(true);
        }

        var processingReport = new GraciousProcessingReport();
        foreach (var simpleJsonSchema in jsonSchemas)
        {
            processingReport.MergeWith(Validate(message, simpleJsonSchema));
        }

        return processingReport;
    }

    /// Validates a given message against a specified JSON schema.
    /// <param name="message">The message to be validated.</param>
    /// <param name="simpleJsonSchema">The JSON schema to validate the message against.</param>
    /// <returns>A set of validation errors found during the validation process. If no errors exist, an empty set is returned.</returns>
    private static HashSet<ValidationError> Validate(IMessage message, SimpleJsonSchema simpleJsonSchema)
    {
        try
        {
            var receivedJson = JToken.Parse(message.GetPayload<string>());
            if (!receivedJson.HasValues)
            {
                return [];
            }

            receivedJson.IsValid(simpleJsonSchema.Schema!, out IList<ValidationError> errors);

            return [.. errors];
        }
        catch (JsonReaderException e)
        {
            throw new AgenixSystemException("Failed to validate Json schema", e);
        }
    }

    /// Determines whether JSON schema validation is enabled by default.
    /// It checks the settings to decide if outbound schema validation or
    /// outbound JSON schema validation is enabled.
    /// @return True if JSON schema validation is enabled by default; otherwise, false.
    /// /
    private static bool IsJsonSchemaValidationEnabled()
    {
        return AgenixSettings.IsOutboundSchemaValidationEnabled() ||
               AgenixSettings.IsOutboundJsonSchemaValidationEnabled();
    }
}
