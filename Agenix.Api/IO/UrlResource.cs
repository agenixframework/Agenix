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

#region Imports

using System.Net;
using Agenix.Api.Util;

#endregion

namespace Agenix.Api.IO;

/// <summary>
///     A <see cref="System.Uri" /> backed resource
///     on top of <see cref="System.Net.WebRequest" />
/// </summary>
/// <remarks>
///     <p>
///         Obviously supports resolution as a <see cref="System.Uri" />, and also
///         as a <see cref="System.IO.FileInfo" /> in the case of the <c>"file:"</c>
///         protocol.
///     </p>
/// </remarks>
/// <example>
///     <p>
///         Some examples of the strings that can be used to initialize a new
///         instance of the <see cref="UrlResource" /> class
///         include...
///         <list type="bullet">
///             <item>
///                 <description>file:///Config/objects.xml</description>
///             </item>
///             <item>
///                 <description>http://www.mycompany.com/services.txt</description>
///             </item>
///         </list>
///     </p>
/// </example>
public class UrlResource : AbstractResource
{
    private readonly string _resourcePath;
    private readonly string _rootLocation;
    private readonly Uri _uri;

    /// <summary>
    ///     Creates a new instance of the
    ///     <see cref="UrlResource" /> class.
    /// </summary>
    /// <example>
    ///     <p>
    ///         Some examples of the values that the <paramref name="resourceName" />
    ///         can typically be expected to hold include...
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>file:///Config/objects.xml</description>
    ///             </item>
    ///             <item>
    ///                 <description>http://www.mycompany.com/services.txt</description>
    ///             </item>
    ///         </list>
    ///     </p>
    /// </example>
    /// <param name="resourceName">
    ///     A string representation of the <see cref="System.Uri" /> resource.
    /// </param>
    public UrlResource(string resourceName) : base(resourceName)
    {
        if (resourceName.StartsWith("file:///"))
        {
            resourceName = resourceName["file:///".Length..];
        }

        _uri = new Uri(resourceName);
        _rootLocation = _uri.Host;
        if (!_uri.IsDefaultPort)
        {
            _rootLocation += ":" + _uri.Port;
        }

        _resourcePath = _uri.AbsolutePath;
        var n = _resourcePath.LastIndexOf('/');
        _resourcePath = n > 0 ? _resourcePath.Substring(1, n - 1) : null;
        WebRequest = WebRequest.Create(_uri);
    }

    /// <summary>
    ///     Returns the <see cref="System.Net.WebRequest" /> instance
    ///     used for the resource resolution.
    /// </summary>
    /// <value>
    ///     A <see cref="System.Net.WebRequest" /> instance.
    /// </value>
    /// <seealso cref="System.Net.HttpWebRequest" />
    /// <seealso cref="System.Net.FileWebRequest" />
    public WebRequest WebRequest { get; }

    /// <summary>
    ///     Return an <see cref="System.IO.Stream" /> for this resource.
    /// </summary>
    /// <value>
    ///     An <see cref="System.IO.Stream" />.
    /// </value>
    /// <exception cref="System.IO.IOException">
    ///     If the stream could not be opened.
    /// </exception>
    /// <seealso cref="IInputStreamSource" />
    public override Stream InputStream => WebRequest.GetResponse().GetResponseStream();

    /// <summary>
    ///     Returns the <see cref="System.Uri" /> handle for this resource.
    /// </summary>
    /// <value>
    ///     The <see cref="System.Uri" /> handle for this resource.
    /// </value>
    /// <exception cref="System.IO.IOException">
    ///     If the resource is not available or cannot be exposed as a
    ///     <see cref="System.Uri" />.
    /// </exception>
    /// <seealso cref="IResource.Uri" />
    public override Uri Uri => _uri;

    /// <summary>
    ///     Returns a <see cref="System.IO.FileInfo" /> handle for this resource.
    /// </summary>
    /// <value>
    ///     The <see cref="System.IO.FileInfo" /> handle for this resource.
    /// </value>
    /// <exception cref="System.IO.FileNotFoundException">
    ///     If the resource is not available on a filesystem.
    /// </exception>
    /// <seealso cref="IResource.File" />
    public override FileInfo File
    {
        get
        {
            if (_uri.IsFile)
            {
                return new FileInfo(_uri.AbsolutePath);
            }

            throw new FileNotFoundException(Description +
                                            " cannot be resolved to absolute file path - " +
                                            "resource does not use 'file:' protocol.");
        }
    }

    /// <summary>
    ///     Does this <see cref="IResource" /> support relative
    ///     resource retrieval?
    /// </summary>
    /// <remarks>
    ///     <p>
    ///         This implementation does support relative resource retrieval, and
    ///         so will always return <see langword="true" />.
    ///     </p>
    /// </remarks>
    /// <value>
    ///     <see langword="true" /> if this
    ///     <see cref="IResource" /> supports relative resource
    ///     retrieval.
    /// </value>
    /// <seealso cref="AbstractResource.SupportsRelativeResources" />
    protected override bool SupportsRelativeResources => true;

    /// <summary>
    ///     Gets the root location of the resource.
    /// </summary>
    /// <value>
    ///     The root location of the resource.
    /// </value>
    /// <seealso cref="AbstractResource.RootLocation" />
    protected override string RootLocation => _rootLocation;

    /// <summary>
    ///     Gets the current path of the resource.
    /// </summary>
    /// <value>
    ///     The current path of the resource.
    /// </value>
    /// <seealso cref="AbstractResource.ResourcePath" />
    protected override string ResourcePath => _resourcePath;

    /// <summary>
    ///     Gets those characters that are valid path separators for the
    ///     resource type.
    /// </summary>
    /// <value>
    ///     Those characters that are valid path separators for the resource
    ///     type.
    /// </value>
    /// <seealso cref="AbstractResource.PathSeparatorChars" />
    protected override char[] PathSeparatorChars => ['/'];

    /// <summary>
    ///     Returns a description for this resource.
    /// </summary>
    /// <value>
    ///     A description for this resource.
    /// </value>
    /// <seealso cref="IResource.Description" />
    public override string Description => StringUtils.Surround("URL [", Uri, "]");

    /// <summary>
    ///     Does the supplied <paramref name="resourceName" /> relative ?
    /// </summary>
    /// <param name="resourceName">
    ///     The name of the resource to test.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if resource name is relative;
    ///     otherwise <see langword="false" />.
    /// </returns>
    protected override bool IsRelativeResource(string resourceName)
    {
        return true;
    }
}
