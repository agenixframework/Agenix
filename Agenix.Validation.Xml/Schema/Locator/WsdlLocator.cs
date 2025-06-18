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


using Agenix.Api.IO;
using Agenix.Api.Log;
using Agenix.Core.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Xml.Schema.Locator;

/// <summary>
///     Responsible for locating WSDL resources and their imports.
/// </summary>
public class WsdlLocator(IResource wsdl) : IWsdlLocator
{
    /// <summary>
    ///     Logger
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(WsdlLocator));

    private readonly IResource _wsdl = wsdl ?? throw new ArgumentNullException(nameof(wsdl));
    private IResource? _importResource;

    public Stream GetBaseInputSource()
    {
        return _wsdl.InputStream;
    }

    public Stream GetImportInputSource(string parentLocation, string importLocation)
    {
        var resolvedImportLocation = ResolveImportLocation(parentLocation, importLocation);
        _importResource = FileUtils.GetFileResource(resolvedImportLocation);
        return _importResource.InputStream;
    }

    public string GetBaseUri()
    {
        return _wsdl.Uri.ToString();
    }

    public string? GetLatestImportUri()
    {
        return _importResource?.Uri.ToString();
    }

    public void Close()
    {
        // The caller manages no-op - resources
    }

    public void Dispose()
    {
        Close();
    }

    /// <summary>
    ///     Resolves the full URI of an imported WSDL location based on the parent location and the relative or absolute import
    ///     location.
    /// </summary>
    /// <param name="parentLocation">The URI of the parent WSDL document.</param>
    /// <param name="importLocation">The URI of the import location, which can be relative or absolute.</param>
    /// <returns>The fully resolved import location as a string.</returns>
    /// <exception cref="ArgumentException">Thrown when the parent location is invalid or cannot be processed.</exception>
    private static string ResolveImportLocation(string parentLocation, string importLocation)
    {
        if (Uri.TryCreate(importLocation, UriKind.Absolute, out _))
        {
            return importLocation;
        }

        var lastSlashIndex = parentLocation.LastIndexOf('/');
        if (lastSlashIndex == -1)
        {
            throw new ArgumentException($"Invalid parent location: {parentLocation}");
        }

        return parentLocation[..(lastSlashIndex + 1)] + importLocation;
    }
}
