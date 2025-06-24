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

using Agenix.Validation.Xml.Util;

namespace Agenix.Validation.Xml.Namespace;

/// <summary>
///     QName represents a qualified name as defined in the XML specifications:
///     NCName, Namespace Name, Namespace Prefix.
/// </summary>
[Serializable]
public class QName : IEquatable<QName>
{
    /// <summary>
    ///     Local part of this QName.
    /// </summary>
    private readonly string _localPart;

    /// <summary>
    ///     Namespace URI of this QName.
    /// </summary>
    private readonly string _namespaceURI;

    /// <summary>
    ///     Prefix of this QName.
    /// </summary>
    private readonly string _prefix;

    /// <summary>
    ///     QName constructor specifying the local part.
    /// </summary>
    /// <param name="localPart">local part of the QName</param>
    /// <exception cref="ArgumentException">When localPart is null</exception>
    public QName(string localPart) : this(XmlConstants.NullNsUri, localPart, XmlConstants.DefaultNsPrefix)
    {
    }

    /// <summary>
    ///     QName constructor specifying the Namespace URI and local part.
    /// </summary>
    /// <param name="namespaceURI">Namespace URI of the QName</param>
    /// <param name="localPart">local part of the QName</param>
    /// <exception cref="ArgumentException">When localPart is null</exception>
    public QName(string namespaceURI, string localPart) : this(namespaceURI, localPart, XmlConstants.DefaultNsPrefix)
    {
    }

    /// <summary>
    ///     QName constructor specifying the Namespace URI, local part and prefix.
    /// </summary>
    /// <param name="namespaceURI">Namespace URI of the QName</param>
    /// <param name="localPart">local part of the QName</param>
    /// <param name="prefix">prefix of the QName</param>
    /// <exception cref="ArgumentException">When localPart or prefix is null</exception>
    public QName(string namespaceURI, string localPart, string prefix)
    {
        // Map null Namespace URI to default to preserve compatibility with QName 1.0
        _namespaceURI = namespaceURI ?? XmlConstants.NullNsUri;

        // Local part is required. "" is allowed to preserve compatibility with QName 1.0
        if (localPart == null)
        {
            throw new ArgumentException("local part cannot be \"null\" when creating a QName");
        }

        _localPart = localPart;

        // Prefix is required
        if (prefix == null)
        {
            throw new ArgumentException("prefix cannot be \"null\" when creating a QName");
        }

        _prefix = prefix;
    }

    /// <summary>
    ///     Get the Namespace URI of this QName.
    /// </summary>
    public string NamespaceURI => _namespaceURI;

    /// <summary>
    ///     Get the local part of this QName.
    /// </summary>
    public string LocalPart => _localPart;

    /// <summary>
    ///     Get the prefix of this QName.
    /// </summary>
    public string Prefix => _prefix;

    /// <summary>
    ///     Test this QName for equality with another QName.
    /// </summary>
    /// <param name="other">the QName to test for equality</param>
    /// <returns>true if the given QName is equal to this QName else false</returns>
    public bool Equals(QName other)
    {
        if (other == null)
        {
            return false;
        }

        return _localPart.Equals(other._localPart) && _namespaceURI.Equals(other._namespaceURI);
    }

    /// <summary>
    ///     Test this QName for equality with another Object.
    ///     Two QNames are considered equal if and only if both the Namespace URI and local part are equal.
    ///     The prefix is NOT used to determine equality.
    /// </summary>
    /// <param name="objectToTest">the Object to test for equality with this QName</param>
    /// <returns>true if the given Object is equal to this QName else false</returns>
    public override bool Equals(object objectToTest)
    {
        if (objectToTest == this)
        {
            return true;
        }

        if (objectToTest is not QName)
        {
            return false;
        }

        var qName = (QName)objectToTest;
        return _localPart.Equals(qName._localPart) && _namespaceURI.Equals(qName._namespaceURI);
    }

    /// <summary>
    ///     Generate the hash code for this QName.
    ///     The hash code is calculated using both the Namespace URI and the local part of the QName.
    ///     The prefix is NOT used to calculate the hash code.
    /// </summary>
    /// <returns>hash code for this QName Object</returns>
    public override int GetHashCode()
    {
        return _namespaceURI.GetHashCode() ^ _localPart.GetHashCode();
    }

    /// <summary>
    ///     String representation of this QName.
    ///     This implementation represents a QName as: "{" + Namespace URI + "}" + local part.
    ///     If the Namespace URI equals XMLConstants.NULL_NS_URI, only the local part is returned.
    ///     Note the prefix value is NOT returned as part of the String representation.
    /// </summary>
    /// <returns>String representation of this QName</returns>
    public override string ToString()
    {
        if (_namespaceURI.Equals(XmlConstants.NullNsUri))
        {
            return _localPart;
        }

        return "{" + _namespaceURI + "}" + _localPart;
    }

    /// <summary>
    ///     QName derived from parsing the formatted String.
    ///     The String MUST be in the form returned by QName.ToString().
    ///     This implementation parses a String formatted as: "{" + Namespace URI + "}" + local part.
    ///     If the Namespace URI equals XMLConstants.NULL_NS_URI, only the local part should be provided.
    ///     The prefix value CANNOT be represented in the String and will be set to XMLConstants.DEFAULT_NS_PREFIX.
    /// </summary>
    /// <param name="qNameAsString">String representation of the QName</param>
    /// <returns>QName corresponding to the given String</returns>
    /// <exception cref="ArgumentException">When qNameAsString is null or malformed</exception>
    public static QName ValueOf(string qNameAsString)
    {
        // null is not valid
        if (qNameAsString == null)
        {
            throw new ArgumentException("cannot create QName from \"null\" or \"\" String");
        }

        // "" local part is valid to preserve compatible behavior with QName 1.0
        if (qNameAsString.Length == 0)
        {
            return new QName(XmlConstants.NullNsUri, qNameAsString, XmlConstants.DefaultNsPrefix);
        }

        // local part only?
        if (qNameAsString[0] != '{')
        {
            return new QName(XmlConstants.NullNsUri, qNameAsString, XmlConstants.DefaultNsPrefix);
        }

        // Namespace URI improperly specified?
        if (qNameAsString.StartsWith("{" + XmlConstants.NullNsUri + "}"))
        {
            throw new ArgumentException(
                "Namespace URI .equals(XMLConstants.NULL_NS_URI), " +
                ".equals(\"" + XmlConstants.NullNsUri + "\"), " +
                "only the local part, " +
                "\"" + qNameAsString.Substring(2 + XmlConstants.NullNsUri.Length) + "\", " +
                "should be provided.");
        }

        // Namespace URI and local part specified
        var endOfNamespaceURI = qNameAsString.IndexOf('}');
        if (endOfNamespaceURI == -1)
        {
            throw new ArgumentException(
                "cannot create QName from \"" + qNameAsString + "\", missing closing \"}\"");
        }

        return new QName(
            qNameAsString.Substring(1, endOfNamespaceURI - 1),
            qNameAsString.Substring(endOfNamespaceURI + 1),
            XmlConstants.DefaultNsPrefix);
    }

    public static bool operator ==(QName left, QName right)
    {
        return ReferenceEquals(left, right) || (left?.Equals(right) ?? false);
    }

    public static bool operator !=(QName left, QName right)
    {
        return !(left == right);
    }
}
