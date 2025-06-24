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

using System.Globalization;
using System.Xml;
using System.Xml.XPath;
using Agenix.Api.Context;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Core.Message.Selector;
using Agenix.Validation.Xml.Util;
using Agenix.Validation.Xml.Xpath;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Xml.Message.Selector;

/// <summary>
///     Message selector accepts XML messages in case XPath expression evaluation result matches
///     the expected value. With this selector someone can select messages according to a message payload XML
///     element value for instance.
///     Syntax is xpath://root/element
/// </summary>
public class XpathPayloadMessageSelector : AbstractMessageSelector
{
    /// <summary>
    ///     Special selector element name identifying this message selector implementation
    /// </summary>
    public const string SelectorPrefix = "xpath:";

    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(XpathPayloadMessageSelector));

    /// <summary>
    ///     Default constructor using fields.
    /// </summary>
    /// <param name="selectKey">The selector key</param>
    /// <param name="matchingValue">The expected matching value</param>
    /// <param name="context">The test context</param>
    public XpathPayloadMessageSelector(string selectKey, string matchingValue, TestContext context)
        : base(selectKey[SelectorPrefix.Length..], matchingValue, context)
    {
    }

    public override bool Accept(IMessage message)
    {
        XmlDocument doc;

        try
        {
            doc = XmlUtils.ParseMessagePayload(GetPayloadAsString(message));
        }
        catch (XmlException e)
        {
            Log.LogWarning("Ignoring non XML message for XPath message selector ({ExceptionType})", e.GetType().Name);
            return false; // non XML message - not accepted
        }

        try
        {
            var namespaces = XmlUtils.LookupNamespaces(doc);

            // add default namespace mappings
            foreach (var ns in Context.NamespaceContextBuilder.NamespaceMappings)
            {
                namespaces[ns.Key] = ns.Value;
            }

            string value;
            if (XpathUtils.HasDynamicNamespaces(SelectKey))
            {
                var dynamicNamespaces = XpathUtils.GetDynamicNamespaces(SelectKey);
                foreach (var ns in dynamicNamespaces)
                {
                    namespaces[ns.Key] = ns.Value;
                }

                value = EvaluateAsString(
                    XPathExpressionFactory.CreateXPathExpression(
                        XpathUtils.ReplaceDynamicNamespaces(SelectKey, namespaces),
                        namespaces),
                    doc);
            }
            else
            {
                value = EvaluateAsString(
                    XPathExpressionFactory.CreateXPathExpression(SelectKey, namespaces),
                    doc);
            }

            return Evaluate(value);
        }
        catch (XPathException e)
        {
            Log.LogWarning(
                "Could not evaluate XPath expression for message selector - ignoring message ({ExceptionType})",
                e.GetType().Name);
            return false; // wrong XML message - not accepted
        }
    }

    /// <summary>
    ///     Evaluates the XPath expression and returns the result as a string.
    /// </summary>
    /// <param name="expression">The XPath expression</param>
    /// <param name="document">The XML document to evaluate against</param>
    /// <returns>The evaluation result as string</returns>
    private string EvaluateAsString(XPathExpression expression, XmlDocument document)
    {
        var navigator = document.CreateNavigator();
        var result = navigator.Evaluate(expression);

        return result switch
        {
            XPathNodeIterator iterator => iterator.MoveNext() ? iterator.Current?.Value ?? string.Empty : string.Empty,
            string str => str,
            double number => number.ToString(CultureInfo.InvariantCulture),
            bool boolean => boolean.ToString().ToLower(),
            _ => result?.ToString() ?? string.Empty
        };
    }


    /// <summary>
    ///     Message selector factory for this implementation.
    /// </summary>
    public class Factory : IMessageSelector.IMessageSelectorFactory
    {
        public bool Supports(string key)
        {
            return key.StartsWith(SelectorPrefix);
        }

        IMessageSelector IMessageSelector.IMessageSelectorFactory.Create(string key, string value, TestContext context)
        {
            return Create(key, value, context);
        }

        public XpathPayloadMessageSelector Create(string key, string value, TestContext context)
        {
            return new XpathPayloadMessageSelector(key, value, context);
        }
    }
}
