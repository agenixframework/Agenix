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
using Agenix.Api.Common;
using Agenix.Api.Exceptions;
using Agenix.Api.IO;
using Agenix.Core.Util;

namespace Agenix.Validation.Xml.Schema;

public abstract class AbstractSchemaCollection : SimpleXsdSchema, InitializingPhase
{
    /// <summary>Official xmlns namespace</summary>
    public const string WwwW3Org2000Xmlns = "http://www.w3.org/2000/xmlns/";

    public const string W3CXmlSchemaNsUri = "http://www.w3.org/2001/XMLSchema";

    /// <summary>List of schema resources</summary>
    protected readonly List<IResource> _schemaResources = [];

    /// <summary>Imported schemas</summary>
    protected readonly List<string> ImportedSchemas = [];


    /// <summary>
    ///     Gets the list of schema resources associated with the collection.
    /// </summary>
    public List<IResource> SchemaResources => _schemaResources;

    public void Initialize()
    {
        var targetXsd = LoadSchemaResources();
        if (targetXsd == null)
        {
            throw new AgenixSystemException("Failed to find target schema xsd file resource");
        }

        if (_schemaResources.Count == 0)
        {
            throw new AgenixSystemException("At least one schema xsd file resource is required");
        }

        SetXsd(new ByteArrayResource(FileUtils.CopyToByteArray(targetXsd)));

        AfterPropertiesSet();
    }


    /// <summary>
    ///     Creates and returns an instance of <c>XmlSchemaValidator</c> for validating XML documents against a set of XML
    ///     Schema Definitions (XSDs).
    /// </summary>
    /// <returns>An initialized <c>XmlSchemaValidator</c> instance configured with compiled schema definitions.</returns>
    /// <exception cref="AgenixSystemException">
    ///     Thrown when there is an issue creating the validator due to IO errors or invalid XML schema definitions.
    /// </exception>
    public virtual XmlSchemaValidator CreateValidator()
    {
        try
        {
            var schemaSet = new XmlSchemaSet();

            foreach (var resource in _schemaResources)
            {
                using var stream = resource.InputStream;
                var schema = XmlSchema.Read(stream, ValidationEventHandler);
                schemaSet.Add(schema);
            }

            schemaSet.Compile();

            return new XmlSchemaValidator(schemaSet);
        }
        catch (IOException e)
        {
            throw new AgenixSystemException("Failed to create validator from multi resource schema files", e);
        }
        catch (XmlException e)
        {
            throw new AgenixSystemException("Failed to create validator from multi resource schema files", e);
        }
    }

    /// <summary>
    ///     Handles XML schema validation events during schema parsing or validation processes.
    /// </summary>
    /// <param name="sender">The source of the validation event, typically an <c>XmlReader</c> or <c>XmlSchemaSet</c>.</param>
    /// <param name="e">The <c>ValidationEventArgs</c> containing details about the validation error or warning.</param>
    /// <exception cref="XmlException">
    ///     Thrown when a validation error of severity <c>Error</c> occurs.
    /// </exception>
    private static void ValidationEventHandler(object sender, ValidationEventArgs e)
    {
        // Handle validation events
        if (e.Severity == XmlSeverityType.Error)
        {
            throw new XmlException($"Schema validation error: {e.Message}");
        }
    }

    /// <summary>
    ///     Recursively add all included schemas as schema resource.
    /// </summary>
    protected void AddIncludedSchemas(XmlSchema schema)
    {
        foreach (var o in schema.Includes)
        {
            var include = (XmlSchemaExternal)o;
            if (include is XmlSchemaInclude schemaInclude)
            {
                string schemaLocation;

                if (Uri.IsWellFormedUriString(schemaInclude.SchemaLocation, UriKind.Absolute))
                {
                    schemaLocation = schemaInclude.SchemaLocation;
                }
                else
                {
                    // Get the base URI from the schema's source URI
                    var baseUri = schema.SourceUri ?? "";
                    var lastSlashIndex = baseUri.LastIndexOf('/');
                    if (lastSlashIndex >= 0)
                    {
                        schemaLocation = baseUri.Substring(0, lastSlashIndex + 1) + schemaInclude.SchemaLocation;
                    }
                    else
                    {
                        schemaLocation = schemaInclude.SchemaLocation;
                    }
                }

                _schemaResources.Add(FileUtils.GetFileResource(schemaLocation));
            }
        }
    }

    /// <summary>
    ///     Recursively add all imported schemas as schema resource.
    ///     This is necessary when schema imports are located in assemblies. If they are not added immediately the reference to
    ///     them is lost.
    /// </summary>
    /// <param name="schema">The XML schema to process for imports</param>
    protected void AddImportedSchemas(XmlSchema schema)
    {
        foreach (XmlSchemaExternal external in schema.Includes)
        {
            if (external is XmlSchemaImport schemaImport)
            {
                // Prevent duplicate imports
                if (!ImportedSchemas.Contains(schemaImport.Namespace))
                {
                    ImportedSchemas.Add(schemaImport.Namespace);
                    var referencedSchema = schemaImport.Schema;

                    if (referencedSchema != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            // Write the schema to a memory stream
                            referencedSchema.Write(memoryStream);

                            // Create a resource from a byte array
                            IResource schemaResource = new ByteArrayResource(memoryStream.ToArray());

                            // Recursively add imported schemas
                            AddImportedSchemas(referencedSchema);
                            _schemaResources.Add(schemaResource);
                        }
                    }
                }
            }
        }
    }


    /// <summary>
    ///     Loads all schema resource files from schema locations.
    /// </summary>
    /// <returns></returns>
    protected abstract IResource LoadSchemaResources();
}
