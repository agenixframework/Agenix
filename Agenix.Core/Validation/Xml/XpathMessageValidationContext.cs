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
using Agenix.Api.Builder;
using Agenix.Api.Message;
using Agenix.Api.Util;
using Agenix.Api.Validation.Context;
using Agenix.Api.Variable;
using Agenix.Core.Message;

namespace Agenix.Core.Validation.Xml;

/// Represents a specialized XML validation context that incorporates XPath expression evaluation.
/// /
public class XpathMessageValidationContext : XmlMessageValidationContext, IValidationContext
{
    /// Stores a collection of XPath expressions mapped to their corresponding expected values.
    private readonly IDictionary<string, object> _xPathExpressions;

    /// Represents a specialized XML validation context that incorporates XPath expression evaluation.
    /// /
    public XpathMessageValidationContext() : this(new Builder())
    {
    }

    /// Represents an XML validation context combined with support for XPath expression evaluation.
    /// This class allows validation of XML messages while incorporating XPath-based logic.
    /// It extends the functionality of XmlMessageValidationContext by adding XPath-specific processing capabilities.
    public XpathMessageValidationContext(Builder builder) : base(new XmlMessageValidationContext.Builder()
        .NamespaceContext(builder._namespaces)
        .Namespaces(builder.ControlNamespaces)
        .SchemaRepository(builder._schemaRepository)
        .SchemaValidation(builder._schemaValidation)
        .Schema(builder._schema)
        .Ignore(builder.IgnoreExpressions))
    {
        _xPathExpressions = builder.AllExpressions;
    }

    /// Dictionary containing XPath expressions as keys and their associated expected values as values.
    /// /
    public IDictionary<string, object> XpathExpressions => _xPathExpressions;

    /// Indicates whether a validation context requires a validator for processing.
    /// This property overrides the default behavior defined in the IValidationContext interface,
    /// explicitly returning true to signify that the context necessitates validation.
    bool IValidationContext.RequiresValidator => true;

    /// <summary>
    ///     Determines whether the given path expression is an XPath expression.
    /// </summary>
    /// <param name="pathExpression">The path expression to evaluate.</param>
    /// <returns>Returns true if the given path expression is an XPath expression, otherwise false.</returns>
    public static bool IsXpathExpression(string pathExpression)
    {
        return StringUtils.HasText(pathExpression) && pathExpression.StartsWith('/');
    }

    /// Provides a fluent builder for configuring and constructing instances of XpathMessageValidationContext.
    /// Extends the XmlValidationContextBuilder to include support for XPath expressions and relevant adapters.
    /// /
    public new sealed class Builder : XmlValidationContextBuilder<XpathMessageValidationContext, Builder>,
        IWithExpressions<Builder>, IVariableExtractorAdapter, IMessageProcessorAdapter
    {
        private readonly Dictionary<string, object> _expressions = new();

        public IDictionary<string, object> AllExpressions => _expressions;

        /// Converts the current context into an IMessageProcessor implementation by incorporating the defined XPath expressions.
        /// This allows for message processing based on path expression evaluations.
        /// <returns>An instance of IMessageProcessor configured with the current XPath expressions.</returns>
        public IMessageProcessor AsProcessor()
        {
            return new DelegatingPathExpressionProcessor.Builder()
                .Expressions(_expressions)
                .Build();
        }

        /// Creates an instance of `IVariableExtractor` using the configured expressions within the builder.
        /// <return>An `IVariableExtractor` instance initialized with the current set of expressions.</return>
        public IVariableExtractor AsExtractor()
        {
            return new DelegatingPayloadVariableExtractor.Builder()
                .Expressions(_expressions)
                .Build();
        }

        /// Sets the expressions for the current context builder.
        /// <param name="expressions">
        ///     A dictionary containing key-value pairs of expressions to be associated with the context
        ///     builder.
        /// </param>
        /// <returns>The updated instance of the builder with the specified expressions set.</returns>
        public Builder Expressions(IDictionary<string, object> expressions)
        {
            foreach (var expression in expressions)
            {
                _expressions[expression.Key] = expression.Value;
            }

            return this;
        }

        /// Represents a construct for handling or evaluating expressions within specific validation contexts.
        /// Provides a mechanism to pair a given expression with a corresponding value.
        /// <param name="expression">The expression to be evaluated, specified as a string.</param>
        /// <param name="value">The value associated with the expression, used for validation or evaluation purposes.</param>
        /// <returns>An instance of the builder with the specified expression and value included in its context.</returns>
        public Builder Expression(string expression, object value)
        {
            _expressions[expression] = value;
            return this;
        }

        /// Fluent builder for creating an instance of the XpathMessageValidationContext.
        /// @return A new instance of the Builder class.
        /// /
        public static Builder Xpath()
        {
            return new Builder();
        }

        public override XpathMessageValidationContext Build()
        {
            return new XpathMessageValidationContext(this);
        }
    }
}
