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
using Agenix.Validation.Xml.Schema.Locator;

namespace Agenix.Validation.Xml.Schema;

/// <summary>
///     Provides functionality to read and parse WSDL (Web Services Description Language) documents.
///     Implements the <see cref="IWsdlReader" /> interface to handle WSDL input from various sources,
///     including locators and input streams.
/// </summary>
public class WcfWsdlReader : IWsdlReader
{
    /// <summary>
    ///     Reads a WSDL (Web Services Description Language) document using the specified locator
    ///     and extracts its definition, including the target namespace and namespace declarations.
    /// </summary>
    /// <param name="locator">The locator that provides the base input source and base URI of the WSDL document.</param>
    /// <returns>A <see cref="WsdlDefinition" /> object containing the parsed details of the WSDL document.</returns>
    public WsdlDefinition ReadWsdl(IWsdlLocator locator)
    {
        using var baseStream = locator.GetBaseInputSource();
        var doc = new XmlDocument();
        doc.Load(baseStream);

        return ParseWsdlDocument(doc, locator.GetBaseUri());
    }

    /// <summary>
    ///     Reads a WSDL (Web Services Description Language) document from the specified locator
    ///     and extracts its definition, including the target namespace and namespace declarations.
    /// </summary>
    /// <param name="locator">The locator that provides the base input source and base URI of the WSDL document.</param>
    /// <returns>A <see cref="WsdlDefinition" /> object containing the parsed details of the WSDL document.</returns>
    public WsdlDefinition ReadWsdl(string documentBaseUri, Stream inputStream)
    {
        var doc = new XmlDocument();
        doc.Load(inputStream);

        return ParseWsdlDocument(doc, documentBaseUri);
    }


    public void Dispose()
    {
        // Nothing to dispose
    }

    /// <summary>
    ///     Parses an XML document representing a WSDL (Web Services Description Language)
    ///     and extracts its definition, including the target namespace and namespace declarations.
    /// </summary>
    /// <param name="doc">The XML document to parse, representing the WSDL structure.</param>
    /// <param name="baseUri">The base URI of the WSDL document, used to resolve relative references.</param>
    /// <returns>A <see cref="WsdlDefinition" /> object containing the parsed details of the WSDL document.</returns>
    private static WsdlDefinition ParseWsdlDocument(XmlDocument doc, string baseUri)
    {
        var definition = new WsdlDefinition
        {
            DocumentBaseUri = baseUri, Namespaces = new Dictionary<string, string>()
        };

        // Extract target namespace
        var definitionsElement = doc.DocumentElement;
        if (definitionsElement?.LocalName == "definitions")
        {
            definition.TargetNamespace = definitionsElement.GetAttribute("targetNamespace");

            // Extract namespace declarations
            foreach (XmlAttribute attr in definitionsElement.Attributes)
            {
                if (attr.Name.StartsWith("xmlns:"))
                {
                    var prefix = attr.Name[6..];
                    definition.Namespaces[prefix] = attr.Value;
                }
                else if (attr.Name == "xmlns")
                {
                    definition.Namespaces[""] = attr.Value;
                }
            }
        }

        // Parse imports
        ParseImports(definitionsElement, definition);

        // Parse types
        ParseTypes(definitionsElement, definition);


        return definition;
    }

    private static void ParseImports(XmlElement definitionsElement, WsdlDefinition definition)
    {
        var importNodes = definitionsElement.GetElementsByTagName("import");
        foreach (XmlElement importElement in importNodes)
        {
            var import = new WsdlImport
            {
                Namespace = importElement.GetAttribute("namespace"),
                LocationUri = importElement.GetAttribute("location")
            };
            definition.Imports.Add(import);
        }
    }

    private static void ParseTypes(XmlElement definitionsElement, WsdlDefinition definition)
    {
        var typesNodes = definitionsElement.GetElementsByTagName("types");
        if (typesNodes.Count == 0)
        {
            return;
        }

        definition.Types = new WsdlTypes();

        foreach (XmlElement typesElement in typesNodes)
        {
            // Add all child elements as extensibility elements
            // Common extensibility elements in types include XSD schemas, documentation, etc.
            foreach (var childElement in typesElement.ChildNodes.OfType<XmlElement>())
            {
                definition.Types.ExtensibilityElements.Add(childElement);
            }
        }
    }
}
