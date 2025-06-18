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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Agenix.Api.Exceptions;
using Agenix.Api.IO;
using Agenix.Api.Log;
using Agenix.Api.Xml;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Xml;

/// <summary>
///     Marshaller uses XML serialization to marshal/unmarshal data.
///     Provides similar functionality to Java's JAXB2Marshaller.
/// </summary>
public class Xml2Marshaller : IMarshaller, IUnmarshaller
{
    private static readonly ILogger Log = LogManager.GetLogger(typeof(Xml2Marshaller));
    private static readonly Type[] TypeArray = [];
    private readonly string[] _contextPaths;
    private readonly object _lockObject = new();

    private readonly Dictionary<string, object> _marshallerProperties = new();
    private readonly XmlSchema[] _schemas;
    private readonly Type[] _typesToBeBound;

    private volatile XmlSerializerNamespaces _namespaces;

    /// <summary>
    ///     Default constructor with no bound types
    /// </summary>
    public Xml2Marshaller() : this(TypeArray)
    {
    }

    /// <summary>
    ///     Constructor with types to be bound
    /// </summary>
    public Xml2Marshaller(params Type[] typesToBeBound)
    {
        _typesToBeBound = typesToBeBound;
        _contextPaths = null;
        _schemas = null;
    }

    /// <summary>
    ///     Constructor with context paths (assembly names or namespaces)
    /// </summary>
    public Xml2Marshaller(params string[] contextPaths)
    {
        _typesToBeBound = null;
        _contextPaths = contextPaths;
        _schemas = null;
    }

    /// <summary>
    ///     Constructor with schema resource and bound types
    /// </summary>
    public Xml2Marshaller(IResource schemaResource, params Type[] typesToBeBound)
    {
        _typesToBeBound = typesToBeBound;
        _contextPaths = null;
        _schemas = LoadSchemas(schemaResource);
    }

    /// <summary>
    ///     Constructor with schema resource and context paths
    /// </summary>
    public Xml2Marshaller(IResource schemaResource, params string[] contextPaths)
    {
        _typesToBeBound = null;
        _contextPaths = contextPaths;
        _schemas = LoadSchemas(schemaResource);
    }

    /// <summary>
    ///     Constructor with multiple schema resources and bound types
    /// </summary>
    public Xml2Marshaller(IResource[] schemaResources, params Type[] typesToBeBound)
    {
        _typesToBeBound = typesToBeBound;
        _contextPaths = null;
        _schemas = LoadSchemas(schemaResources);
    }

    /// <summary>
    ///     Constructor with multiple schema resources and context paths
    /// </summary>
    public Xml2Marshaller(IResource[] schemaResources, params string[] contextPaths)
    {
        _typesToBeBound = null;
        _contextPaths = contextPaths;
        _schemas = LoadSchemas(schemaResources);
    }

    /// <summary>
    ///     Marshal object to XML result
    /// </summary>
    public void Marshal(object graph, StringWriter result)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(result);

