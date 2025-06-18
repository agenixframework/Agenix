#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
