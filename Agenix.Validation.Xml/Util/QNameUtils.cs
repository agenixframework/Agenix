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
using Agenix.Validation.Xml.Namespace;

namespace Agenix.Validation.Xml.Util;

/// <summary>
///     Utility class for working with XML qualified names (QName).
/// </summary>
public static class QNameUtils
{
    /// <summary>
    ///     Validates the given string as a QName.
    /// </summary>
    /// <param name="text">The qualified name</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool ValidateQName(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        if (text[0] == '{')
        {
            var closingBrace = text.IndexOf('}');
            if (closingBrace == -1 || closingBrace == text.Length - 1)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Returns the qualified name of the given XML node.
    /// </summary>
    /// <param name="node">The XML node</param>
    /// <returns>The qualified name of the node</returns>
    public static QName GetQNameForNode(XmlNode node)
    {
        if (!string.IsNullOrEmpty(node.NamespaceURI) &&
            !string.IsNullOrEmpty(node.Prefix) &&
            !string.IsNullOrEmpty(node.LocalName))
        {
            return new QName(node.NamespaceURI, node.LocalName, node.Prefix);
        }

        if (!string.IsNullOrEmpty(node.NamespaceURI) && !string.IsNullOrEmpty(node.LocalName))
        {
            return new QName(node.NamespaceURI, node.LocalName);
        }

        if (!string.IsNullOrEmpty(node.LocalName))
        {
            return new QName(node.LocalName);
        }

        // As a last resort, use the node name
        return new QName(node.Name);
    }

    /// <summary>
    ///     Parse the given qualified name string into a QName.
    ///     Expects the syntax localPart, {namespace}localPart, or {namespace}prefix:localPart.
    /// </summary>
    /// <param name="qNameString">The QName string to parse</param>
    /// <returns>A corresponding QName instance</returns>
    /// <exception cref="ArgumentException">When the given string is null or empty</exception>
    public static QName ParseQNameString(string qNameString)
    {
        if (string.IsNullOrEmpty(qNameString))
        {
            throw new ArgumentException("QName text may not be null or empty", nameof(qNameString));
        }

        if (qNameString[0] != '{')
        {
            return new QName(qNameString);
        }

        var endOfNamespaceUri = qNameString.IndexOf('}');
        if (endOfNamespaceUri == -1)
        {
            throw new ArgumentException($"Cannot create QName from \"{qNameString}\", missing closing \"}}\"");
        }

        var prefixSeparator = qNameString.IndexOf(':', endOfNamespaceUri + 1);
        var namespaceUri = qNameString.Substring(1, endOfNamespaceUri - 1);

        if (prefixSeparator == -1)
        {
            var localName = qNameString.Substring(endOfNamespaceUri + 1);
            return new QName(namespaceUri, localName);
        }
        else
        {
            var localName = qNameString.Substring(prefixSeparator + 1);
            var prefix = qNameString.Substring(endOfNamespaceUri + 1, prefixSeparator - endOfNamespaceUri - 1);
            return new QName(namespaceUri, localName, prefix);
        }
    }
}
