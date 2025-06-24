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
using System.Xml.XPath;
using Agenix.Api.Common;
using Agenix.Api.Context;
using Agenix.Api.Log;
using Agenix.Api.Variable.Dictionary;
using Agenix.Api.Xml.Namespace;
using Agenix.Validation.Xml.Util;
using Agenix.Validation.Xml.Xpath;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Xml.Variable.Dictionary.Xml;

/// <summary>
///     XML data dictionary implementation maps elements via XPath expressions. When an element is identified by some
///     expression
///     in the dictionary, the value is overwritten accordingly. Namespace context is either evaluated on the fly or by
///     global namespace
///     context builder.
/// </summary>
public class XpathMappingDataDictionary : AbstractXmlDataDictionary, InitializingPhase
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(NodeMappingDataDictionary));

    private NamespaceContextBuilder? _namespaceContextBuilder;

    /// <summary>
    ///     Gets or sets the namespace context builder.
    /// </summary>
    public NamespaceContextBuilder NamespaceContextBuilder
    {
        get => _namespaceContextBuilder;
        set => _namespaceContextBuilder = value;
    }

    public new void Initialize()
    {
        if (PathMappingStrategy != null &&
            PathMappingStrategy != PathMappingStrategy.EXACT)
        {
            Log.LogWarning("{ClassName} ignores path mapping strategy other than {Strategy}",
                GetType().Name, PathMappingStrategy.EXACT);
        }

        base.Initialize();
    }

    public override T Translate<T>(XmlNode node, T value, TestContext context)
    {
        foreach (var expressionEntry in Mappings)
        {
            var expression = expressionEntry.Key;

            if (XpathUtils.EvaluateExpression(node.OwnerDocument, expression,
                    BuildNamespaceContext(node, context), XPathResultType.NodeSet) is XPathNodeIterator findings &&
                ContainsNode(findings, node))
            {
                if (Log.IsEnabled(LogLevel.Debug))
                {
                    Log.LogDebug("Data dictionary setting element '{NodePath}' value: {Value}",
                        XmlUtils.GetNodesPathName(node), expressionEntry.Value);
                }

                return ConvertIfNecessary(expressionEntry.Value, value, context);
            }
        }

        return value;
    }

    /// <summary>
    ///     Checks if given node set contains node.
    /// </summary>
    /// <param name="findings">The XmlNodeList to search in</param>
    /// <param name="node">The node to find</param>
    /// <returns>True if the node is found in the list</returns>
    private static bool ContainsNode(XPathNodeIterator findings, XmlNode node)
    {
        var targetNavigator = node.CreateNavigator();

        foreach (XPathNavigator navigator in findings)
        {
            if (navigator.IsSamePosition(targetNavigator))
            {
                return true;
            }
        }

        return false;
    }


    /// <summary>
    ///     Builds namespace context with dynamic lookup on received node document and global namespace mappings from
    ///     namespace context builder.
    /// </summary>
    /// <param name="node">The element node from message</param>
    /// <param name="context">The current test context</param>
    /// <returns>The namespace context</returns>
    private IXmlNamespaceResolver BuildNamespaceContext(XmlNode node, TestContext context)
    {
        var simpleNamespaceContext = new DefaultNamespaceContext();
        var namespaces = XmlUtils.LookupNamespaces(node.OwnerDocument);

        // Add default namespace mappings
        var builderNamespaces = GetNamespaceContextBuilder(context).NamespaceMappings;
        foreach (var ns in builderNamespaces)
        {
            namespaces[ns.Key] = ns.Value;
        }

        simpleNamespaceContext.AddNamespaces(namespaces);

        return simpleNamespaceContext;
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
}
