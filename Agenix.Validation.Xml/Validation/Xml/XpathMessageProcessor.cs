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
using Agenix.Api.Builder;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Util;
using Agenix.Validation.Xml.Util;
using Agenix.Validation.Xml.Xpath;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Xml.Validation.Xml;

/// <summary>
///     Processor implementation evaluating XPath expressions on message payload during message construction.
///     Class identifies XML elements inside the message payload via XPath expressions to overwrite their value.
/// </summary>
public class XpathMessageProcessor : AbstractMessageProcessor
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(XpathMessageProcessor));

    /// <summary>
    ///     Default constructor.
    /// </summary>
    public XpathMessageProcessor() : this(new Builder())
    {
    }

    /// <summary>
    ///     Constructor using fluent builder.
    /// </summary>
    /// <param name="builder">The builder instance</param>
    private XpathMessageProcessor(Builder builder)
    {
        XPathExpressions = new Dictionary<string, object>(builder.expressions);
    }

    /// <summary>
    ///     Overwrites message elements before validating (via XPath expressions)
    /// </summary>
    public Dictionary<string, object> XPathExpressions { get; }

    /// <summary>
    ///     Intercept the message payload construction and replace elements identified
    ///     via XPath expressions.
    ///     Method parses the message payload to DOM document representation; therefore, the message payload
    ///     needs to be XML here.
    /// </summary>
    /// <param name="message">The message to process</param>
    /// <param name="context">The test context</param>
    public override void ProcessMessage(IMessage message, TestContext context)
    {
        if (message.Payload == null || !StringUtils.HasText(message.GetPayload<string>()))
        {
            return;
        }

        var doc = XmlUtils.ParseMessagePayload(message.GetPayload<string>());

        if (doc == null)
        {
            throw new AgenixSystemException("Not able to set message elements, because no XML resource defined");
        }

        foreach (var entry in XPathExpressions)
        {
            var pathExpression = entry.Key;
            var valueExpression = entry.Value?.ToString() ?? string.Empty;

            // Check if value expr is variable or function (and resolve it if yes)
            valueExpression = context.ReplaceDynamicContentInString(valueExpression);

            XmlNode node;
            if (XpathUtils.IsXPathExpression(pathExpression))
            {
                node = XpathUtils.EvaluateAsNode(doc, pathExpression,
                    context.NamespaceContextBuilder.BuildContext(message, new Dictionary<string, string>()));
            }
            else
            {
                node = XmlUtils.FindNodeByName(doc, pathExpression);
            }

            if (node == null)
            {
                throw new AgenixSystemException($"Could not find element for expression {pathExpression}");
            }

            if (node.NodeType == XmlNodeType.Element)
            {
                // Fix: otherwise there will be a new line in the output
                node.InnerText = valueExpression;
            }
            else
            {
                node.Value = valueExpression;
            }

            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug("Element {PathExpression} was set to value: {ValueExpression}",
                    pathExpression, valueExpression);
            }
        }

        message.Payload = XmlUtils.Serialize(doc);
    }


    /// <summary>
    ///     Determines whether the processor supports the specified message type.
    /// </summary>
    /// <param name="messageType">
    ///     The message type to be evaluated against the supported message types.
    /// </param>
    /// <returns>
    ///     True if the specified message type is supported by the processor; otherwise, false.
    /// </returns>
    public override bool SupportsMessageType(string messageType)
    {
        return nameof(MessageType.XML).Equals(messageType, StringComparison.OrdinalIgnoreCase) ||
               nameof(MessageType.XHTML).Equals(messageType, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Fluent builder.
    /// </summary>
    public sealed class Builder : IMessageProcessor.IBuilder<XpathMessageProcessor, Builder>, IWithExpressions<Builder>
    {
        internal readonly Dictionary<string, object> expressions = new();

        public XpathMessageProcessor Build()
        {
            return new XpathMessageProcessor(this);
        }

        public Builder Expressions(IDictionary<string, object> newExpressions)
        {
            foreach (var kvp in newExpressions)
            {
                expressions[kvp.Key] = kvp.Value;
            }

            return this;
        }

        public Builder Expression(string expression, object expectedValue)
        {
            expressions[expression] = expectedValue;
            return this;
        }
    }
}
