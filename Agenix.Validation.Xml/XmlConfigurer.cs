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

using System.Text;
using System.Xml;
using Agenix.Api.Common;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Validation.Xml.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Xml;

/// <summary>
///     Class is loaded with DI container in Agenix. When loaded automatically initializes XML utilities
///     with this XML processing configuration. Configuration is pushed to XML utility classes after properties are set.
/// </summary>
public class XmlConfigurer : InitializingPhase
{
    // Configuration parameter constants
    public const string SplitCdataSections = "split-cdata-sections";
    public const string FormatPrettyPrint = "format-pretty-print";
    public const string ElementContentWhitespace = "element-content-whitespace";
    public const string CdataSections = "cdata-sections";
    public const string ValidateIfSchema = "validate-if-schema";
    public const string ResourceResolver = "resource-resolver";
    public const string XmlDeclaration = "xml-declaration";
    public const string Indent = "indent";
    public const string IndentChars = "indent-chars";
    public const string OmitXmlDeclaration = "omit-xml-declaration";
    public const string Encoding = "encoding";
    public const string NewLineHandling = "new-line-handling";
    public const string NewLineChars = "new-line-chars";
    public const string CheckCharacters = "check-characters";
    public const string ConformanceLevel = "conformance-level";
    public const string DtdProcessing = "dtd-processing";
    public const string MaxCharactersInDocument = "max-characters-in-document";
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(XmlConfigurer));

    /// <summary>
    ///     XML reader factory for creating configured readers
    /// </summary>
    private readonly XmlReaderFactory _readerFactory;

    /// <summary>
    ///     XML writer factory for creating configured writers
    /// </summary>
    private readonly XmlWriterFactory _writerFactory;

    /// <summary>Default parser and serializer settings</summary>
    private Dictionary<string, object> _parseSettings = new();

    private Dictionary<string, object> _serializeSettings = new();

    public XmlConfigurer()
    {
        try
        {
            _readerFactory = new XmlReaderFactory();
            _writerFactory = new XmlWriterFactory();

            Logger.LogDebug("XML Configurer initialized with .NET XML processing capabilities");

            SetDefaultParseSettings();
            SetDefaultSerializeSettings();
        }
        catch (Exception e)
        {
            throw new AgenixSystemException("Failed to initialize XML configurer", e);
        }
    }

    /// <summary>
    ///     Initialize the XML configurer and register with XML utilities.
    /// </summary>
    public void Initialize()
    {
        SetDefaultParseSettings();
        SetDefaultSerializeSettings();

        // Register this configurer with XML utilities
        XmlUtils.Initialize(this);

        Logger.LogInformation(
            "XML Configurer initialized with {ParseSettings} parse settings and {SerializeSettings} serialize settings",
            _parseSettings.Count, _serializeSettings.Count);
    }

    /// <summary>
    ///     Creates configured XmlReader instance with common properties and configuration parameters.
    /// </summary>
    public XmlReader CreateXmlReader(Stream input)
    {
        var settings = CreateXmlReaderSettings();
        return XmlReader.Create(input, settings);
    }

    /// <summary>
    ///     Creates configured XmlReader instance from string input.
    /// </summary>
    public XmlReader CreateXmlReader(string xmlContent)
    {
        var settings = CreateXmlReaderSettings();
        return XmlReader.Create(new StringReader(xmlContent), settings);
    }

    /// <summary>
    ///     Creates configured XmlReader instance from TextReader.
    /// </summary>
    public XmlReader CreateXmlReader(TextReader input)
    {
        var settings = CreateXmlReaderSettings();
        return XmlReader.Create(input, settings);
    }

    /// <summary>
    ///     Creates XmlReaderSettings based on parser configuration.
    /// </summary>
    public XmlReaderSettings CreateXmlReaderSettings()
    {
        var settings = new XmlReaderSettings();
        ConfigureReaderSettings(settings);
        return settings;
    }

    /// <summary>
    ///     Creates configured XmlWriter instance with common properties and configuration parameters.
    /// </summary>
    public XmlWriter CreateXmlWriter(Stream output)
    {
        var settings = CreateXmlWriterSettings();
        var textWriter = new StreamWriter(output, settings.Encoding);
        return XmlWriter.Create(textWriter, settings);
    }

    /// <summary>
    ///     Creates configured XmlWriter instance that writes to TextWriter.
    /// </summary>
    public XmlWriter CreateXmlWriter(TextWriter output)
    {
        var settings = CreateXmlWriterSettings();
        return XmlWriter.Create(output, settings);
    }

    /// <summary>
    ///     Creates configured XmlWriter instance that writes to StringBuilder.
    /// </summary>
    public XmlWriter CreateXmlWriter(StringBuilder output)
    {
        var settings = CreateXmlWriterSettings();
        return XmlWriter.Create(output, settings);
    }

    /// <summary>
    ///     Creates XmlWriterSettings based on serializer configuration.
    /// </summary>
    public XmlWriterSettings CreateXmlWriterSettings()
    {
        var settings = new XmlWriterSettings();
        ConfigureWriterSettings(settings);
        return settings;
    }

    /// <summary>
    ///     Creates XmlDocument with configured settings.
    /// </summary>
    public XmlDocument CreateXmlDocument()
    {
        var document = new XmlDocument();
        ConfigureXmlDocument(document);
        return document;
    }

    /// <summary>
    ///     Creates XmlNameTable for namespace management.
    /// </summary>
    public XmlNameTable CreateXmlNameTable()
    {
        return new NameTable();
    }

    /// <summary>
    ///     Creates XmlResolver for resource resolution.
    /// </summary>
    public XmlResolver CreateXmlResolver()
    {
        return new XmlUrlResolver();
    }

    /// <summary>
    ///     Set reader configuration based on parse settings.
    /// </summary>
    protected void ConfigureReaderSettings(XmlReaderSettings settings)
    {
        foreach (var setting in _parseSettings)
        {
            SetReaderConfigParameter(settings, setting.Key, setting.Value);
        }
    }

    /// <summary>
    ///     Set writer configuration based on serialize settings.
    /// </summary>
    protected void ConfigureWriterSettings(XmlWriterSettings settings)
    {
        foreach (var setting in _serializeSettings)
        {
            SetWriterConfigParameter(settings, setting.Key, setting.Value);
        }
    }

    /// <summary>
    ///     Configure XmlDocument based on settings.
    /// </summary>
    protected void ConfigureXmlDocument(XmlDocument document)
    {
        // Apply document-specific settings
        if (_parseSettings.TryGetValue(ElementContentWhitespace, out var preserveWhitespace))
        {
            document.PreserveWhitespace = Convert.ToBoolean(preserveWhitespace);
        }

        if (_parseSettings.TryGetValue(ResourceResolver, out var resolver) && resolver is XmlResolver xmlResolver)
        {
            document.XmlResolver = xmlResolver;
        }
    }

    /// <summary>
    ///     Sets a config parameter on XmlReaderSettings if applicable.
    /// </summary>
    public static void SetReaderConfigParameter(XmlReaderSettings settings, string parameterName, object value)
    {
        try
        {
            switch (parameterName.ToLowerInvariant())
            {
                case CdataSections:
                case SplitCdataSections:
                    // .NET XmlReader handles CDATA automatically
                    break;

                case ValidateIfSchema:
                    if (Convert.ToBoolean(value))
                    {
                        settings.ValidationType = ValidationType.Schema;
                    }

                    break;

                case ElementContentWhitespace:
                    settings.IgnoreWhitespace = !Convert.ToBoolean(value);
                    break;

                case ResourceResolver:
                    if (value is XmlResolver resolver)
                    {
                        settings.XmlResolver = resolver;
                    }

                    break;

                case CheckCharacters:
                    settings.CheckCharacters = Convert.ToBoolean(value);
                    break;

                case ConformanceLevel:
                    if (Enum.TryParse<ConformanceLevel>(value.ToString(), true, out var level))
                    {
                        settings.ConformanceLevel = level;
                    }

                    break;

                // Add DTD processing configuration
                case DtdProcessing:
                    if (Enum.TryParse<DtdProcessing>(value.ToString(), true, out var dtdProcessing))
                    {
                        settings.DtdProcessing = dtdProcessing;
                        // For security, set XmlResolver to null when allowing DTD processing
                        // to prevent external entity resolution
                        if (dtdProcessing != System.Xml.DtdProcessing.Prohibit)
                        {
                            settings.XmlResolver = null;
                        }
                    }

                    break;

                case MaxCharactersInDocument:
                    if (long.TryParse(value.ToString(), out var maxChars) && maxChars > 0)
                    {
                        settings.MaxCharactersInDocument = maxChars;
                    }

                    break;


                default:
                    Logger.LogWarning("Unknown or unsupported reader parameter: {Parameter}", parameterName);
                    break;
            }
        }
        catch (Exception ex)
        {
            LogParameterNotSet(parameterName, "XmlReader", ex);
        }
    }

    /// <summary>
    ///     Sets a config parameter on XmlWriterSettings if applicable.
    /// </summary>
    public static void SetWriterConfigParameter(XmlWriterSettings settings, string parameterName, object value)
    {
        try
        {
            switch (parameterName.ToLowerInvariant())
            {
                case FormatPrettyPrint:
                case Indent:
                    settings.Indent = Convert.ToBoolean(value);
                    break;

                case IndentChars:
                    settings.IndentChars = value?.ToString() ?? "  ";
                    break;

                case XmlDeclaration:
                    settings.OmitXmlDeclaration = !Convert.ToBoolean(value);
                    break;

                case OmitXmlDeclaration:
                    settings.OmitXmlDeclaration = Convert.ToBoolean(value);
                    break;

                case Encoding:
                    settings.Encoding = value switch
                    {
                        Encoding encoding => encoding,
                        string encodingName => System.Text.Encoding.GetEncoding(encodingName),
                        _ => settings.Encoding
                    };
                    break;

                case NewLineHandling:
                    if (Enum.TryParse<NewLineHandling>(value.ToString(), true, out var newLineHandling))
                    {
                        settings.NewLineHandling = newLineHandling;
                    }

                    break;

                case NewLineChars:
                    settings.NewLineChars = value?.ToString() ?? Environment.NewLine;
                    break;

                case CheckCharacters:
                    settings.CheckCharacters = Convert.ToBoolean(value);
                    break;

                case ConformanceLevel:
                    if (Enum.TryParse<ConformanceLevel>(value.ToString(), true, out var level))
                    {
                        settings.ConformanceLevel = level;
                    }

                    break;

                default:
                    Logger.LogWarning("Unknown or unsupported writer parameter: {Parameter}", parameterName);
                    break;
            }
        }
        catch (Exception ex)
        {
            LogParameterNotSet(parameterName, "XmlWriter", ex);
        }
    }


    /// <summary>
    ///     Logging that parameter was not set on component.
    /// </summary>
    private static void LogParameterNotSet(string parameterName, string componentName, Exception? ex = null)
    {
        if (ex != null)
        {
            Logger.LogWarning(ex, "Unable to set '{Parameter}' parameter on {Component}", parameterName, componentName);
        }
        else
        {
            Logger.LogWarning("Unable to set '{Parameter}' parameter on {Component}", parameterName, componentName);
        }
    }

    /// <summary>
    ///     Sets the default parse settings.
    /// </summary>
    private void SetDefaultParseSettings()
    {
        _parseSettings.TryAdd(CdataSections, true);

        _parseSettings.TryAdd(SplitCdataSections, false);


        _parseSettings.TryAdd(ValidateIfSchema, true);

        if (!_parseSettings.ContainsKey(ResourceResolver))
        {
            _parseSettings[ResourceResolver] = CreateXmlResolver();
        }

        _parseSettings.TryAdd(ElementContentWhitespace, false);

        // Add DTD processing configuration - set to Parse to allow DTDs
        _parseSettings.TryAdd(DtdProcessing, nameof(System.Xml.DtdProcessing.Parse));

        // Set a reasonable limit for document size when allowing DTD processing
        _parseSettings.TryAdd(MaxCharactersInDocument, 10000000L); // 10MB limit
    }

    /// <summary>
    ///     Sets the default serialize settings.
    /// </summary>
    private void SetDefaultSerializeSettings()
    {
        _serializeSettings.TryAdd(FormatPrettyPrint, true);

        _serializeSettings.TryAdd(XmlDeclaration, true);

        _serializeSettings.TryAdd(Indent, true);

        _serializeSettings.TryAdd(IndentChars, "  ");
    }

    /// <summary>
    ///     Sets the parseSettings property.
    /// </summary>
    public void SetParseSettings(Dictionary<string, object>? parseSettings)
    {
        _parseSettings = parseSettings ?? new Dictionary<string, object>();
    }

    /// <summary>
    ///     Gets the parseSettings property.
    /// </summary>
    public Dictionary<string, object> GetParseSettings()
    {
        return _parseSettings;
    }

    /// <summary>
    ///     Sets the serializeSettings property.
    /// </summary>
    public void SetSerializeSettings(Dictionary<string, object>? serializeSettings)
    {
        _serializeSettings = serializeSettings ?? new Dictionary<string, object>();
    }

    /// <summary>
    ///     Gets the serializeSettings property.
    /// </summary>
    public Dictionary<string, object> GetSerializeSettings()
    {
        return _serializeSettings;
    }

    /// <summary>
    ///     Add or update a parse setting.
    /// </summary>
    public void AddParseSetting(string key, object value)
    {
        _parseSettings[key] = value;
    }

    /// <summary>
    ///     Add or update a serialize setting.
    /// </summary>
    public void AddSerializeSetting(string key, object value)
    {
        _serializeSettings[key] = value;
    }

    private class UpperCaseEncoding(Encoding encoding) : UTF8Encoding(false)
    {
        public override string BodyName => encoding.BodyName.ToUpperInvariant(); // Used in XML declaration
        public override string WebName => encoding.WebName.ToUpperInvariant();
        public override string HeaderName => encoding.HeaderName.ToUpperInvariant();
    }
}
