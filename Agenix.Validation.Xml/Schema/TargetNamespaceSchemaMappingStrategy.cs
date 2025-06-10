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


using System.Xml.Schema;
using Agenix.Api.Util;

namespace Agenix.Validation.Xml.Schema;

/// <summary>
/// Mapping strategy checks on target namespaces in schemas to find matching schema
/// instance.
/// </summary>
public class TargetNamespaceSchemaMappingStrategy : AbstractSchemaMappingStrategy
{
    /// <summary>
    /// Retrieves an XML schema from a collection of schemas that matches the specified namespace and element name.
    /// </summary>
    /// <param name="schemas">The collection of XML schemas to search through.</param>
    /// <param name="namespaceName">The target namespace to match against the schemas' target namespaces.</param>
    /// <param name="elementName">The name of the root element to match (not used in the logic of this implementation).</param>
    /// <returns>
    /// The first matching <see cref="XmlSchema"/> whose target namespace matches the specified <paramref name="namespaceName"/>, or null if no match is found.
    /// </returns>
    public override IXsdSchema? GetSchema(List<IXsdSchema> schemas, string namespaceName, string elementName)
    {
        return schemas.FirstOrDefault(schema => StringUtils.HasText(schema?.TargetNamespace ?? string.Empty) && string.Equals(schema?.TargetNamespace, namespaceName, StringComparison.Ordinal));
    }
}

