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

using System.Text;
using System.Xml;
using Agenix.Api;

namespace Agenix.Validation.Xml.Util;

/// <summary>
///     Class providing several utility methods for XML processing.
/// </summary>
public static class XmlUtils
{
    private static XmlConfigurer _configurer;

    static XmlUtils()
    {
        _configurer = new XmlConfigurer();
        _configurer.Initialize();
    }

    /// <summary>
    ///     Initializes XML utilities with custom configurer.
    /// </summary>
    /// <param name="xmlConfigurer">The XML configurer</param>
    public static void Initialize(XmlConfigurer xmlConfigurer)
    {
        _configurer = xmlConfigurer;
    }

    /// <summary>
    ///     Searches for a node within a DOM document with a given node path expression.
    ///     Elements are separated by '.' characters.
    ///     Example: Foo.Bar.Poo
    /// </summary>
    /// <param name="doc">DOM Document to search for a node</param>
    /// <param name="pathExpression">dot separated path expression</param>
    /// <returns>Node element found in the DOM document</returns>
    public static XmlNode? FindNodeByName(XmlDocument doc, string pathExpression)
    {
        var pathParts = pathExpression.Split('.');
        var numParts = pathParts.Length;

        if (numParts == 1)
        {
            var elementsByTagName = doc.GetElementsByTagName(pathExpression);
            return elementsByTagName.Count > 0 ? elementsByTagName[0] : null;
        }

        var element = pathParts.Last();
        var elements = doc.GetElementsByTagName(element);

        if (elements.Count == 0)
        {
            // No element found, but maybe we are searching for an attribute
            var parentPath = pathExpression[..(pathExpression.Length - element.Length - 1)];
            var found = FindNodeByName(doc, parentPath);

            return found?.Attributes?[element];
        }

        foreach (XmlNode elementNode in elements)
        {
            var pathBuilder = new StringBuilder(element);
            var parent = elementNode.ParentNode;
            var cnt = numParts - 1;

            while (parent != null && cnt > 0)
            {
                if (parent.NodeType != XmlNodeType.Document)
                {
                    pathBuilder.Insert(0, '.');
                    pathBuilder.Insert(0, parent.LocalName);
                }

                parent = parent.ParentNode;
                cnt--;
            }

            if (pathBuilder.ToString() == pathExpression)
            {
                return elementNode;
            }
        }

        return null;
    }

    /// <summary>
    ///     Removes text nodes that are only containing whitespace characters inside a DOM tree.
    /// </summary>
    /// <param name="element">the root node to normalize</param>
    public static void StripWhitespaceNodes(XmlNode element)
    {
        foreach (XmlNode child in element.ChildNodes)
        {
            StripWhitespaceNodes(child);
        }

        if (element.NodeType == XmlNodeType.Text &&
            string.IsNullOrWhiteSpace(element.Value))
        {
            element.ParentNode?.RemoveChild(element);
        }
    }

    /// <summary>
    ///     Returns the path expression for a given node.
    ///     Path expressions look like: Foo.Bar.Poo where elements are separated with a dot character.
    /// </summary>
    /// <param name="node">node in DOM tree</param>
    /// <returns>the path expression representing the node in DOM tree</returns>
    public static string GetNodesPathName(XmlNode node)
    {
        var builder = new StringBuilder();

        if (node.NodeType == XmlNodeType.Attribute)
        {
            var attr = (XmlAttribute)node;
            if (attr.OwnerElement != null)
            {
                BuildNodeName(attr.OwnerElement, builder);
            }

            builder.Append('.');
            builder.Append(node.LocalName);
        }
        else
        {
            BuildNodeName(node, builder);
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Builds the node path expression for a node in the DOM tree.
    /// </summary>
    /// <param name="node">node in a DOM tree</param>
    /// <param name="builder">string builder</param>
    private static void BuildNodeName(XmlNode node, StringBuilder builder)
    {
        if (node?.ParentNode == null || node.ParentNode.NodeType == XmlNodeType.Document)
        {
            if (node?.LocalName != null)
            {
                builder.Append(node.LocalName);
            }

            return;
        }

        BuildNodeName(node.ParentNode, builder);

        if (node.ParentNode?.ParentNode != null)
        {
            builder.Append('.');
        }

        builder.Append(node.LocalName);
    }

    /// <summary>
    ///     Serializes a DOM document
    /// </summary>
    /// <param name="doc">Document to serialize</param>
    /// <returns>serialized XML string</returns>
    public static string Serialize(XmlDocument doc)
    {
        // Get the configured writer settings instead of hardcoded ones
        var settings = _configurer.CreateXmlWriterSettings();

        // Preserve the encoding from the document if specified
        var targetEncoding = GetTargetEncoding(doc);
        if (targetEncoding != null)
        {
            settings.Encoding = targetEncoding;
        }

        using var stringWriter = new EncodingStringWriter(settings.Encoding);
        using var xmlWriter = XmlWriter.Create(stringWriter, settings);
        doc.WriteTo(xmlWriter);
        xmlWriter.Flush();
        return stringWriter.ToString();
    }


    /// <summary>
    ///     Pretty prints an XML string.
    /// </summary>
    /// <param name="xml">XML string to format</param>
    /// <returns>pretty printed XML string</returns>
    public static string PrettyPrint(string xml)
    {
        if (string.IsNullOrEmpty(xml))
        {
            return xml;
        }

        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml.Trim());
            return Serialize(doc);
        }
        catch
        {
            return xml;
        }
    }

