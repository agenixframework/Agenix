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


using System.Text;
using System.Xml;
using System.Xml.Schema;
using Agenix.Api.Exceptions;
using Agenix.Api.IO;
using Agenix.Api.Log;
using Agenix.Api.Util;
using Agenix.Core.Util;
using Agenix.Validation.Xml.Schema.Locator;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Xml.Schema;

public class WsdlXsdSchema : AbstractSchemaCollection
{
    /// <summary>
    /// WSDL file resource
    /// </summary>
    private IResource _wsdl;

    /// <summary>
    /// Logger
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(WsdlXsdSchema));

    /// <summary>
    /// Default constructor
    /// </summary>
    public WsdlXsdSchema() : base()
    {
    }

    /// <summary>
    /// Constructor using WSDL resource.
    /// </summary>
    /// <param name="wsdl">The WSDL resource</param>
    public WsdlXsdSchema(IResource wsdl) : base()
    {
        _wsdl = wsdl;
    }


    private void InheritNamespaces(IWsdlSchema schema, WsdlDefinition wsdl)
    {
        var wsdlNamespaces = wsdl.Namespaces;

        foreach (var nsEntry in wsdlNamespaces)
        {
            if (StringUtils.HasText(nsEntry.Key))
            {
                var attributeName = $"xmlns:{nsEntry.Key}";
                if (string.IsNullOrEmpty(schema.Element.GetAttribute(attributeName)))
                {
                    schema.Element.SetAttribute(attributeName, nsEntry.Value);
                }
            }
            else // handle default namespace
            {
                if (!schema.Element.HasAttribute("xmlns"))
                {
                    schema.Element.SetAttribute("xmlns", nsEntry.Value);
                }
            }
        }
    }

    private IResource CreateSchemaResourceFromElement(XmlElement element)
    {
        try
        {
            using var memoryStream = new MemoryStream();
            using var xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = false,
                OmitXmlDeclaration = false
            });

            element.OwnerDocument.Save(xmlWriter);
            xmlWriter.Flush();

            return new ByteArrayResource(memoryStream.ToArray());
        }
        catch (Exception e)
        {
            throw new AgenixSystemException("Failed to transform schema element to resource", e);
        }
    }

    private string GetTargetNamespace(IWsdlSchema schema)
    {
        return schema.TargetNamespace ?? schema.Element.GetAttribute("targetNamespace");
    }


    private IResource LoadSchemas(WsdlDefinition definition)
    {
        var types = definition.Types;
        IResource targetXsd = null;
        IResource firstSchemaInWsdl = null;

        if (types != null)
        {
            var schemaTypes = types.ExtensibilityElements;
            foreach (var schemaObject in schemaTypes)
            {
                if (schemaObject is SchemaImpl schema)
                {
                    InheritNamespaces(schema, definition);

                    AddImportedSchemas(schema.Schema);
                    AddIncludedSchemas(schema.Schema);

                    var targetNamespace = GetTargetNamespace(schema);
                    if (!ImportedSchemas.Contains(targetNamespace))
                    {
                        var schemaResource = CreateSchemaResourceFromElement(schema.Element);

                        ImportedSchemas.Add(targetNamespace);
                        _schemaResources.Add(schemaResource);

                        if (definition.TargetNamespace.Equals(targetNamespace) && targetXsd == null)
                        {
                            targetXsd = schemaResource;
                        }
                        else if (targetXsd == null && firstSchemaInWsdl == null)
                        {
                            firstSchemaInWsdl = schemaResource;
                        }
                    }
                }
                else
                {
                    Log.LogWarning("Found unsupported schema type implementation {SchemaType}",
                        schemaObject.GetType().Name);
                }
            }
        }

        // Process imports
        foreach (var wsdlImport in definition.Imports)
        {
            var schemaLocation = ResolveImportLocation(wsdlImport, definition);
            var importedDefinition = GetWsdlDefinition(FileUtils.GetFileResource(schemaLocation));
            LoadSchemas(importedDefinition);
        }

        return SelectTargetXsd(targetXsd, firstSchemaInWsdl);
    }

    private string ResolveImportLocation(WsdlImport wsdlImport, WsdlDefinition definition)
    {
        var locationUri = new Uri(wsdlImport.LocationUri, UriKind.RelativeOrAbsolute);

        if (locationUri.IsAbsoluteUri)
        {
            return wsdlImport.LocationUri;
        }

        var documentBaseUri = definition.DocumentBaseUri?.Replace("\\", "/");
        if (string.IsNullOrEmpty(documentBaseUri))
        {
            throw new AgenixSystemException("Cannot resolve relative import without document base URI");
        }

        var lastSlashIndex = documentBaseUri.LastIndexOf('/');
        var basePath = lastSlashIndex >= 0 ? documentBaseUri.Substring(0, lastSlashIndex + 1) : "";

        return basePath + wsdlImport.LocationUri;
    }


    private IResource? SelectTargetXsd(IResource targetXsd, IResource firstSchemaInWsdl)
    {
        if (targetXsd != null)
        {
            return targetXsd;
        }

        // No schema resource in WSDL matched the targetNamespace,
        // use the first schema resource found as main schema
        if (firstSchemaInWsdl != null)
        {
            return firstSchemaInWsdl;
        }

        if (_schemaResources.Any())
        {
            return _schemaResources[0];
        }

        return null;
    }


    /// <summary>
    /// Loads and retrieves the WSDL definition from the provided resource.
    /// </summary>
    /// <param name="wsdl">
    /// The resource object referencing the WSDL file to be read. This includes properties such as the URI and input stream.
    /// </param>
    /// <returns>
    /// A <see cref="WsdlDefinition"/> object representing the target WSDL definition, including namespaces, base URI, and target namespace.
    /// </returns>
    /// <exception cref="AgenixSystemException">
    /// Thrown when the WSDL schema instance cannot be loaded or parsed due to an error.
    /// </exception>
    private WsdlDefinition GetWsdlDefinition(IResource wsdl)
    {
        try
        {
            WsdlDefinition definition;

            if (wsdl.Uri.ToString().StartsWith("jar:", StringComparison.OrdinalIgnoreCase))
            {
                // Locate WSDL imports
                var wsdlReader = WsdlFactory.NewInstance().NewWsdlReader();
                definition = wsdlReader.ReadWsdl(new WsdlLocator(wsdl));
            }
            else
            {
                var wsdlReader = WsdlFactory.NewInstance().NewWsdlReader();
                using var inputStream = wsdl.InputStream;
                definition = wsdlReader.ReadWsdl(wsdl.Uri.LocalPath, inputStream);
            }

            return definition;
        }
        catch (Exception e)
        {
            throw new AgenixSystemException("Failed to load WSDL schema instance", e);
        }
    }

    /// <summary>
    /// Loads schema resources from the provided WSDL file resource.
    /// </summary>
    /// <returns>
    /// The loaded schema resources as an <see cref="IResource"/> object.
    /// </returns>
    protected override IResource LoadSchemaResources()
    {
        ObjectHelper.AssertNotNull(_wsdl, "wsdl file resource is required");

        if (!_wsdl.Exists) {
            throw new AgenixSystemException("wsdl file resource '" + _wsdl + " does not exist");
        }

        try {
            return LoadSchemas(GetWsdlDefinition(_wsdl));
        } catch (Exception e) {
            throw new AgenixSystemException("Failed to load schema types from WSDL file", e);
        }
    }
}
