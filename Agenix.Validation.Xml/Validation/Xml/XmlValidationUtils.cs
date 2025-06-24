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
using Agenix.Api;
using Agenix.Api.Log;
using Agenix.Api.Util;
using Agenix.Validation.Xml.Util;
using Agenix.Validation.Xml.Xpath;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Xml.Validation.Xml;

/// <summary>
///     Abstract utility class for XML validation operations.
/// </summary>
public abstract class XmlValidationUtils
{
    /// <summary>
    ///     Logger
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(XmlValidationUtils));

    /// <summary>
    ///     Prevent instantiation.
    /// </summary>
    private XmlValidationUtils()
    {
    }

    /// <summary>
    ///     Checks if given element node is either on ignore list or
    ///     contains @ignore@ tag inside control message
    /// </summary>
    /// <param name="source">The source node</param>
    /// <param name="received">The received node</param>
    /// <param name="ignoreExpressions">Set of ignore expressions</param>
    /// <param name="namespaceContext">Namespace context</param>
    /// <returns>True if element should be ignored</returns>
    public static bool IsElementIgnored(XmlNode source, XmlNode received,
        HashSet<string> ignoreExpressions, IXmlNamespaceResolver namespaceContext)
    {
        if (IsElementIgnored(received, ignoreExpressions, namespaceContext))
        {
            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug("Element: '{LocalName}' is on ignore list - skipped validation", received.LocalName);
            }

            return true;
        }

        if (source.FirstChild != null &&
            StringUtils.HasText(source.FirstChild.Value) &&
            source.FirstChild.Value.Trim().Equals(AgenixSettings.IgnorePlaceholder))
        {
            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug("Element: '{LocalName}' is ignored by placeholder '{IgnorePlaceholder}'",
                    received.LocalName, AgenixSettings.IgnorePlaceholder);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Checks whether the node is ignored by node path expression or xpath expression.
    /// </summary>
    /// <param name="received">The received node</param>
    /// <param name="ignoreExpressions">Set of ignore expressions</param>
    /// <param name="namespaceContext">Namespace context</param>
    /// <returns>True if element should be ignored</returns>
    public static bool IsElementIgnored(XmlNode received, HashSet<string> ignoreExpressions,
        IXmlNamespaceResolver namespaceContext)
    {
        if (ignoreExpressions == null || !ignoreExpressions.Any())
        {
            return false;
        }

        // This is the faster version, but then the ignoreValue name must be
        // the full path name like: Numbers.NumberItem.AreaCode
        if (ignoreExpressions.Contains(XmlUtils.GetNodesPathName(received)))
        {
            return true;
        }

        // This is the slower version, but here the ignoreValues can be
        // the short path name like only: AreaCode
        //
        // If there are more nodes with the same short name,
        // the first one will match, eg. if there are:
        //      Numbers1.NumberItem.AreaCode
        //      Numbers2.NumberItem.AreaCode
        // And ignoreValues contains just: AreaCode
        // the only first Node: Numbers1.NumberItem.AreaCode will be ignored.
        foreach (var expression in ignoreExpressions)
        {
            if (received.Equals(XmlUtils.FindNodeByName(received.OwnerDocument, expression)))
            {
                return true;
            }
        }

        // This is the XPath version using XPath expressions in
        // ignoreValues to identify nodes to be ignored
        foreach (var expression in ignoreExpressions)
        {
            if (XpathUtils.IsXPathExpression(expression))
            {
                var foundNodes = XpathUtils.EvaluateAsNodeList(received.OwnerDocument,
                    expression, namespaceContext);

                if (foundNodes != null)
                {
                    for (var i = 0; i < foundNodes.Count; i++)
                    {
                        if (foundNodes[i] != null && foundNodes[i].Equals(received))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    ///     Checks whether the current attribute is ignored either by global ignore placeholder in source attribute value or
    ///     by xpath ignore expressions.
    /// </summary>
    /// <param name="receivedElement">The received element</param>
    /// <param name="receivedAttribute">The received attribute</param>
    /// <param name="sourceAttribute">The source attribute</param>
    /// <param name="ignoreMessageElements">Set of ignore expressions</param>
    /// <param name="namespaceContext">Namespace context</param>
    /// <returns>True if attribute should be ignored</returns>
    public static bool IsAttributeIgnored(XmlNode receivedElement, XmlNode receivedAttribute,
        XmlNode sourceAttribute, HashSet<string> ignoreMessageElements, IXmlNamespaceResolver namespaceContext)
    {
        if (IsAttributeIgnored(receivedElement, receivedAttribute, ignoreMessageElements, namespaceContext))
        {
            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug("Attribute '{LocalName}' is on ignore list - skipped value validation",
                    receivedAttribute.LocalName);
            }

            return true;
        }

        if (StringUtils.HasText(sourceAttribute.Value) &&
            sourceAttribute.Value.Trim().Equals(AgenixSettings.IgnorePlaceholder))
        {
            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug("Attribute: '{LocalName}' is ignored by placeholder '{IgnorePlaceholder}'",
                    receivedAttribute.LocalName, AgenixSettings.IgnorePlaceholder);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Checks whether the current attribute is ignored.
    /// </summary>
    /// <param name="receivedElement">The received element</param>
    /// <param name="receivedAttribute">The received attribute</param>
    /// <param name="ignoreMessageElements">Set of ignore expressions</param>
    /// <param name="namespaceContext">Namespace context</param>
    /// <returns>True if attribute should be ignored</returns>
    private static bool IsAttributeIgnored(XmlNode receivedElement, XmlNode receivedAttribute,
        HashSet<string> ignoreMessageElements, IXmlNamespaceResolver namespaceContext)
    {
        if (ignoreMessageElements == null || !ignoreMessageElements.Any())
        {
            return false;
        }

        // This is the faster version, but then the ignoreValue name must be
        // the full path name like: Numbers.NumberItem.AreaCode
        if (ignoreMessageElements.Contains(XmlUtils.GetNodesPathName(receivedElement) + "." + receivedAttribute.Name))
        {
            return true;
        }

        // This is the slower version, but here the ignoreValues can be
        // the short path name like only: AreaCode
        //
        // If there are more nodes with the same short name,
        // the first one will match, eg. if there are:
        //      Numbers1.NumberItem.AreaCode
        //      Numbers2.NumberItem.AreaCode
        // And ignoreValues contains just: AreaCode
        // the only first Node: Numbers1.NumberItem.AreaCode will be ignored.
        return ignoreMessageElements
                   .Select(expression => XmlUtils.FindNodeByName(receivedElement.OwnerDocument, expression))
                   .OfType<XmlNode>().Contains(receivedAttribute) ||
               // This is the XPath version using XPath expressions in
               // ignoreValues to identify nodes to be ignored
               (from expression in ignoreMessageElements
                where XpathUtils.IsXPathExpression(expression)
                select XpathUtils.EvaluateAsNode(receivedElement.OwnerDocument, expression, namespaceContext))
               .OfType<XmlNode>().Contains(receivedAttribute);
    }
}
