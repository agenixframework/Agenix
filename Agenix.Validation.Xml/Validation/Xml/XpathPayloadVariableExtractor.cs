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

using System.Xml;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Api.Validation.Xml;
using Agenix.Api.Variable;
using Agenix.Core.Message;
using Agenix.Core.Validation;
using Agenix.Validation.Xml.Util;
using Agenix.Validation.Xml.Xpath;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Xml.Validation.Xml;

/// <summary>
///     Provides functionality for extracting variables from an XML message payload using XPath expressions.
///     Implements the <see cref="IVariableExtractor" /> interface.
/// </summary>
public class XpathPayloadVariableExtractor : IVariableExtractor
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(XpathPayloadVariableExtractor));

    /// <summary>
    ///     Namespace definitions used in xpath expressions
    /// </summary>
    private readonly Dictionary<string, string> _namespaces;

    /// <summary>
    ///     Dictionary defines xpath expressions and target variable names
    /// </summary>
    private readonly Dictionary<string, object>? _xPathExpressions;

    public XpathPayloadVariableExtractor()
        : this(new Builder())
    {
    }

    /// <summary>
    ///     Constructor using fluent builder.
    /// </summary>
    /// <param name="builder">The builder instance</param>
    private XpathPayloadVariableExtractor(Builder builder)
    {
        _xPathExpressions = builder.expressions;
        _namespaces = builder.namespaces;
    }

    /// <summary>
    ///     Dictionary of XPath expressions and corresponding target variable names.
    /// </summary>
    public Dictionary<string, object>? Expressions => _xPathExpressions;

    /// <summary>
    ///     Namespace definitions used in xpath expressions.
    /// </summary>
    public Dictionary<string, string> Namespaces => _namespaces;

    /// <summary>
    ///     Processes a message and test context to extract variables.
    /// </summary>
    /// <param name="message">The message to be processed.</param>
    /// <param name="context">The test context providing additional information for processing.</param>
    public void Process(IMessage message, TestContext context)
    {
        ExtractVariables(message, context);
    }

    /// <summary>
    ///     Extract variables using Xpath expressions.
    /// </summary>
    public void ExtractVariables(IMessage message, TestContext context)
    {
        if (_xPathExpressions == null || _xPathExpressions.Count == 0)
        {
            return;
        }

        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug("Reading XML elements with XPath");
        }

        var nsContext = XmlValidationHelper
            .GetNamespaceContextBuilder(context)
            .BuildContext(message, _namespaces);

        foreach (var entry in _xPathExpressions)
        {
            var pathExpression = context.ReplaceDynamicContentInString(entry.Key);
            var variableName = entry.Value?.ToString()
                               ?? throw new AgenixSystemException(
                                   $"Variable name must be set on extractor path expression '{pathExpression}'");

            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug("Evaluating XPath expression: {PathExpression}", pathExpression);
            }

            var doc = XmlUtils.ParseMessagePayload(message.GetPayload<string>());

            if (XpathUtils.IsXPathExpression(pathExpression))
            {
                var resultType =
                    XPathExpressionResultExtensions.FromString(pathExpression, XPathExpressionResult.String);

                var value = XpathUtils.Evaluate(doc, XPathExpressionResultExtensions.CutOffPrefix(pathExpression),
                    nsContext, resultType);

                switch (value)
                {
                    case null:
                        throw new AgenixSystemException(
                            $"Not able to find value for expression: {XPathExpressionResultExtensions.CutOffPrefix(pathExpression)}");
                    case IEnumerable<object> list:
                        value = string.Join(",", list.Select(x => x.ToString()));
                        break;
                }

                context.SetVariable(variableName, value);
            }
            else
            {
                var node = XmlUtils.FindNodeByName(doc, pathExpression);

                if (node == null)
                {
                    throw new AgenixSystemException(
                        $"No element found for expression: {XPathExpressionResultExtensions.CutOffPrefix(pathExpression)}");
                }

                if (node.NodeType == XmlNodeType.Element)
                {
                    context.SetVariable(variableName, node.FirstChild != null ? node.FirstChild.Value : string.Empty);
                }
                else
                {
                    context.SetVariable(variableName, node.Value);
                }
            }
        }
    }

    /// <summary>
    ///     Fluent builder.
    /// </summary>
    public sealed class Builder : IVariableExtractor.IBuilder<XpathPayloadVariableExtractor, Builder>,
        IXmlNamespaceAware, IMessageProcessorAdapter, IValidationContextAdapter
    {
        internal readonly Dictionary<string, object> expressions = new();
        internal readonly Dictionary<string, string> namespaces = new();

        /// <summary>
        ///     Initializes a new instance of the Builder class with a collection of expressions.
        /// </summary>
        /// <param name="newExpressions">A dictionary containing key-value pairs of expressions to be added to the builder.</param>
        /// <returns>Returns the Builder instance.</returns>
        public Builder Expressions(IDictionary<string, object> newExpressions)
        {
            foreach (var expression in newExpressions)
            {
                expressions[expression.Key] = expression.Value;
            }

            return this;
        }

        /// <summary>
        ///     Associates an XPath expression with a variable name for extracting payload data.
        /// </summary>
        /// <param name="expression">The XPath expression used to extract data.</param>
        /// <param name="variableName">The name of the variable the extracted data will be assigned to.</param>
        /// <returns>The current instance of the builder.</returns>
        public Builder Expression(string expression, object variableName)
        {
            expressions[expression] = variableName;
            return this;
        }

        public XpathPayloadVariableExtractor Build()
        {
            return new XpathPayloadVariableExtractor(this);
        }

        /// <summary>
        ///     Builds and returns an <see cref="IMessageProcessor" /> instance configured with the required expressions.
        /// </summary>
        /// <returns>An instance of <see cref="IMessageProcessor" /> initialized with the specified configurations.</returns>
        public IMessageProcessor AsProcessor()
        {
            return new DelegatingPathExpressionProcessor.Builder()
                .Expressions(expressions)
                .Build();
        }

        /// <summary>
        ///     Converts the builder into a validation context for use in validation workflows.
        /// </summary>
        /// <returns>The validation context instance derived from the current builder configuration.</returns>
        public IValidationContext AsValidationContext()
        {
            return new PathExpressionValidationContext.Builder()
                .Expressions(expressions)
                .Build();
        }

        /// <summary>
        ///     Sets the namespaces to be used in XML processing.
        /// </summary>
        /// <param name="newNamespaces">A dictionary containing namespace prefixes and their corresponding URIs.</param>
        public void SetNamespaces(IDictionary<string, string> newNamespaces)
        {
            Namespaces(namespaces);
        }

        /// <summary>
        ///     Creates a new instance of the Builder using XPath configuration.
        /// </summary>
        /// <returns>A new Builder instance.</returns>
        public static Builder FromXpath()
        {
            return new Builder();
        }

        /// <summary>
        ///     Adds explicit namespace declaration for later path validation expressions.
        /// </summary>
        /// <param name="prefix">The namespace prefix</param>
        /// <param name="namespaceUri">The namespace URI</param>
        /// <returns>This builder instance</returns>
        public Builder Namespace(string prefix, string namespaceUri)
        {
            namespaces[prefix] = namespaceUri;
            return this;
        }

        /// <summary>
        ///     Sets default namespace declarations on this action builder.
        /// </summary>
        /// <param name="namespaceMappings">Dictionary of namespace mappings</param>
        /// <returns>This builder instance</returns>
        public Builder Namespaces(Dictionary<string, string> namespaceMappings)
        {
            foreach (var mapping in namespaceMappings)
            {
                namespaces[mapping.Key] = mapping.Value;
            }

            return this;
        }
    }
}
