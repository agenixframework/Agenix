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
using Agenix.Api.Message;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Core.Validation;
using Agenix.Core.Validation.Json;
using Agenix.Validation.Json.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Agenix.Validation.Json.Validation;

/// <summary>
///     Message validator evaluates a set of JSONPath expressions on the message payload and checks that values are as
///     expected.
/// </summary>
public class JsonPathMessageValidator : AbstractMessageValidator<JsonPathMessageValidationContext>,
    IMessageValidator<IValidationContext>
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(JsonPathMessageValidator));

    /// <summary>
    ///     Determines if the given message type is supported by evaluating
    ///     whether the message type is JSON and has a valid JSON payload.
    /// </summary>
    /// <param name="messageType">The type of the message to check.</param>
    /// <param name="message">The message instance to validate.</param>
    /// <returns>
    ///     A boolean value indicating whether the message type is supported.
    /// </returns>
    public override bool SupportsMessageType(string messageType, IMessage message)
    {
        return new JsonTextMessageValidator().SupportsMessageType(messageType, message);
    }

    /// <summary>
    ///     Validates the received message against the control message using JSONPath expressions
    ///     specified in the validation context.
    /// </summary>
    /// <param name="receivedMessage">The message that was received and needs validation.</param>
    /// <param name="controlMessage">The control message to validate against.</param>
    /// <param name="context">The execution context for the current test.</param>
    /// <param name="validationContext">The context containing JSONPath expressions and their expected values.</param>
    /// <exception cref="ValidationException">Thrown when the received message payload is empty or validation fails.</exception>
    /// <exception cref="AgenixSystemException">Thrown when the received message payload cannot be parsed as JSON.</exception>
    public override void ValidateMessage(IMessage receivedMessage, IMessage controlMessage, TestContext context,
        JsonPathMessageValidationContext validationContext)
    {
        if (validationContext.JsonPathExpressions == null ||
            validationContext.JsonPathExpressions.Count == 0)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(receivedMessage.GetPayload<string>()))
        {
            throw new ValidationException("Unable to validate message elements - receive message payload was empty");
        }

        Log.LogDebug("Start JSONPath element validation ...");

        try
        {
            var readerContext = JToken.Parse(receivedMessage.GetPayload<string>());

            foreach (var (key, value) in validationContext.JsonPathExpressions)
            {
                var expectedValue = value;

                if (expectedValue is string)
                //check if the expected value is variable or function (and resolve it, if yes)
                {
                    expectedValue = context.ReplaceDynamicContentInString(expectedValue.ToString());
                }

                var jsonPathExpression = context.ReplaceDynamicContentInString(key);
                var jsonPathResult = JsonPathUtils.EvaluateAsString(readerContext, jsonPathExpression);
                //do the validation of actual and expected value for an element
                ValidationUtils.ValidateValues(jsonPathResult, expectedValue, jsonPathExpression, context);

                Log.LogDebug("Validating element: {0}='{1}': OK", jsonPathExpression, expectedValue);
            }

            Log.LogDebug("JSONPath element validation successful: All values OK");
        }
        catch (JsonReaderException e)
        {
            throw new AgenixSystemException("Failed to parse JSON text", e);
        }
    }

    /// <summary>
    ///     Finds and processes the appropriate validation context from a list of validation contexts.
    ///     Combines multiple matching validation contexts into a single context if necessary, updating other related contexts
    ///     accordingly.
    /// </summary>
    /// <param name="validationContexts">
    ///     A list of validation contexts from which the appropriate validation context will be located and processed.
    /// </param>
    /// <returns>
    ///     A <see cref="JsonPathMessageValidationContext" /> instance representing the consolidated validation context if
    ///     multiple matching contexts exist,
    ///     or the result from the base implementation if no consolidation is required.
    /// </returns>
    public override JsonPathMessageValidationContext FindValidationContext(List<IValidationContext> validationContexts)
    {
        var jsonPathMessageValidationContexts = validationContexts
            .OfType<JsonPathMessageValidationContext>()
            .ToList();

        if (jsonPathMessageValidationContexts.Count > 1)
        {
            var jsonPathMessageValidationContext = jsonPathMessageValidationContexts[0];

            // Collect all jsonPath expressions and combine into one single validation context
            var jsonPathExpressions = jsonPathMessageValidationContexts
                .Select(context => context.JsonPathExpressions)
                .Aggregate(new Dictionary<string, object>(), (collect, map) =>
                {
                    foreach (var item in map)
                    {
                        collect[item.Key] = item.Value;
                    }

                    return collect;
                });


            if (jsonPathExpressions.Count == 0)
            {
                return base.FindValidationContext(validationContexts);
            }

            foreach (var expression in jsonPathExpressions)
            {
                jsonPathMessageValidationContext.JsonPathExpressions[expression.Key] = expression.Value;
            }

            // Update the status of other validation contexts to optional as validation is performed by this single context
            jsonPathMessageValidationContexts
                .Where(vc => vc != jsonPathMessageValidationContext)
                .ToList()
                .ForEach(vc => vc.UpdateStatus(ValidationStatus.OPTIONAL));

            return jsonPathMessageValidationContext;
        }

        return base.FindValidationContext(validationContexts);
    }

    /// <summary>
    ///     Retrieves the required type of validation context for the JsonPathMessageValidator.
    /// </summary>
    /// <returns>
    ///     A <see cref="Type" /> representing the required validation context,
    ///     which is <see cref="JsonPathMessageValidationContext" />.
    /// </returns>
    protected override Type GetRequiredValidationContextType()
    {
        return typeof(JsonPathMessageValidationContext);
    }
}
