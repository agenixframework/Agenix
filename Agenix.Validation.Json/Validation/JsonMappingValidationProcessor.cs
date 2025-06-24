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
using Agenix.Api.Spi;
using Agenix.Api.Validation;
using Agenix.Core.Util;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Agenix.Core.Validation.Json;

/// <summary>
///     A specialized validation processor that validates JSON mapped objects using a specified
///     <see cref="JsonSerializer" />.
///     Inherits from <see cref="AbstractValidationProcessor{T}" />.
/// </summary>
/// <typeparam name="T">The type of object to be validated.</typeparam>
public abstract class JsonMappingValidationProcessor<T>(JsonSerializer jsonSerializer)
    : AbstractValidationProcessor<T>
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(JsonMappingValidationProcessor<T>).Name);

    private JsonSerializer _jsonSerializer = jsonSerializer;

    // Factory method to initialize the builder
    public static Builder<T> Validate()
    {
        return Builder<T>.Validate();
    }

    /// <summary>
    ///     Reads and deserializes JSON payload from the given message.
    /// </summary>
    /// <param name="message">The message containing the JSON payload to be deserialized.</param>
    /// <returns>The deserialized object of type T.</returns>
    /// <exception cref="AgenixSystemException">Thrown when the JSON payload cannot be deserialized.</exception>
    private T ReadJson(IMessage message)
    {
        if (_jsonSerializer == null)
        {
            ObjectHelper.AssertNotNull(ReferenceResolver,
                "Json mapping validation callback requires 'Newtonsoft.Json.JsonSerializer' instance " +
                "or proper reference resolver with nested poco definition of type marshaller");

            _jsonSerializer = ReferenceResolver.Resolve<JsonSerializer>();
        }

        try
        {
            var payload = message.GetPayload<string>(); // Get the payload as string
            using var stringReader = new StringReader(payload);
            using var jsonReader = new JsonTextReader(stringReader);
            return _jsonSerializer.Deserialize<T>(jsonReader) ??
                   throw new InvalidOperationException("Failed to deserialize JSON payload");
        }
        catch (JsonException e)
        {
            throw new AgenixSystemException("Failed to unmarshal message payload", e);
        }
    }

    /// <summary>
    ///     Validates the JSON payload from the given message within the specified context.
    /// </summary>
    /// <param name="message">The message containing the JSON payload to be validated.</param>
    /// <param name="context">The test context in which the validation is executed.</param>
    /// <exception cref="AgenixSystemException">Thrown when validation of the JSON payload fails.</exception>
    public new void Validate(IMessage message, TestContext context)
    {
        Log.LogDebug("Start JSON object validation ...");

        Validate(ReadJson(message), message.GetHeaders(), context);

        Log.LogDebug("JSON object validation successful: All values OK");
    }

    /// <summary>
    ///     Responsible for building instances of <see cref="JsonMappingValidationProcessor{T}" />.
    /// </summary>
    /// <typeparam name="TB">The type of object to be validated by the processor.</typeparam>
    public class Builder<TB> : IMessageProcessor.IBuilder<JsonMappingValidationProcessor<TB>, Builder<TB>>,
        IReferenceResolverAware
    {
        private JsonSerializer _jsonSerializer;
        private IReferenceResolver _referenceResolver;
        private GenericValidationProcessor<TB> _validationProcessor;

        // Default constructor, no need to pass Type
        /// <summary>
        ///     Responsible for building instances of <see cref="JsonMappingValidationProcessor{T}" />.
        /// </summary>
        /// <typeparam name="TB">The type of object to be validated by the processor.</typeparam>
        public Builder()
        {
        }

        /// <summary>
        ///     Builds a new instance of <see cref="JsonMappingValidationProcessor{TB}" /> with the configured settings.
        /// </summary>
        /// <returns>A new instance of <see cref="JsonMappingValidationProcessor{TB}" />.</returns>
        /// <exception cref="AgenixSystemException">Thrown when the JsonSerializer or validation processor is not set.</exception>
        public JsonMappingValidationProcessor<TB> Build()
        {
            // Ensure the _jsonSerializer is either set or resolved
            if (_jsonSerializer == null)
            {
                if (_referenceResolver != null)
                {
                    _jsonSerializer = _referenceResolver.Resolve<JsonSerializer>();
                }
                else
                {
                    throw new AgenixSystemException(
                        "Missing JsonSerializer - please set proper serializer or reference resolver");
                }
            }

            // Ensure a validation processor is set
            if (_validationProcessor == null)
            {
                throw new AgenixSystemException("Missing validation processor - please add proper validation logic");
            }

            return new ConcreteJsonMappingValidationProcessor<TB>(_jsonSerializer, _validationProcessor);
        }

        /// <summary>
        ///     Sets the reference resolver for the builder.
        /// </summary>
        /// <param name="referenceResolver">The reference resolver to be set.</param>
        public void SetReferenceResolver(IReferenceResolver referenceResolver)
        {
            _referenceResolver = referenceResolver;
        }

        // Static factory method to create a new builder instance
        /// <summary>
        ///     Validates the provided message within the given test context.
        /// </summary>
        public static Builder<TB> Validate()
        {
            return new Builder<TB>();
        }

        // Method to set the validation processor
        /// <summary>
        ///     Sets the validation processor for the JsonMappingValidationProcessor.
        /// </summary>
        /// <param name="validationProcessor">
        ///     The validation processor implementing <see cref="IGenericValidationProcessor{TB}" />
        ///     to be used for validating the objects.
        /// </param>
        /// <returns>A reference to the current instance of the <see cref="Builder{TB}" /> for method chaining.</returns>
        public Builder<TB> Validator(GenericValidationProcessor<TB> validationProcessor)
        {
            _validationProcessor = validationProcessor;
            return this;
        }

        // Method to set the object mapper
        /// <summary>
        ///     Configures the JsonSerializer to be used by the JsonMappingValidationProcessor.
        /// </summary>
        /// <param name="jsonSerializer">The JsonSerializer instance to use for JSON serialization and deserialization operations.</param>
        /// <returns>An instance of the Builder configured with the specified JsonSerializer.</returns>
        public Builder<TB> JsonSerializer(JsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
            return this;
        }

        // Method to set the reference resolver
        /// <summary>
        ///     Sets the reference resolver to be used by the <see cref="JsonMappingValidationProcessor{T}.Builder{TB}" />.
        /// </summary>
        /// <param name="referenceResolver">The reference resolver to be set.</param>
        /// <returns>
        ///     The current instance of <see cref="JsonMappingValidationProcessor{T}.Builder{TB}" /> to facilitate method
        ///     chaining.
        /// </returns>
        public Builder<TB> WithReferenceResolver(IReferenceResolver referenceResolver)
        {
            _referenceResolver = referenceResolver;
            return this;
        }

        /// <summary>
        ///     A concrete implementation of <see cref="JsonMappingValidationProcessor{T}" />.
        ///     This class uses a provided <see cref="JsonSerializer" /> and a validation processor to validate JSON mapped
        ///     objects.
        /// </summary>
        /// <typeparam name="T">The type of object to be validated.</typeparam>
        private class ConcreteJsonMappingValidationProcessor<TC>(
            JsonSerializer jsonSerializer,
            GenericValidationProcessor<TC> validationProcessor)
            : JsonMappingValidationProcessor<TC>(jsonSerializer)
        {
            /// <summary>
            ///     Validates the given payload using the specified validation processor.
            /// </summary>
            /// <param name="payload">The object of type TC to be validated.</param>
            /// <param name="headers">A dictionary of headers that may contain additional information for validation.</param>
            /// <param name="context">The current test context.</param>
            public override void Validate(TC payload, IDictionary<string, object> headers, TestContext context)
            {
                validationProcessor.Invoke(payload, headers, context);
            }
        }
    }
}
