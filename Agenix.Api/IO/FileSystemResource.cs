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

using System.Globalization;
using Agenix.Api.Util;

#endregion

namespace Agenix.Api.IO;

/// <summary>
///     A <see cref="System.IO.File" /> backed resource.
/// </summary>
/// <remarks>
///     <p>
///         Supports resolution as both a <see cref="System.IO.FileInfo" /> and a
///         <see cref="System.Uri" />.
///     </p>
///     <p>
///         Also supports the use of the <c>~</c> character. If the <c>~</c> character
///         is the first character in a resource path (sans protocol), the <c>~</c>
///         character will be replaced with the value of the
///         <c>System.AppDomain.CurrentDomain.BaseDirectory</c> property (an example of
///         this can be seen in the examples below).
///     </p>
/// </remarks>
/// <example>
///     <p>
///         Consider the example of an application that is running (has been launched
///         from) the <c>C:\App\</c> directory. The following resource paths will map
///         to the following resources on the filesystem...
///     </p>
///     <code escaped="true">
///     strings.txt              C:\App\strings.txt
///     ~/strings.txt            C:\App\strings.txt
///     file://~/strings.txt     C:\App\strings.txt
///     file://~/../strings.txt  C:\strings.txt
///     ../strings.txt           C:\strings.txt
///     ~/../strings.txt         C:\strings.txt
/// 
///     // note that only a leading ~ character is resolved to the executing directory...
///     stri~ngs.txt              C:\App\stri~ngs.txt
/// </code>
/// </example>
public class FileSystemResource : AbstractResource
{
    private FileInfo _fileHandle;
    private string _resourcePath;
    private string _rootLocation;

    /// <summary>
    ///     Initializes this instance.
    /// </summary>
    /// <param name="resourceName"></param>
    protected void Initialize(string resourceName)
    {
        _fileHandle = ResolveFileHandle(resourceName);
        _rootLocation = ResolveRootLocation(resourceName);
        _resourcePath = ResolveResourcePath(resourceName);
    }

    /// <summary>
    ///     Resolves the <see cref="System.IO.FileInfo" /> handle
    ///     for the supplied <paramref name="resourceName" />.
    /// </summary>
    /// <param name="resourceName">
    ///     The name of the file system resource.
    /// </param>
    /// <returns>
    ///     The <see cref="System.IO.FileInfo" /> handle for this resource.
    /// </returns>
    protected virtual FileInfo ResolveFileHandle(string resourceName)
    {
        return new FileInfo(ResolveResourceNameWithoutProtocol(resourceName));
    }

    /// <summary>
    ///     Resolves the root location for the supplied <paramref name="resourceName" />.
    /// </summary>
    /// <param name="resourceName">
    ///     The name of the file system resource.
    /// </param>
    /// <returns>
    ///     The root location of the resource.
    /// </returns>
    protected virtual string ResolveRootLocation(string resourceName)
    {
        var root = _fileHandle.Directory.Root.ToString();
        if (root.Length > 0 &&
            (root.EndsWith(Path.DirectorySeparatorChar.ToString()) ||
             root.EndsWith(Path.AltDirectorySeparatorChar.ToString())))
        {
            root = root[..^1];
        }

        return root;
    }

    /// <summary>
    ///     Resolves the path for the supplied <paramref name="resourceName" />.
    /// </summary>
    /// <param name="resourceName">
    ///     The name of the file system resource.
    /// </param>
    /// <returns>
    ///     The current path of the resource.
    /// </returns>
    protected virtual string ResolveResourcePath(string resourceName)
    {
        var path = _fileHandle.DirectoryName;
        if (path.Equals(_fileHandle.Directory.Root.ToString()))
        {
            path = null;
        }
        else
        {
            path = path[(_rootLocation.Length + 1)..];
        }

        return path;
    }