    /// <summary>
    ///     Look up namespace attribute declarations in the specified node and
    ///     store them in a binding map, where the key is the namespace prefix and the value
    ///     is the namespace uri.
    /// </summary>
    /// <param name="referenceNode">XML node to search for namespace declarations</param>
    /// <returns>map containing namespace prefix - namespace url pairs</returns>
    public static Dictionary<string, string> LookupNamespaces(XmlNode referenceNode)
    {
        var namespaces = new Dictionary<string, string>();

        var node = referenceNode.NodeType == XmlNodeType.Document
            ? ((XmlDocument)referenceNode).DocumentElement // Get the root element
            : referenceNode;

        if (node?.Attributes != null)
        {
            foreach (XmlAttribute attribute in node.Attributes)
            {
                if (attribute.Name.StartsWith("xmlns:"))
                {
                    var prefix = attribute.Name["xmlns:".Length..];
                    namespaces[prefix] = attribute.Value;
                }
                else if (attribute.Name == "xmlns")
                {
                    // default namespace
                    namespaces[string.Empty] = attribute.Value;
                }
            }
        }

        return namespaces;
    }


    /// <summary>
    ///     Parse message payload with DOM implementation.
    /// </summary>
    /// <param name="messagePayload">XML message payload</param>
    /// <returns>DOM document</returns>
    public static XmlDocument ParseMessagePayload(string messagePayload)
    {
        var doc = _configurer.CreateXmlDocument();

        // Use configured XmlReader for proper parsing behavior
        using var reader = _configurer.CreateXmlReader(messagePayload.Trim());
        doc.Load(reader); // Use Load(XmlReader) instead of LoadXml(string)

        return doc;
    }

    /// <summary>
    ///     Try to find encoding for document node. Also supports default encoding set
    ///     as system property or environment variable.
    /// </summary>
    /// <param name="doc">XML document</param>
    /// <returns>Target encoding</returns>
    public static Encoding GetTargetEncoding(XmlDocument doc)
    {
        // Check for default encoding from environment/config
        var defaultEncoding = AgenixSettings.AgenixFileEncoding();

        if (!string.IsNullOrEmpty(defaultEncoding))
        {
            try
            {
                return Encoding.GetEncoding(defaultEncoding);
            }
            catch
            {
                // Fall through to default
            }
        }

        // Try to get encoding from XML declaration
        if (doc.FirstChild is XmlDeclaration xmlDeclaration &&
            !string.IsNullOrEmpty(xmlDeclaration.Encoding))
        {
            try
            {
                return Encoding.GetEncoding(xmlDeclaration.Encoding);
            }
            catch
            {
                // Fall through to default
            }
        }

        return Encoding.UTF8;
    }

    /// <summary>
    ///     Try to find target encoding in XML declaration.
    /// </summary>
    /// <param name="messagePayload">XML message payload</param>
    /// <returns>Target encoding</returns>
    private static Encoding GetTargetEncoding(string messagePayload)
    {
        // Check for default encoding from environment/config
        var defaultEncoding = AgenixSettings.AgenixFileEncoding();

        if (!string.IsNullOrEmpty(defaultEncoding))
        {
            try
            {
                return Encoding.GetEncoding(defaultEncoding);
            }
            catch
            {
                // Fall through to parsing
            }
        }

        var payload = messagePayload.Trim();
        const string encodingKey = "encoding";

        if (payload.StartsWith("<?xml") &&
            payload.Contains(encodingKey) &&
            payload.Contains("?>") &&
            payload.IndexOf(encodingKey, StringComparison.Ordinal) < payload.IndexOf("?>", StringComparison.Ordinal))
        {
            try
            {
                var encodingPart = payload.Substring(
                    payload.IndexOf(encodingKey, StringComparison.Ordinal) + encodingKey.Length,
                    payload.IndexOf("?>", StringComparison.Ordinal) -
                    payload.IndexOf(encodingKey, StringComparison.Ordinal) - encodingKey.Length);

                var quoteChar = '"';
                var doubleQuoteIndex = encodingPart.IndexOf('"');
                var singleQuoteIndex = encodingPart.IndexOf('\'');

                if (singleQuoteIndex >= 0 && (doubleQuoteIndex < 0 || singleQuoteIndex < doubleQuoteIndex))
                {
                    quoteChar = '\'';
                }

                var startIndex = encodingPart.IndexOf(quoteChar) + 1;
                var endIndex = encodingPart.IndexOf(quoteChar, startIndex);

                if (startIndex > 0 && endIndex > startIndex)
                {
                    var encodingName = encodingPart.Substring(startIndex, endIndex - startIndex);
                    return Encoding.GetEncoding(encodingName);
                }
            }
            catch
            {
                // Fall through to default
            }
        }

        return Encoding.UTF8;
    }

    /// <summary>
    ///     Removes leading XML declaration from xml if present.
    /// </summary>
    /// <param name="xml">XML string</param>
    /// <returns>XML string without declaration</returns>
    public static string OmitXmlDeclaration(string xml)
    {
        if (xml.StartsWith("<?xml") && xml.Contains("?>"))
        {
            return xml[(xml.IndexOf("?>", StringComparison.Ordinal) + 2)..].Trim();
        }

        return xml;
    }


    private class EncodingStringWriter(Encoding encoding) : StringWriter
    {
        public override Encoding Encoding => encoding;
    }
}
