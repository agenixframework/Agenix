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
