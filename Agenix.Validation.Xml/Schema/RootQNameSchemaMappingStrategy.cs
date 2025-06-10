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
