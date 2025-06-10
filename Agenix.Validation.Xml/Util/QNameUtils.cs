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


using System.Xml;
using Agenix.Validation.Xml.Namespace;

namespace Agenix.Validation.Xml.Util;

/// <summary>
/// Utility class for working with XML qualified names (QName).
/// </summary>
public static class QNameUtils
{
    /// <summary>
    /// Validates the given string as a QName.
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
            int closingBrace = text.IndexOf('}');
            if (closingBrace == -1 || closingBrace == text.Length - 1)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns the qualified name of the given XML node.
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
        else if (!string.IsNullOrEmpty(node.NamespaceURI) && !string.IsNullOrEmpty(node.LocalName))
        {
            return new QName(node.NamespaceURI, node.LocalName);
        }
        else if (!string.IsNullOrEmpty(node.LocalName))
        {
            return new QName(node.LocalName);
        }
        else
        {
            // As a last resort, use the node name
            return new QName(node.Name);
        }
    }

    /// <summary>
    /// Parse the given qualified name string into a QName.
    /// Expects the syntax localPart, {namespace}localPart, or {namespace}prefix:localPart.
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

