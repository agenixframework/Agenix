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

namespace Agenix.Api.Xml.Namespace;

/// <summary>
///     Default implementation of IXmlNamespaceResolver that provides namespace context functionality
///     similar to Java's NamespaceContext interface.
/// </summary>
public class DefaultNamespaceContext : IXmlNamespaceResolver
{
    private readonly Dictionary<string, string> _namespaces = new();

    /// <summary>
    ///     Gets the namespace URI for the specified prefix (IXmlNamespaceResolver implementation).
    /// </summary>
    string IXmlNamespaceResolver.LookupNamespace(string prefix)
    {
        return GetNamespaceUri(prefix);
    }

    /// <summary>
    ///     Gets the prefix for the specified namespace URI (IXmlNamespaceResolver implementation).
    /// </summary>
    string IXmlNamespaceResolver.LookupPrefix(string namespaceName)
    {
        return GetPrefix(namespaceName);
    }

    /// <summary>
    ///     Gets all namespace declarations in scope.
    /// </summary>
    IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope(XmlNamespaceScope scope)
    {
        return new Dictionary<string, string>(_namespaces);
    }

    /// <summary>
    ///     Adds given namespace mappings to this context.
    /// </summary>
    /// <param name="mappings">The namespace mappings to add</param>
    public void AddNamespaces(Dictionary<string, string> mappings)
    {
        if (mappings == null)
        {
            return;
        }

        foreach (var mapping in mappings)
        {
            _namespaces[mapping.Key] = mapping.Value;
        }
    }

    /// <summary>
    ///     Adds a single namespace mapping to this context.
    /// </summary>
    /// <param name="prefix">The namespace prefix</param>
    /// <param name="namespaceUri">The namespace URI</param>
    public void AddNamespace(string prefix, string namespaceUri)
    {
        _namespaces[prefix ?? string.Empty] = namespaceUri;
    }

    /// <summary>
    ///     Gets the namespace URI for the given prefix.
    /// </summary>
    /// <param name="prefix">The namespace prefix</param>
    /// <returns>The namespace URI, or null if not found</returns>
    public string GetNamespaceUri(string prefix)
    {
        return _namespaces.GetValueOrDefault(prefix ?? string.Empty);
    }

    /// <summary>
    ///     Gets the first prefix associated with the given namespace URI.
    /// </summary>
    /// <param name="newNamespaceUri">The namespace URI</param>
    /// <returns>The prefix, or null if not found</returns>
    public string GetPrefix(string newNamespaceUri)
    {
        if (string.IsNullOrEmpty(newNamespaceUri))
        {
            return null;
        }

        return _namespaces
            .Where(entry => newNamespaceUri.Equals(entry.Value))
            .Select(entry => entry.Key)
            .FirstOrDefault();
    }

    /// <summary>
    ///     Gets all prefixes associated with the given namespace URI.
    /// </summary>
    /// <param name="newNamespaceUri">The namespace URI</param>
    /// <returns>An enumerable of prefixes</returns>
    public IEnumerable<string> GetPrefixes(string newNamespaceUri)
    {
        if (string.IsNullOrEmpty(newNamespaceUri))
        {
            return [];
        }

        return _namespaces
            .Where(entry => newNamespaceUri.Equals(entry.Value))
            .Select(entry => entry.Key)
            .ToList();
    }

    /// <summary>
    ///     Gets all namespace mappings in this context.
    /// </summary>
    /// <returns>A copy of the namespace mappings</returns>
    public Dictionary<string, string> GetNamespaces()
    {
        return new Dictionary<string, string>(_namespaces);
    }

    /// <summary>
    ///     Checks if a prefix is defined in this context.
    /// </summary>
    /// <param name="prefix">The prefix to check</param>
    /// <returns>True if the prefix is defined, false otherwise</returns>
    public bool HasNamespaceUri(string prefix)
    {
        return _namespaces.ContainsKey(prefix ?? string.Empty);
    }
}
