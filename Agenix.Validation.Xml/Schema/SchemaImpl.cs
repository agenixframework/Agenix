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
using System.Xml.Schema;

namespace Agenix.Validation.Xml.Schema;

/// <summary>
///     Represents an implementation of the <see cref="IWsdlSchema" /> interface, providing functionality
///     to work with XML schemas as both <see cref="XmlSchema" /> and <see cref="XmlElement" /> objects.
/// </summary>
public class SchemaImpl : IWsdlSchema
{
    public SchemaImpl(XmlSchema xmlSchema)
    {
        Schema = xmlSchema;
        // Convert XmlSchema to XmlElement if needed
        Element = ConvertSchemaToElement(xmlSchema);
    }

    public SchemaImpl(XmlElement element)
    {
        Element = element;
        // Convert XmlElement to XmlSchema if needed
        Schema = ConvertElementToSchema(element);
    }

    public XmlSchema Schema { get; }

    public XmlElement Element { get; }

    public string TargetNamespace => Schema?.TargetNamespace ?? Element?.GetAttribute("targetNamespace");

    /// <summary>
    ///     Converts an <see cref="XmlSchema" /> to an <see cref="XmlElement" />.
    /// </summary>
    /// <param name="schema">The <see cref="XmlSchema" /> to be converted to an <see cref="XmlElement" />.</param>
    /// <returns>An <see cref="XmlElement" /> instance representing the provided <see cref="XmlSchema" />.</returns>
    private XmlElement ConvertSchemaToElement(XmlSchema schema)
    {
        var doc = new XmlDocument();
        using var writer = doc.CreateNavigator().AppendChild();
        schema.Write(writer);
        return doc.DocumentElement;
    }

    /// <summary>
    ///     Converts an <see cref="XmlElement" /> to an <see cref="XmlSchema" />.
    /// </summary>
    /// <param name="element">The <see cref="XmlElement" /> to be converted to an <see cref="XmlSchema" />.</param>
    /// <returns>An <see cref="XmlSchema" /> instance representing the provided <see cref="XmlElement" />.</returns>
    private XmlSchema ConvertElementToSchema(XmlElement element)
    {
        using var reader = new XmlNodeReader(element);
        return XmlSchema.Read(reader, null);
    }
}
