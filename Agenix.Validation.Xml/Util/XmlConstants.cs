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


namespace Agenix.Validation.Xml.Util;

/// <summary>
/// XML constants equivalent to Java's XMLConstants.
/// </summary>
public static class XmlConstants
{
    /// <summary>
    /// Namespace URI to use to represent that there is no Namespace.
    /// </summary>
    public const string NullNsUri = "";

    /// <summary>
    /// Prefix to use to represent the default XML Namespace.
    /// </summary>
    public const string DefaultNsPrefix = "";

    /// <summary>
    /// The official XML Namespace name URI.
    /// </summary>
    public const string XmlNsUri = "http://www.w3.org/XML/1998/namespace";

    /// <summary>
    /// The official XML Namespace prefix.
    /// </summary>
    public const string XmlNsPrefix = "xml";

    /// <summary>
    /// The official XML attribute used for specifying XML Namespace declarations.
    /// </summary>
    public const string XmlnsAttribute = "xmlns";

    /// <summary>
    /// The official XML attribute used for specifying XML Namespace declarations, XMLConstants.XMLNS_ATTRIBUTE, Namespace name URI.
    /// </summary>
    public const string XmlnsAttributeNsUri = "http://www.w3.org/2000/xmlns/";
}