        try
        {
            var serializer = CreateSerializer(graph.GetType());

            // Check if we need post-processing
            if (RequiresPostProcessing())
            {
                // Serialize to string first for post-processing
                using var stringWriter = new StringWriter();
                using var xmlWriter = XmlWriter.Create(stringWriter, CreateXmlWriterSettings());
                serializer.Serialize(xmlWriter, graph, _namespaces);

                var xmlString = stringWriter.ToString();
                var processedXml = PostProcessXmlOutput(xmlString);

                result.Write(processedXml);
            }
            else
            {
                // Direct serialization without post-processing
                var settings = new XmlWriterSettings();
                ApplyMarshallerProperties(settings);

                using var xmlWriter = XmlWriter.Create(result, settings);
                serializer.Serialize(xmlWriter, graph, _namespaces);
            }
        }
        catch (Exception ex)
        {
            throw new MarshallingException($"Failed to marshal object of type {graph.GetType().Name}", ex);
        }
    }


    /// <summary>
    ///     Unmarshal from XML reader
    /// </summary>
    public object Unmarshal(XmlReader source)
    {
        ArgumentNullException.ThrowIfNull(source);

        try
        {
            // Try to determine the type from the root element
            var rootElementType = DetermineTypeFromXml(source);
            var serializer = CreateSerializer(rootElementType);

            // Validate against schema if available
            if (_schemas?.Length > 0)
            {
                ValidateAgainstSchema(source);
            }

            return serializer.Deserialize(source);
        }
        catch (Exception ex)
        {
            throw new UnmarshallingException("Failed to unmarshal XML", ex);
        }
    }


    /// <summary>
    ///     Determines if the current marshaler properties require post-processing
    /// </summary>
    private bool RequiresPostProcessing()
    {
        lock (_lockObject)
        {
            return _marshallerProperties.Any(prop => IsPostProcessingProperty(prop.Key));
        }
    }


    /// <summary>
    ///     Checks if a property requires post-processing
    /// </summary>
    private static bool IsPostProcessingProperty(string propertyKey)
    {
        return propertyKey.ToLowerInvariant() switch
        {
            "omitxmldeclaration" => true,
            "jaxb.fragment" => true,
            "stripnamespaces" => true,
            "removeemptylines" => true,
            // Add more properties that need post-processing here
            _ => false
        };
    }


    /// <summary>
    ///     Applies post-processing transformations to XML output based on marshaler properties
    /// </summary>
    private string PostProcessXmlOutput(string xmlOutput)
    {
        lock (_lockObject)
        {
            return _marshallerProperties.Aggregate(xmlOutput,
                (current, property) => ApplyPostProcessingProperty(current, property.Key, property.Value));
        }
    }

    /// <summary>
    ///     Applies a single post-processing property transformation
    /// </summary>
    private string ApplyPostProcessingProperty(string xml, string propertyKey, object propertyValue)
    {
        try
        {
            return propertyKey.ToLowerInvariant() switch
            {
                "omitxmldeclaration" or "jaxb.fragment" when Convert.ToBoolean(propertyValue) =>
                    RemoveXmlDeclaration(xml),

                "stripnamespaces" when Convert.ToBoolean(propertyValue) =>
                    RemoveNamespaceDeclarations(xml),

                "removeemptylines" when Convert.ToBoolean(propertyValue) =>
                    RemoveEmptyLines(xml),

                "trimwhitespace" when Convert.ToBoolean(propertyValue) =>
                    xml.Trim(),

                "removecomments" when Convert.ToBoolean(propertyValue) =>
                    RemoveXmlComments(xml),

                "normalizewhitespace" when Convert.ToBoolean(propertyValue) =>
                    NormalizeWhitespace(xml),

                // Add more post-processing transformations here
                _ => xml
            };
        }
        catch (Exception ex)
        {
            Log.LogWarning(ex, "Failed to apply post-processing for property {Key}={Value}", propertyKey,
                propertyValue);
            return xml;
        }
    }

    /// <summary>
    ///     Removes XML declaration from the beginning of XML string
    /// </summary>
    private static string RemoveXmlDeclaration(string xml)
    {
        if (xml.StartsWith("<?xml"))
        {
            var declarationEnd = xml.IndexOf("?>") + 2;
            return xml.Substring(declarationEnd).TrimStart('\r', '\n', ' ', '\t');
        }

        return xml;
    }

    /// <summary>
    ///     Removes namespace declarations from XML elements
    /// </summary>
    private static string RemoveNamespaceDeclarations(string xml)
    {
        // Remove xmlns declarations
        return Regex.Replace(xml,
            @"\s+xmlns(:[^=]*)?=""[^""]*""",
            "",
            RegexOptions.IgnoreCase);
    }

    /// <summary>
    ///     Removes empty lines from XML
    /// </summary>
    private static string RemoveEmptyLines(string xml)
    {
        // Remove whitespace between XML elements (including newlines, spaces, tabs)
        xml = Regex.Replace(xml,
            @">\s+<",
            "><",
            RegexOptions.Multiline);

        // Remove leading/trailing whitespace
        return xml.Trim();
    }

    /// <summary>
    ///     Removes XML comments
    /// </summary>
    private static string RemoveXmlComments(string xml)
    {
        return Regex.Replace(xml,
            @"<!--.*?-->",
            "",
            RegexOptions.Singleline);
    }

    /// <summary>
    ///     Normalizes whitespace in XML
    /// </summary>
    private static string NormalizeWhitespace(string xml)
    {
        // Replace multiple whitespace with single space, preserve line breaks
        return Regex.Replace(xml,
            @"[ \t]+",
            " ");
    }


    /// <summary>
    ///     Marshal object to stream
    /// </summary>
    public void Marshal(object graph, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(stream);

        try
        {
            var serializer = CreateSerializer(graph.GetType());

            // Check if we need post-processing
            if (RequiresPostProcessing())
            {
                // Serialize to string first for post-processing
                using var stringWriter = new StringWriter();
                using var xmlWriter = XmlWriter.Create(stringWriter, CreateXmlWriterSettings());
                serializer.Serialize(xmlWriter, graph, _namespaces);

                var xmlString = stringWriter.ToString();
                var processedXml = PostProcessXmlOutput(xmlString);

                // Write processed XML to stream
                using var streamWriter = new StreamWriter(stream, leaveOpen: true);
                streamWriter.Write(processedXml);
            }
            else
            {
                // Direct serialization without post-processing
                var settings = new XmlWriterSettings();
                ApplyMarshallerProperties(settings);

                using var xmlWriter = XmlWriter.Create(stream, settings);
                serializer.Serialize(xmlWriter, graph, _namespaces);
            }
        }
        catch (Exception ex)
        {
            throw new MarshallingException($"Failed to marshal object of type {graph.GetType().Name}", ex);
        }
    }

    /// <summary>
    ///     Marshal object to string
    /// </summary>
    public string MarshalToString(object graph)
    {
        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, CreateXmlWriterSettings());

        var serializer = CreateSerializer(graph.GetType());
        serializer.Serialize(xmlWriter, graph, _namespaces);

        var result = stringWriter.ToString();
        return PostProcessXmlOutput(result);
    }

    /// <summary>
    ///     Unmarshal from stream
    /// </summary>
    public T Unmarshal<T>(Stream source)
    {
        using var reader = XmlReader.Create(source);
        return (T)Unmarshal(reader);
    }

    /// <summary>
    ///     Unmarshal from string
    /// </summary>
    public T UnmarshalFromString<T>(string xml)
    {
        using var stringReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(stringReader);
        return (T)Unmarshal(xmlReader);
    }

    /// <summary>
    ///     Set marshaller property
    /// </summary>
    public void SetProperty(string key, object value)
    {
        lock (_lockObject)
        {
            _marshallerProperties[key] = value;
        }
    }

    /// <summary>
    ///     Add namespace prefix mapping
    /// </summary>
    public void AddNamespace(string prefix, string uri)
    {
        _namespaces ??= new XmlSerializerNamespaces();
        _namespaces.Add(prefix, uri);
    }

    /// <summary>
    ///     Create XML serializer for the given type
    /// </summary>
    private XmlSerializer CreateSerializer(Type type)
    {
        try
        {
            if (_typesToBeBound?.Contains(type) == true || _typesToBeBound?.Length == 0)
            {
                return new XmlSerializer(type);
            }

            if (_contextPaths != null)
            {
                // Try to find the type in the specified assemblies/namespaces
                var foundType = FindTypeInContextPaths(type);
                return new XmlSerializer(foundType ?? type);
            }

            return new XmlSerializer(type);
        }
        catch (Exception ex)
        {
            throw new MarshallingException($"Failed to create serializer for type {type.Name}", ex);
        }
    }

    /// <summary>
    ///     Find type in the specified context paths
    /// </summary>
    private Type FindTypeInContextPaths(Type targetType)
    {
        if (_contextPaths == null)
        {
            return null;
        }

        foreach (var contextPath in _contextPaths)
        {
            try
            {
                // Try as an assembly name
                var assembly = Assembly.LoadFrom(contextPath);
                var foundType = assembly.GetTypes().FirstOrDefault(t => t.Name == targetType.Name);
                if (foundType != null)
                {
                    return foundType;
                }
            }
            catch
            {
                // Try as a namespace
                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.Namespace?.StartsWith(contextPath) == true)
                    .Where(t => t.Name == targetType.Name);

                var foundType = types.FirstOrDefault();
                if (foundType != null)
                {
                    return foundType;
                }
            }
        }

        return null;
    }

    /// <summary>
    ///     Determine type from XML root element
    /// </summary>
    private Type DetermineTypeFromXml(XmlReader reader)
    {
        if (_typesToBeBound?.Length > 0)
        {
            // Find a matching type based on an XML root element
            var rootElementName = reader.LocalName;
            return _typesToBeBound.FirstOrDefault(t =>
                t.Name.Equals(rootElementName, StringComparison.OrdinalIgnoreCase) ||
                GetXmlTypeName(t).Equals(rootElementName, StringComparison.OrdinalIgnoreCase)
            ) ?? _typesToBeBound[0];
        }

        return typeof(object);
    }

    /// <summary>
    ///     Get XML type name from type attributes
    /// </summary>
    private string GetXmlTypeName(Type type)
    {
        var xmlTypeAttribute = (XmlTypeAttribute)Attribute.GetCustomAttribute(type, typeof(XmlTypeAttribute));
        return xmlTypeAttribute?.TypeName ?? type.Name;
    }

    private XmlWriterSettings CreateXmlWriterSettings()
    {
        var settings = new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true, OmitXmlDeclaration = false };
        ApplyMarshallerProperties(settings);
        return settings;
    }

    /// <summary>
    ///     Applies configured marshaller properties to the provided XML writer settings.
    /// </summary>
    /// <param name="settings">The <see cref="XmlWriterSettings" /> instance where the marshaller properties will be applied.</param>
    private void ApplyMarshallerProperties(XmlWriterSettings settings)
    {
        lock (_lockObject)
        {
            foreach (var property in _marshallerProperties)
            {
                // Skip post-processing properties - they're handled separately
                if (IsPostProcessingProperty(property.Key))
                {
                    continue;
                }

                try
                {
                    switch (property.Key.ToLowerInvariant())
                    {
                        case "indent":
                        case "jaxb.formatted.output":
                            settings.Indent = Convert.ToBoolean(property.Value);
                            break;
                        case "encoding":
                        case "jaxb.encoding":
                            settings.Encoding = property.Value as Encoding ?? Encoding.UTF8;
                            break;
                        case "omitxmldeclaration":
                        case "jaxb.fragment":
                            // JAXB fragment property means omit XML declaration
                            settings.OmitXmlDeclaration = Convert.ToBoolean(property.Value);
                            break;
                        case "indentchars":
                            settings.IndentChars = property.Value?.ToString() ?? "  ";
                            break;
                        case "newlinehandling":
                            if (Enum.TryParse<NewLineHandling>(property.Value?.ToString(), true,
                                    out var newLineHandling))
                            {
                                settings.NewLineHandling = newLineHandling;
                            }

                            break;
                        case "newlinechars":
                            settings.NewLineChars = property.Value?.ToString() ?? Environment.NewLine;
                            break;
                        default:
                            Log.LogWarning("Unknown marshaller property: {Key}={Value}", property.Key, property.Value);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Log.LogWarning(ex, "Unable to set marshaller property {Key}={Value}", property.Key, property.Value);
                }
            }
        }
    }


    /// <summary>
    ///     Loads and returns an array of XML schemas from the given resource.
    /// </summary>
    /// <param name="schemaResource">The resource containing the schema to be loaded.</param>
    /// <returns>An array of <see cref="System.Xml.Schema.XmlSchema" /> objects loaded from the resource.</returns>
    private static XmlSchema[] LoadSchemas(IResource schemaResource)
    {
        try
        {
            using var stream = schemaResource.InputStream;
            return [XmlSchema.Read(stream, ValidationEventHandler)];
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to load schema from resource {ResourceName}", schemaResource.Description);
            return [];
        }
    }

    /// <summary>
    ///     Loads XML schemas from the provided resources and returns an array of XmlSchema objects.
    /// </summary>
    /// <param name="schemaResources">An array of IResource objects representing the schema files to be loaded.</param>
    /// <returns>An array of XmlSchema objects loaded from the specified resources.</returns>
    private static XmlSchema[] LoadSchemas(IResource[] schemaResources)
    {
        var schemas = new List<XmlSchema>();
        foreach (var resource in schemaResources)
        {
            try
            {
                using var stream = resource.InputStream;
                schemas.Add(XmlSchema.Read(stream, ValidationEventHandler));
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "Failed to load schema from resource {ResourceName}", resource.Description);
            }
        }

        return schemas.ToArray();
    }

    /// <summary>
    ///     Validates the XML document against the loaded XML schemas.
    ///     Throws an exception if the XML document fails validation.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader" /> containing the XML document to validate.</param>
    private void ValidateAgainstSchema(XmlReader reader)
    {
        if (_schemas?.Length == 0)
        {
            return;
        }

        try
        {
            var settings = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema, Schemas = new XmlSchemaSet()
            };

            // Add each schema individually
            foreach (var schema in _schemas)
            {
                settings.Schemas.Add(schema);
            }

            settings.ValidationEventHandler += (sender, e) =>
            {
                if (e.Severity == XmlSeverityType.Error)
                {
                    throw new ValidationException($"XML validation error: {e.Message}");
                }

                Log.LogWarning("XML validation warning: {Message}", e.Message);
            };

            var validatingReader = XmlReader.Create(reader, settings);
            while (validatingReader.Read()) { } // Read through to validate
        }
        catch (Exception ex) when (!(ex is ValidationException))
        {
            throw new ValidationException("XML schema validation failed", ex);
        }
    }

    /// <summary>
    ///     Handles XML validation events such as warnings and errors during schema validation.
    /// </summary>
    /// <param name="sender">The source of the validation event.</param>
    /// <param name="e">The <see cref="ValidationEventArgs" /> instance containing the event data.</param>
    private static void ValidationEventHandler(object sender, ValidationEventArgs e)
    {
        switch (e.Severity)
        {
            case XmlSeverityType.Warning:
                Log.LogWarning("XML validation warning: {Message}", e.Message);
                break;
            case XmlSeverityType.Error:
                Log.LogError("XML validation error: {Message}", e.Message);
                throw new XmlSchemaValidationException("XML validation error: " + e.Message, e.Exception);
            default:
                throw new AgenixSystemException("Unsupported Xml Severity Type");
        }
    }
}
