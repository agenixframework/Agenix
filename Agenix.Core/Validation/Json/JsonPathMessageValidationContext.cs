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

using System.Collections.Generic;
using Agenix.Api;
using Agenix.Api.Builder;
using Agenix.Api.Message;
using Agenix.Api.Validation.Context;
using Agenix.Api.Variable;
using Agenix.Core.Message;

namespace Agenix.Core.Validation.Json;

/// <summary>
///     Specialized validation context adds JSON path expressions for message validation.
/// </summary>
public class JsonPathMessageValidationContext(JsonPathMessageValidationContext.Builder builder)
    : DefaultValidationContext
{
    // Default constructor
    public JsonPathMessageValidationContext() : this(new Builder())
    {
    }

    // Get the control message elements that have to be present in
    // the received message. Message element values are compared as well.
    public IDictionary<string, object> JsonPathExpressions { get; } = builder._expressions;

    // Check whether the given path expression is a JSONPath expression
    public static bool IsJsonPathExpression(string pathExpression)
    {
        return !string.IsNullOrEmpty(pathExpression) && pathExpression.StartsWith('$');
    }

    // Fluent builder
    public class Builder : IValidationContext.IBuilder<JsonPathMessageValidationContext, IBuilder>,
        IWithExpressions<Builder>, IVariableExtractorAdapter, IMessageProcessorAdapter, IBuilder
    {
        internal readonly Dictionary<string, object> _expressions = new();

        public JsonPathMessageValidationContext Build()
        {
            return new JsonPathMessageValidationContext(this);
        }

        /// <summary>
        ///     Converts the current builder configuration into an instance of IMessageProcessor.
        ///     This method is used to produce a message processor based on the configured expressions and settings applied to the
        ///     builder.
        /// </summary>
        /// <returns>An instance of IMessageProcessor configured with the current builder's expressions.</returns>
        public IMessageProcessor AsProcessor()
        {
            return new DelegatingPathExpressionProcessor.Builder()
                .Expressions(_expressions)
                .Build();
        }

        /// <summary>
        ///     Converts the current builder configuration into an instance of IVariableExtractor.
        ///     This method is used to produce a variable extractor based on the expressions and settings applied to the builder.
        /// </summary>
        /// <returns>An instance of IVariableExtractor configured with the current builder's expressions.</returns>
        public IVariableExtractor AsExtractor()
        {
            return new DelegatingPayloadVariableExtractor.Builder()
                .Expressions(_expressions)
                .Build();
        }

        /// <summary>
        ///     Updates the internal collection of expressions with the specified key-value pairs.
        ///     This method is used to define or modify the expressions that can be used for validation or processing purposes.
        /// </summary>
        /// <param name="expressions">
        ///     A dictionary containing key-value pairs where keys represent expression names and values
        ///     represent the associated expression objects.
        /// </param>
        /// <returns>The current instance of the builder, allowing for fluent configuration.</returns>
        public Builder Expressions(IDictionary<string, object> expressions)
        {
            foreach (var kvp in expressions)
            {
                _expressions[kvp.Key] = kvp.Value;
            }

            return this;
        }

        /// <summary>
        ///     Adds an expression and its associated value to the builder configuration.
        ///     This method is used to define expressions that will be incorporated into the validation context.
        /// </summary>
        /// <param name="expression">The expression key to be added to the builder configuration.</param>
        /// <param name="value">The value associated with the provided expression key.</param>
        /// <returns>The current instance of the builder with the updated expression configuration.</returns>
        public Builder Expression(string expression, object value)
        {
            _expressions[expression] = value;
            return this;
        }

        // Static entry method for fluent builder API
        public static Builder JsonPath()
        {
            return new Builder();
        }
    }
}
