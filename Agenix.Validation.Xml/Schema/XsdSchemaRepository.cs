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


using System.Text.RegularExpressions;
using System.Xml;
using Agenix.Api.IO;
using Agenix.Api.Log;
using Agenix.Core.Repository;
using Agenix.Core.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Xml.Schema;

public class XsdSchemaRepository() : BaseRepository(DefaultName)
{
    private const string DefaultName = "schemaRepository";

    /// <summary>
    ///     Logger
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(XsdSchemaRepository));

    /// <summary>
    ///     Mapping strategy
    /// </summary>
    private IXsdSchemaMappingStrategy _schemaMappingStrategy = new TargetNamespaceSchemaMappingStrategy();

    /// <summary>
    ///     List of schema resources
    /// </summary>
    private List<IXsdSchema> _schemas = [];

    /// <summary>
    ///     Find the matching schema for document using given schema mapping strategy.
    /// </summary>
    /// <param name="doc">The document instance to validate.</param>
    /// <returns>Boolean flag marking matching schema instance found</returns>
    public bool CanValidate(XmlDocument doc)
    {
        var schema = _schemaMappingStrategy.GetSchema(_schemas, doc);
        return schema != null;
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    /// <summary>
    ///     Adds a resource to the repository, processing it based on its file type.
    /// </summary>
    /// <param name="resource">The resource instance to be added to the repository, typically representing a file.</param>
    protected override void AddRepository(IResource resource)
    {
        var resourcePath = ExtractResourceName(resource);

        if (IsXsdResource(resourcePath))
        {
            AddXsdSchema(resource);
        }
        else if (IsWsdlResource(resourcePath))
        {
            AddWsdlSchema(resource);
        }
        else
        {
            Log.LogWarning("Skipped resource other than XSD/WSDL schema for repository '{ResourcePath}'", resourcePath);
        }
    }

    private string ExtractResourceName(IResource resource)
    {
        var description = resource.Description;

        if (string.IsNullOrEmpty(description))
        {
            return string.Empty;
        }

        // Use regex to extract the resource name from the assembly resource format
        var match = Regex.Match(
            description,
            @"resource\s*\[([^\]]+)\]",
            RegexOptions.IgnoreCase);

        if (match.Success)
        {
            var resourcePath = match.Groups[1].Value;

            // Handle dot-separated resource names
            // "Agenix.Validation.Xml.Tests.Resources.Validation.TestService.wsdl"
            if (resourcePath.Contains('.'))
            {
                var parts = resourcePath.Split('.');
                if (parts.Length >= 2)
                {
                    // Reconstruct the filename from the last parts
                    var extension = parts[^1];
                    var nameWithoutExt = parts[^2];
                    return $"{nameWithoutExt}.{extension}";
                }
            }

            return resourcePath;
        }

        // Fallback for non-assembly resources
        return resource.Uri?.ToString() ?? description;
    }


    private bool IsXsdResource(string resourcePath)
    {
        return !string.IsNullOrEmpty(resourcePath) &&
               resourcePath.EndsWith(".xsd", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsWsdlResource(string resourcePath)
    {
        return !string.IsNullOrEmpty(resourcePath) &&
               resourcePath.EndsWith(".wsdl", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Loads a WSDL schema resource, initializes it, and adds its schema to the internal schema collection.
    /// </summary>
    /// <param name="resource">The WSDL resource to load and process into an XML schema.</param>
    private void AddWsdlSchema(IResource resource)
    {
        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug("Loading WSDL schema resource '{Location}'", resource.Description);
        }

        var wsdl = new WsdlXsdSchema(resource);
        wsdl.Initialize();
        _schemas.Add(wsdl);
    }

    /// <summary>
    ///     Adds an XSD schema to the repository using the provided resource.
    /// </summary>
    /// <param name="resource">The resource containing the XSD schema to be loaded into the repository.</param>
    private void AddXsdSchema(IResource resource)
    {
        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug("Loading XSD schema resource '{Location}'", resource.Description);
        }

        var schema = new SimpleXsdSchema(new ByteArrayResource(FileUtils.CopyToByteArray(resource)));
        schema.AfterPropertiesSet();
        _schemas.Add(schema);
    }

    /// <summary>
    ///     Get the list of known schemas.
    /// </summary>
    /// <returns>The schema sources</returns>
    public List<IXsdSchema> GetSchemas()
    {
        return _schemas;
    }

    /// <summary>
    ///     Set the list of known schemas.
    /// </summary>
    /// <param name="schemas">The schemas to set</param>
    public void SetSchemas(List<IXsdSchema> schemas)
    {
        _schemas = schemas ?? throw new ArgumentNullException(nameof(schemas));
    }

    /// <summary>
    ///     Set the schema mapping strategy.
    /// </summary>
    /// <param name="schemaMappingStrategy">The schema mapping strategy to set</param>
    public void SetSchemaMappingStrategy(IXsdSchemaMappingStrategy schemaMappingStrategy)
    {
        _schemaMappingStrategy =
            schemaMappingStrategy ?? throw new ArgumentNullException(nameof(schemaMappingStrategy));
    }

    /// <summary>
    ///     Gets the schema mapping strategy.
    /// </summary>
    /// <returns>The current XsdSchemaMappingStrategy</returns>
    public IXsdSchemaMappingStrategy GetSchemaMappingStrategy()
    {
        return _schemaMappingStrategy;
    }
}
