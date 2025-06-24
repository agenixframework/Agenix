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
using Agenix.Api.Exceptions;
using Agenix.Api.Validation.Context;
using Agenix.Core.Validation.Json;
using Agenix.Core.Validation.Xml;

namespace Agenix.Core.Validation;

/// Represents a context for validating path-based expressions in various data formats such as JSON and XML.
/// This class is designed to facilitate validation workflows that involve JSONPath and XPath support.
/// It provides methods and a fluent builder API for constructing customized validation contexts
/// to handle complex scenarios with path-based expression requirements.
/// Common use cases involve evaluating structured documents or data configurations
/// that rely on path expressions for validation rules.
public class PathExpressionValidationContext
{
    /// Represents a validation context specifically designed for handling path expressions.
    /// This class provides a flexible and extendable way to build validation contexts
    /// with support for JSONPath and XPath expressions.
    /// /
    private PathExpressionValidationContext()
    {
        // prevent direct instantiation
    }

    /// Provides a fluent builder for constructing and customizing path expression validation contexts.
    /// Designed to work within validation scenarios requiring JSONPath and XPath support, this builder
    /// facilitates the creation of validation contexts that incorporate path-based expressions.
    /// /
    public sealed class Builder : IValidationContext.IBuilder<IValidationContext, Builder>, IWithExpressions<Builder>
    {
        private readonly Dictionary<string, object> _expressions = new();

        public IValidationContext Build()
        {
            if (_expressions.Count == 0)
            {
                return new DefaultValidationContext();
            }

            string expression = null;
            using (var enumerator = _expressions.Keys.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    expression = enumerator.Current;
                }
            }

            if (JsonPathMessageValidationContext.IsJsonPathExpression(expression))
            {
                return new JsonPathMessageValidationContext.Builder()
                    .Expressions(_expressions)
                    .Build();
            }

            if (XpathMessageValidationContext.IsXpathExpression(expression))
            {
                return new XpathMessageValidationContext.Builder()
                    .Expressions(_expressions)
                    .Build();
            }

            throw new AgenixSystemException($"Unsupported path expression '{expression}'");
        }

        /// Provides methods to work with path expressions within a validation context.
        /// This class is designed to facilitate the management and usage of path expressions,
        /// including JSONPath and XPath, in custom validation scenarios.
        public Builder Expressions(IDictionary<string, object> expressions)
        {
            foreach (var kvp in expressions)
            {
                _expressions[kvp.Key] = kvp.Value;
            }

            return this;
        }

        /// Represents an expression used for configuring and managing path-based validations,
        /// including the handling of JSONPath and XPath expressions within a validation context.
        public Builder Expression(string expression, object value)
        {
            _expressions[expression] = value;
            return this;
        }

        /// Provides a static entry method for creating a fluent builder API associated with path expressions.
        /// This method allows for constructing and customizing validation contexts with support for JSONPath and XPath expressions.
        /// The resulting builder can be used to define expressions and build validation contexts to suit specific requirements.
        /// <return>Returns a new instance of the Builder class for constructing path expression validation contexts.</return>
        public static Builder PathExpression()
        {
            return new Builder();
        }

        /// Adds a JSON path expression and its associated value to the validation context.
        /// <param name="expression">
        ///     The JSON path expression. This should be a valid JSONPath starting with '$'.
        /// </param>
        /// <param name="value">
        ///     The value associated with the specified JSON path expression.
        /// </param>
        /// <returns>
        ///     The builder instance with the added JSON path expression, enabling method chaining.
        /// </returns>
        /// <exception cref="AgenixSystemException">
        ///     Thrown when the provided expression is not a valid JSON path expression.
        /// </exception>
        public Builder JsonPath(string expression, object value)
        {
            if (!JsonPathMessageValidationContext.IsJsonPathExpression(expression))
            {
                throw new AgenixSystemException($"Unsupported json path expression '{expression}'");
            }

            return Expression(expression, value);
        }

        /// Adds an XPath expression to the validation context.
        /// This method checks whether the provided expression is a valid XPath expression.
        /// If the expression is invalid, a CoreSystemException is thrown.
        /// <param name="expression">The XPath expression to be validated and added.</param>
        /// <param name="value">The associated value to map to the given expression.</param>
        /// <returns>Returns the current instance of the builder for method chaining.</returns>
        /// <exception cref="AgenixSystemException">Thrown when the given expression is not a valid XPath expression.</exception>
        public Builder Xpath(string expression, object value)
        {
            if (!XpathMessageValidationContext.IsXpathExpression(expression))
            {
                throw new AgenixSystemException($"Unsupported xpath expression '{expression}'");
            }

            return Expression(expression, value);
        }
    }
}
