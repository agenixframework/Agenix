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

using System.Text.RegularExpressions;
using System.Xml;
using Agenix.Api.Message;

namespace Agenix.Api.Xml.Namespace;

/// <summary>
///     Builds a namespace context for XPath expression evaluations. Builder supports default mappings
///     as well as dynamic mappings from a received message.
///     Namespace mappings are defined as key value pairs where key is defined as a namespace prefix and value is the
///     actual namespace uri.
/// </summary>
public class NamespaceContextBuilder
{
    /// <summary>The default bean id in DI container</summary>
    public const string DefaultBeanId = "namespaceContextBuilder";

    /// <summary>
    ///     Default namespace mappings for all tests
    /// </summary>
    public Dictionary<string, string> NamespaceMappings { get; set; } = new();

    /// <summary>
    ///     Construct a basic namespace context from the received message and explicit namespace mappings.
    /// </summary>
    /// <param name="receivedMessage">the actual message received.</param>
    /// <param name="namespaces">explicit namespace mappings for this construction.</param>
    /// <returns>the constructed namespace context.</returns>
    public DefaultNamespaceContext BuildContext(IMessage receivedMessage, Dictionary<string, string>? namespaces = null)
    {
        var defaultNamespaceContext = new DefaultNamespaceContext();

        // First add default namespace definitions
        if (NamespaceMappings.Count != 0)
        {
            defaultNamespaceContext.AddNamespaces(NamespaceMappings);
        }

        var dynamicBindings = LookupNamespaces(receivedMessage.GetPayload<string>());

        if (namespaces != null && namespaces.Count != 0)
        {
            // Dynamic binding of namespace declarations in a root element of a received message
            foreach (var binding in dynamicBindings.Where(binding => !namespaces.ContainsValue(binding.Value)))
            {
                defaultNamespaceContext.AddNamespace(binding.Key, binding.Value);
            }

            // Add explicit namespace bindings
            defaultNamespaceContext.AddNamespaces(namespaces);
        }
        else
        {
            defaultNamespaceContext.AddNamespaces(dynamicBindings);
        }

        return defaultNamespaceContext;
    }

    /// <summary>
    ///     Construct a basic namespace context from the received message and explicit namespace mappings.
    ///     Returns an XmlNamespaceManager for compatibility with existing XPath operations.
    /// </summary>
    /// <param name="receivedMessage">the actual message received.</param>
    /// <param name="namespaces">explicit namespace mappings for this construction.</param>
    /// <returns>the constructed XmlNamespaceManager.</returns>
    public XmlNamespaceManager BuildXmlNamespaceManager(IMessage receivedMessage,
        Dictionary<string, string> namespaces = null)
    {
        var context = BuildContext(receivedMessage, namespaces);
        return context.ToXmlNamespaceManager();
    }

    /// <summary>
    ///     Look up namespace attribute declarations in the XML fragment and
    ///     store them in a binding map, where the key is the namespace prefix and the value
    ///     is the namespace uri.
    /// </summary>
    /// <param name="xml">XML fragment.</param>
    /// <returns>Dictionary containing namespace prefix - namespace uri pairs.</returns>
    public static Dictionary<string, string> LookupNamespaces(string xml)
    {
        var namespaces = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(xml) || !xml.Contains("xmlns"))
        {
            return namespaces;
        }

        // Regular expression to match xmlns declarations
        // Matches: xmlns="uri", xmlns:prefix="uri", xmlns:prefix='uri'
        const string xmlnsPattern = """xmlns(?::([^=\s]+))?\s*=\s*["']([^"']+)["']""";
        var matches = Regex.Matches(xml, xmlnsPattern, RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            var prefix = match.Groups[1].Success ? match.Groups[1].Value : string.Empty;
            var uri = match.Groups[2].Value;

            // Use empty string for the default namespace (like XMLConstants.DEFAULT_NS_PREFIX)
            var nsPrefix = string.IsNullOrEmpty(prefix) ? string.Empty : prefix;

            namespaces[nsPrefix] = uri;
        }

        return namespaces;
    }
}
