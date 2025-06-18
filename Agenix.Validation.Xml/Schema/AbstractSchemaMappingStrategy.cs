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

namespace Agenix.Validation.Xml.Schema;

/// <summary>
///     Abstract schema mapping strategy extracts target namespace and root element name
///     for subclasses.
/// </summary>
public abstract class AbstractSchemaMappingStrategy : IXsdSchemaMappingStrategy
{
    /// <summary>
    ///     Gets the schema for given namespace or root element name.
    /// </summary>
    /// <param name="schemas">List of available schemas.</param>
    /// <param name="document">Document instance to validate.</param>
    /// <returns>The matching XSD schema or null if no match found.</returns>
    public IXsdSchema? GetSchema(List<IXsdSchema> schemas, XmlDocument document)
    {
        return document?.DocumentElement == null
            ? null
            : GetSchema(schemas, document.DocumentElement.NamespaceURI, document.DocumentElement.LocalName);
    }

    /// <summary>
    ///     Subclasses must override this method in order to detect schema for
    ///     target namespace and/or root element name.
    /// </summary>
    /// <param name="schemas">List of available schemas.</param>
    /// <param name="targetNamespace">Target namespace.</param>
    /// <param name="elementName">Root element name.</param>
    /// <returns>The matching XSD schema or null if no match found.</returns>
    public abstract IXsdSchema? GetSchema(List<IXsdSchema> schemas, string targetNamespace, string elementName);
}