    /// <summary>
    ///     Resolves the presence of the
    ///     <paramref name="basePathPlaceHolder" /> value
    ///     in the supplied <paramref name="resourceName" /> into a path.
    /// </summary>
    /// <param name="resourceName">
    ///     The name of the resource.
    /// </param>
    /// <param name="basePathPlaceHolder">
    ///     The string that is a placeholder for a base path.
    /// </param>
    /// <returns>
    ///     The name of the resource with any <paramref name="basePathPlaceHolder" />
    ///     value having been resolved into an actual path.
    /// </returns>
    protected override string ResolveBasePathPlaceHolder(
        string resourceName, string basePathPlaceHolder)
    {
        // Remove extra slashes used to indicate that resource is local (handle the case "/C:/path1/...")
        if (resourceName[0] == '/' && resourceName[2] == ':')
        {
            resourceName = resourceName.Substring(1);
        }

        if (StringUtils.HasText(resourceName)
            && resourceName.TrimStart().StartsWith(basePathPlaceHolder))
        {
            return resourceName.Replace(basePathPlaceHolder, AppDomain.CurrentDomain.BaseDirectory).TrimStart();
        }

        return resourceName;
    }

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
        return !
            (resourceName.StartsWith(@"\\") || // UNC file share
             resourceName.IndexOf(':') >= 0 || // drive
             resourceName.StartsWith(BasePathPlaceHolder));
    }

    #region Constructors

    /// <summary>
    ///     Creates a new instance of the
    ///     <see cref="FileSystemResource" /> class.
    /// </summary>
    protected FileSystemResource()
    {
    }

    /// <summary>
    ///     Creates a new instance of the
    ///     <see cref="FileSystemResource" /> class.
    /// </summary>
    /// <param name="resourceName">
    ///     The name of the file system resource.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    ///     If the supplied <paramref name="resourceName" /> is
    ///     <see langword="null" /> or contains only whitespace character(s).
    /// </exception>
    public FileSystemResource(string resourceName)
        : this(resourceName, false)
    {
    }

    /// <summary>
    ///     Creates a new instance of the
    ///     <see cref="FileSystemResource" /> class.
    /// </summary>
    /// <param name="resourceName">
    ///     The name of the file system resource.
    /// </param>
    /// <param name="suppressInitialize">
    ///     Supresses initialization of this instance. Used from derived classes.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    ///     If the supplied <paramref name="resourceName" /> is
    ///     <see langword="null" /> or contains only whitespace character(s).
    /// </exception>
    protected FileSystemResource(string resourceName, bool suppressInitialize)
        : base(resourceName)
    {
        if (!suppressInitialize)
        {
            Initialize(resourceName);
        }
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Returns the underlying <see cref="System.IO.FileInfo" /> handle for
    ///     this resource.
    /// </summary>
    /// <value>
    ///     The <see cref="System.IO.FileInfo" /> handle for this resource.
    /// </value>
    /// <seealso cref="IResource.File" />
    public override FileInfo File => _fileHandle;

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
    ///     Gets the root location of the resource (a drive or UNC file share
    ///     name in this case).
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
    /// <seealso cref="System.IO.Path" />
    protected override char[] PathSeparatorChars => [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];

    /// <summary>
    ///     Return an <see cref="System.IO.Stream" /> for this resource.
    /// </summary>
    /// <value>
    ///     An <see cref="System.IO.Stream" />.
    /// </value>
    /// <exception cref="System.IO.IOException">
    ///     If the stream could not be opened.
    /// </exception>
    /// <exception cref="System.IO.FileNotFoundException">
    ///     If the underlying file could not be found.
    /// </exception>
    /// <seealso cref="IInputStreamSource" />
    public override Stream InputStream
    {
        get
        {
            if (Uri.IsFile)
            {
                try
                {
                    return new FileStream(Uri.LocalPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                }
                catch (DirectoryNotFoundException)
                {
                    // ignore difference between File & Directory exception in this case
                }
            }

            throw new FileNotFoundException(Description
                                            + " cannot be resolved to local file path"
                                            + " - resource does not use 'file:' protocol.");
        }
    }

    /// <summary>
    ///     Returns a description for this resource.
    /// </summary>
    /// <value>
    ///     A description for this resource.
    /// </value>
    /// <seealso cref="NHamcrest.Core.Description" />
    public override string Description =>
        string.Format(
            CultureInfo.InvariantCulture,
            "file [{0}]", _fileHandle.FullName);

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
    /// <seealso cref="System.Uri" />
    public override Uri Uri => new UriBuilder("file", null, 0, _fileHandle.FullName).Uri;

    #endregion
}
