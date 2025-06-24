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
using Agenix.Api.Xml.Namespace;
using Agenix.Core.Validation;
using Agenix.Core.Validation.Xml;
using Agenix.Validation.Xml.Util;
using Agenix.Validation.Xml.Xpath;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Xml.Validation.Xml;

/// <summary>
///     Message validator evaluates set of XPath expressions on message payload and checks that values are as expected.
/// </summary>
/// <since>2.3</since>
public class XpathMessageValidator : AbstractMessageValidator<XpathMessageValidationContext>,
    IMessageValidator<IValidationContext>
{
    /// <summary>
    ///     Logger
    /// </summary>
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(XpathMessageValidator));

    private NamespaceContextBuilder? _namespaceContextBuilder;

    /// <summary>
    ///     Determines whether the validator supports the given message type.
    /// </summary>
    /// <param name="messageType">The type of the message being validated.</param>
    /// <param name="message">The message instance to validate.</param>
    /// <returns>True if the message type is supported; otherwise, false.</returns>
    public override bool SupportsMessageType(string messageType, IMessage message)
    {
        return new DomXmlMessageValidator().SupportsMessageType(messageType, message);
    }

    public override void ValidateMessage(IMessage receivedMessage, IMessage controlMessage,
        TestContext context, XpathMessageValidationContext validationContext)
    {
        if (validationContext.XpathExpressions.Count == 0)
        {
            return;
        }

        var payloadString = receivedMessage.GetPayload<string>();
        if (string.IsNullOrWhiteSpace(payloadString))
        {
            throw new ValidationException("Unable to validate message elements - receive message payload was empty");
        }

        Logger.LogDebug("Start XPath element validation ...");

        var received = XmlUtils.ParseMessagePayload(payloadString);
        var namespaceContext = GetNamespaceContextBuilder(context)
            .BuildContext(receivedMessage, validationContext.Namespaces);

        foreach (var entry in validationContext.XpathExpressions)
        {
            var xPathExpression = entry.Key;
            var expectedValue = entry.Value;

            xPathExpression = context.ReplaceDynamicContentInString(xPathExpression);

            object xPathResult;
            if (XpathUtils.IsXPathExpression(xPathExpression))
            {
                var resultType =
                    XPathExpressionResultExtensions.FromString(xPathExpression, XPathExpressionResult.Node);

                xPathExpression = XPathExpressionResultExtensions.CutOffPrefix(xPathExpression);

                // Give ignore elements the chance to prevent the validation in case a result type is node
                if (resultType.Equals(XPathExpressionResult.Node) &&
                    XmlValidationUtils.IsElementIgnored(
                        XpathUtils.EvaluateAsNode(received, xPathExpression, namespaceContext),
                        validationContext.IgnoreExpressions,
                        namespaceContext))
                {
                    continue;
                }

                xPathResult = XpathUtils.Evaluate(received,
                    xPathExpression,
                    namespaceContext,
                    resultType);
            }
            else
            {
                var node = XmlUtils.FindNodeByName(received, xPathExpression);

                if (node == null)
                {
                    throw new AgenixSystemException(
                        $"Element '{xPathExpression}' could not be found in DOM tree");
                }

                if (XmlValidationUtils.IsElementIgnored(node, validationContext.IgnoreExpressions, namespaceContext))
                {
                    continue;
                }

                xPathResult = GetNodeValue(node);
            }

            if (expectedValue is string)
            {
                // Check if the expected value is variable or function (and resolve it, if yes)
                expectedValue = context.ReplaceDynamicContentInString(expectedValue.ToString()!);
            }

            // Do the validation of actual and expected value for an element
            ValidationUtils.ValidateValues(xPathResult, expectedValue, xPathExpression, context);

            Logger.LogDebug("Validating element: {XPathExpression}='{ExpectedValue}': OK", xPathExpression,
                expectedValue);
        }

        Logger.LogDebug("XPath element validation successful: All elements OK");
    }

    /// <summary>
    ///     Retrieves the required validation context type for the XPath message validator.
    /// </summary>
    /// <returns>The Type representing XpathMessageValidationContext.</returns>
    protected override Type GetRequiredValidationContextType()
    {
        return typeof(XpathMessageValidationContext);
    }

    /// <summary>
    ///     Finds and returns a single merged XpathMessageValidationContext from a list of validation contexts.
    /// </summary>
    /// <param name="validationContexts">The list of validation contexts to search and process.</param>
    /// <returns>The merged XpathMessageValidationContext if available; otherwise, the result of the base implementation.</returns>
    public override XpathMessageValidationContext? FindValidationContext(List<IValidationContext> validationContexts)
    {
        var xpathMessageValidationContexts = validationContexts
            .OfType<XpathMessageValidationContext>()
            .ToList();

        if (xpathMessageValidationContexts.Count > 0)
        {
            var xpathMessageValidationContext = xpathMessageValidationContexts[0];

            // Collect all xpath expressions and combine into one single validation context
            var xpathExpressions = xpathMessageValidationContexts
                .Select(ctx => ctx.XpathExpressions)
                .Aggregate(new Dictionary<string, object>(), (collect, map) =>
                {
                    foreach (var kvp in map)
                    {
                        collect[kvp.Key] = kvp.Value;
                    }

                    return collect;
                });

            if (xpathExpressions.Count > 0)
            {
                foreach (var kvp in xpathExpressions)
                {
                    xpathMessageValidationContext.XpathExpressions[kvp.Key] = kvp.Value;
                }

                // Update the status of other validation contexts to optional as validation is performed by this single context
                foreach (var vc in xpathMessageValidationContexts.Where(vc => vc != xpathMessageValidationContext))
                {
                    vc.UpdateStatus(ValidationStatus.OPTIONAL);
                }

                return xpathMessageValidationContext;
            }
        }

        return base.FindValidationContext(validationContexts);
    }

    /// <summary>
    ///     Resolves an XML node's value
    /// </summary>
    /// <param name="node">The XML node</param>
    /// <returns>Node's string value</returns>
    private static string? GetNodeValue(XmlNode node)
    {
        if (node.NodeType == XmlNodeType.Element && node.FirstChild != null)
        {
            return node.FirstChild.Value;
        }

        return node.Value;
    }

    /// <summary>
    ///     Get explicit namespace context builder set on this class or obtain instance from reference resolver.
    /// </summary>
    /// <param name="context">The test context</param>
    /// <returns>The namespace context builder</returns>
    private NamespaceContextBuilder GetNamespaceContextBuilder(TestContext context)
    {
        if (_namespaceContextBuilder != null)
        {
            return _namespaceContextBuilder;
        }

        return XmlValidationHelper.GetNamespaceContextBuilder(context);
    }

    /// <summary>
    ///     Sets the namespace context builder.
    /// </summary>
    /// <param name="namespaceContextBuilder">The namespace context builder</param>
    public void SetNamespaceContextBuilder(NamespaceContextBuilder namespaceContextBuilder)
    {
        _namespaceContextBuilder = namespaceContextBuilder;
    }
}
