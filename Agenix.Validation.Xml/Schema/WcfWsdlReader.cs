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
            DocumentBaseUri = baseUri,
            Namespaces = new Dictionary<string, string>()
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
