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
using Agenix.Api.Builder;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Core.Validation.Json;

namespace Agenix.Core.Message;

/// Represents a generic processor that delegates processing to specific implementations,
/// such as JSONPath or XPath processors, based on the type of the provided path expression.
/// The delegation is determined through a resource path lookup.
/// This processor supports handling multiple expressions and allows dynamic configuration
/// through a builder pattern.
/// /
public class DelegatingPathExpressionProcessor : IMessageProcessor
{
    /// Stores path expressions used for delegating processing tasks to specific
    /// JSONPath or XPath message processors based on the type of the expressions.
    private readonly IDictionary<string, object> _pathExpressions;

    public DelegatingPathExpressionProcessor()
    {
        _pathExpressions = new Builder().PathExpressions;
    }

    public DelegatingPathExpressionProcessor(Builder builder)
    {
        _pathExpressions = builder.PathExpressions;
    }

    /// Represents a collection of path expressions used for delegating JSONPath or XPath message processing.
    /// Path expressions are mapped to specific values and serve as the basis for delegation decisions.
    /// /
    public IDictionary<string, object> PathExpressions => _pathExpressions;

    public void Process(IMessage message, TestContext context)
    {
        if (_pathExpressions.Count == 0)
        {
            return;
        }

        var jsonPathExpressions = new Dictionary<string, object>();
        var xpathExpressions = new Dictionary<string, object>();

        foreach (var pathExpression in _pathExpressions)
        {
            var path = context.ReplaceDynamicContentInString(pathExpression.Key);
            var variable = pathExpression.Value;

            if (JsonPathMessageValidationContext.IsJsonPathExpression(path))
            {
                jsonPathExpressions.Add(path, variable);
            }
            else
            {
                xpathExpressions.Add(path, variable);
            }
        }

        if (jsonPathExpressions.Count > 0)
        {
            var jsonPathProcessor = LookupMessageProcessor<IMessageProcessor, Builder>("jsonPath", context);

            if (jsonPathProcessor is IWithExpressions<Builder> expressions)
            {
                expressions.Expressions(jsonPathExpressions);
            }

            jsonPathProcessor.Build()
                .Process(message, context);
        }

        if (xpathExpressions.Count <= 0)
        {
            return;
        }

        {
            var xpathProcessor = LookupMessageProcessor<IMessageProcessor, Builder>("xpath", context);

            if (xpathProcessor is IWithExpressions<Builder> expressions)
            {
                expressions.Expressions(xpathExpressions);
            }

            xpathProcessor.Build()
                .Process(message, context);
        }
    }

    /// Looks up and returns a message processor builder of the specified type using the provided context.
    /// <param name="type">The type of the message processor builder to look up.</param>
    /// <param name="context">The context used to resolve or locate the appropriate message processor builder.</param>
    /// <typeparam name="T">The specific type of the message processor.</typeparam>
    /// <typeparam name="TB">The specific type of the message processor builder.</typeparam>
    /// <returns>An instance of the message processor builder if found; otherwise, an exception is thrown.</returns>
    private IMessageProcessor.IBuilder<T, TB> LookupMessageProcessor<T, TB>(string type, TestContext context)
        where T : IMessageProcessor where TB : IMessageProcessor.IBuilder<T, TB>
    {
        var lookup = IMessageProcessor.Lookup<T, TB>(type);
        if (lookup.IsPresent)
        {
            return lookup.Value;
        }

        if (context.ReferenceResolver.IsResolvable(type, typeof(IMessageProcessor.IBuilder<T, TB>)))
        {
            return context.ReferenceResolver.Resolve<IMessageProcessor.IBuilder<T, TB>>(type);
        }

        if (context.ReferenceResolver
            .IsResolvable(type + "MessageProcessorBuilder", typeof(IMessageProcessor.IBuilder<T, TB>)))
        {
            return context.ReferenceResolver
                .Resolve<IMessageProcessor.IBuilder<T, TB>>(type + "MessageProcessorBuilder");
        }

        throw new AgenixSystemException($"Missing proper message processor implementation of type '{type}' - " +
                                        "consider adding proper module to the project");
    }

    /// Fluent builder enabling the construction of the DelegatingPathExpressionProcessor with specific configurations.
    /// Allows defining and managing a collection of path expressions used in JSONPath or XPath processing.
    /// /
    public sealed class Builder : IMessageProcessor.IBuilder<DelegatingPathExpressionProcessor, Builder>,
        IWithExpressions<Builder>
    {
        /// Represents a collection of all registered expressions, allowing retrieval of key-value pairs
        /// where each key is an expression (e.g., JSONPath or XPath), and the value is the associated data or mapping.
        public IDictionary<string, object> PathExpressions { get; } = new Dictionary<string, object>();

        /// Completes the construction process and returns a fully configured instance
        /// of DelegatingPathExpressionProcessor.
        /// <return>An instance of DelegatingPathExpressionProcessor initialized with the provided configurations.</return>
        public DelegatingPathExpressionProcessor Build()
        {
            return new DelegatingPathExpressionProcessor(this);
        }

        /// Adds the provided expressions to the builder's configuration.
        /// <param name="expressions">
        ///     A dictionary containing string keys and associated object values representing the expressions
        ///     to configure.
        /// </param>
        /// <returns>The current instance of the builder with updated expressions.</returns>
        public Builder Expressions(IDictionary<string, object> expressions)
        {
            foreach (var entry in expressions)
            {
                PathExpressions[entry.Key] = entry.Value;
            }

            return this;
        }

        /// Adds an expression and its associated value to the collection.
        /// <param name="expression">The key representing the expression to be added.</param>
        /// <param name="value">The value associated with the given expression.</param>
        /// <return>The current instance of the builder for chaining additional method calls.</return>
        public Builder Expression(string expression, object value)
        {
            PathExpressions[expression] = value;
            return this;
        }

        /// Initiates the builder with an XPath expression context.
        /// @return A new instance of the builder pre-configured for XPath expressions.
        /// /
        public static Builder Xpath()
        {
            return new Builder();
        }

        /// Configures the builder for JSONPath expressions.
        /// @return A new instance of the builder pre-configured for JSONPath expressions.
        /// /
        public static Builder JsonPath()
        {
            return new Builder();
        }
    }
}
