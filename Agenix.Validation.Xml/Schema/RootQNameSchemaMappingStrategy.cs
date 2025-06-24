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

using Agenix.Api.Exceptions;
using Agenix.Api.Util;
using Agenix.Validation.Xml.Namespace;

namespace Agenix.Validation.Xml.Schema;

/// <summary>
///     Mapping strategy uses the root element local name to find matching schema
///     instance.
/// </summary>
public class RootQNameSchemaMappingStrategy : AbstractSchemaMappingStrategy
{
    /// <summary>
    ///     Root element names mapping to schema instances
    /// </summary>
    private Dictionary<string, IXsdSchema?> _mappings = new();

    public override IXsdSchema? GetSchema(List<IXsdSchema> schemas, string namespaceName, string elementName)
    {
        IXsdSchema? schema = null;
        var rootQName = new QName(namespaceName, elementName, "");

        // Try to find by qualified name first
        if (_mappings.ContainsKey(rootQName.ToString()))
        {
            schema = _mappings[rootQName.ToString()];
        }
        // Fallback to element name only
        else if (_mappings.ContainsKey(elementName))
        {
            schema = _mappings[elementName];
        }

        // Validate namespace consistency
        if (schema != null &&
            !(StringUtils.HasText(schema.TargetNamespace) &&
              string.Equals(schema.TargetNamespace, namespaceName, StringComparison.Ordinal)))
        {
            throw new AgenixSystemException(
                $"Schema target namespace inconsistency for located XSD schema definition ({schema.TargetNamespace})");
        }

        return schema;
    }

    /// <summary>
    ///     Sets the mappings.
    /// </summary>
    /// <param name="mappings">The mappings to set</param>
    public void SetMappings(Dictionary<string, IXsdSchema?>? mappings)
    {
        _mappings = mappings ?? new Dictionary<string, IXsdSchema?>();
    }

    /// <summary>
    ///     Gets the mappings.
    /// </summary>
    /// <returns>The current mappings dictionary</returns>
    public Dictionary<string, IXsdSchema?> GetMappings()
    {
        return _mappings;
    }

    /// <summary>
    ///     Adds a mapping for an element name to a schema.
    /// </summary>
    /// <param name="elementName">The element name</param>
    /// <param name="schema">The schema to map to</param>
    public void AddMapping(string elementName, IXsdSchema? schema)
    {
        if (!string.IsNullOrEmpty(elementName) && schema != null)
        {
            _mappings[elementName] = schema;
        }
    }

    /// <summary>
    ///     Adds a mapping for a qualified name to a schema.
    /// </summary>
    /// <param name="namespaceName">The namespace</param>
    /// <param name="elementName">The element name</param>
    /// <param name="schema">The schema to map to</param>
    public void AddMapping(string? namespaceName, string elementName, IXsdSchema? schema)
    {
        if (!string.IsNullOrEmpty(elementName) && schema != null)
        {
            var qName = new QName(namespaceName ?? string.Empty, elementName, "");
            _mappings[qName.ToString()] = schema;
        }
    }

    /// <summary>
    ///     Removes a mapping.
    /// </summary>
    /// <param name="key">The key to remove</param>
    /// <returns>True if the mapping was removed, false otherwise</returns>
    public bool RemoveMapping(string key)
    {
        return _mappings.Remove(key);
    }

    /// <summary>
    ///     Clears all mappings.
    /// </summary>
    public void ClearMappings()
    {
        _mappings.Clear();
    }
}
