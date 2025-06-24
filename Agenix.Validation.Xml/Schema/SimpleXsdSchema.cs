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
using System.Xml.Schema;
using Agenix.Api.IO;
using Agenix.Api.Util;
using Agenix.Validation.Xml.Namespace;

namespace Agenix.Validation.Xml.Schema;

/// <summary>
///     C# equivalent of Spring's SimpleXsdSchema
///     Represents a simple XSD schema wrapper
/// </summary>
public class SimpleXsdSchema : IXsdSchema
{
    private static readonly string SchemaNamespace = "http://www.w3.org/2001/XMLSchema";

    private static readonly QName SchemaName = new(SchemaNamespace, "schema", "xsd");
    private readonly object _lock = new();
    private IResource _resource;
    private XmlSchema? _schema;
    private XmlElement schemaElement;

    public SimpleXsdSchema()
    {
    }


    public SimpleXsdSchema(IResource resource)
    {
        _resource = resource ?? throw new ArgumentNullException(nameof(resource));
    }

    public SimpleXsdSchema(XmlSchema schema)
    {
        _schema = schema ?? throw new ArgumentNullException(nameof(schema));
    }

    /// <summary>
    ///     Gets the target namespace of the schema
    /// </summary>
    public string? TargetNamespace
    {
        get
        {
            EnsureSchemaLoaded();
            return _schema?.TargetNamespace;
        }
    }

    /// <summary>
    ///     Gets the underlying XmlSchema
    /// </summary>
    public XmlSchema Schema
    {
        get
        {
            EnsureSchemaLoaded();
            return _schema ?? throw new InvalidOperationException("Schema could not be loaded");
        }
    }

    /// <summary>
    ///     Gets the schema source (if loaded from resource)
    /// </summary>
    public XmlDocument? Source
    {
        get
        {
            if (_resource == null)
            {
                return null;
            }

            try
            {
                using var stream = _resource.InputStream;
                var doc = new XmlDocument();
                doc.Load(stream);
                return doc;
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    ///     Creates a new validator for this schema
    /// </summary>
    /// <returns>XmlSchemaValidator instance</returns>
    public XmlSchemaValidator CreateValidator()
    {
        EnsureSchemaLoaded();

        var schemaSet = new XmlSchemaSet();
        schemaSet.Add(_schema);
        schemaSet.Compile();

        return new XmlSchemaValidator(schemaSet);
    }

    /// <summary>
    ///     Validates an XML document against this schema
    /// </summary>
    /// <param name="document">The XML document to validate</param>
    /// <returns>List of validation errors (empty if valid)</returns>
    public List<string> Validate(XmlDocument document)
    {
        var errors = new List<string>();

        var schemaSet = new XmlSchemaSet();
        schemaSet.Add(Schema);
        schemaSet.Compile();

        document.Schemas = schemaSet;
        document.Validate((sender, e) =>
        {
            errors.Add($"{e.Severity}: {e.Message}");
        });

        return errors;
    }

    public void SetXsd(IResource xsdResource)
    {
        _resource = xsdResource;
    }

    private void EnsureSchemaLoaded()
    {
        if (_schema != null)
        {
            return;
        }

        lock (_lock)
        {
            if (_schema != null)
            {
                return;
            }

            if (_resource == null)
            {
                throw new InvalidOperationException("No resource or schema provided");
            }

            using var stream = _resource.InputStream;
            _schema = XmlSchema.Read(stream, ValidationEventHandler);

            if (_schema == null)
            {
                throw new InvalidOperationException($"Could not load schema from resource: {_resource}");
            }
        }
    }

    private static void ValidationEventHandler(object? sender, ValidationEventArgs e)
    {
        // Log validation events during schema loading
        // You can customize this based on your logging framework
        Console.WriteLine($"Schema Load Event: {e.Severity} - {e.Message}");
    }

    public override string ToString()
    {
        return $"SimpleXsdSchema[targetNamespace='{TargetNamespace}']";
    }

    /// <summary>
    ///     Initializes the schema after properties are set. Validates the XSD resource and loads the schema.
    /// </summary>
    /// <exception cref="XmlException">Thrown when XML parsing fails</exception>
    /// <exception cref="IOException">Thrown when file I/O operations fail</exception>
    /// <exception cref="ArgumentException">Thrown when validation assertions fail</exception>
    public void AfterPropertiesSet()
    {
        AssertUtils.ArgumentNotNull(_resource, "'xsd' is required");
        AssertUtils.IsTrue(_resource.Exists, $"xsd '{_resource}' does not exist");

        LoadAndValidateSchema();
    }

    /// <summary>
    ///     Loads and validates the XSD schema document in a single operation to avoid stream conflicts.
    /// </summary>
    /// <exception cref="XmlException">Thrown when XML parsing fails</exception>
    /// <exception cref="IOException">Thrown when file I/O operations fail</exception>
    /// <exception cref="ArgumentException">Thrown when schema validation fails</exception>
    private void LoadAndValidateSchema()
    {
        lock (_lock)
        {
            if (_schema != null)
            {
                return;
            }

            // Load the XML document first for validation
            var xmlDocument = new XmlDocument();
            using (var stream = _resource.InputStream)
            {
                xmlDocument.Load(stream);
            }

            schemaElement = xmlDocument.DocumentElement;

            // Add null check and better error reporting
            if (schemaElement == null)
            {
                throw new ArgumentException($"{_resource.Description} has no root element");
            }

            // Validate the root element - check both local name and namespace
            var actualLocalName = schemaElement.LocalName;
            var expectedLocalName = SchemaName.LocalPart;

            AssertUtils.IsTrue(expectedLocalName.Equals(actualLocalName),
                $"{_resource.Description} has invalid root element : [{actualLocalName}] instead of [{expectedLocalName}]");

            var actualNamespaceUri = schemaElement.NamespaceURI;
            var expectedNamespaceUri = SchemaName.NamespaceURI;

            AssertUtils.IsTrue(expectedNamespaceUri.Equals(actualNamespaceUri),
                $"{_resource.Description} has invalid root element namespace: [{actualNamespaceUri}] " +
                $"instead of [{expectedNamespaceUri}]");

            // Now load the schema from a fresh stream
            using (var stream = _resource.InputStream)
            {
                _schema = XmlSchema.Read(stream, ValidationEventHandler);
            }

            if (_schema == null)
            {
                throw new InvalidOperationException($"Could not load schema from resource: {_resource}");
            }

            // Validate target namespace after schema is loaded
            AssertUtils.ArgumentHasText(_schema.TargetNamespace, $"{_resource} has no targetNamespace");
        }
    }
}
