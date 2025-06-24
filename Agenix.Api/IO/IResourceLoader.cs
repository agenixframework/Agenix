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

namespace Agenix.Api.IO;

/// <summary>
///     Describes an object that can load
///     <see cref="IResource" />s.
/// </summary>
/// <remarks>
///     <p>
///         The <see cref="ConfigurableResourceLoader" /> class is a
///         standalone implementation that is usable outside an
///         <see cref="Agenix.Context.IApplicationContext" />; the aforementioned
///         class is also used by the
///         <see cref="Agenix.Core.IO.ResourceConverter" /> class.
///     </p>
/// </remarks>
public interface IResourceLoader
{
    /// <summary>
    ///     Return an <see cref="IResource" /> handle for the
    ///     specified resource.
    /// </summary>
    /// <remarks>
    ///     <p>
    ///         The handle should always be a reusable resource descriptor; this
    ///         allows one to make repeated calls to the underlying
    ///         <see cref="IInputStreamSource.InputStream" />.
    ///     </p>
    ///     <p>
    ///         <ul>
    ///             <li>
    ///                 <b>Must</b> support fully qualified URLs, e.g. "file:C:/test.dat".
    ///             </li>
    ///             <li>
    ///                 Should support relative file paths, e.g. "test.dat" (this will be
    ///                 implementation-specific, typically provided by an
    ///             </li>
    ///         </ul>
    ///     </p>
    ///     <note>
    ///         An <see cref="IResource" /> handle does not imply an
    ///         existing resource; you need to check the value of an
    ///         <see cref="IResource" />'s
    ///         <see cref="IResource.Exists" /> property to determine
    ///         conclusively whether or not the resource actually exists.
    ///     </note>
    /// </remarks>
    /// <param name="location">The resource location.</param>
    /// <returns>
    ///     An appropriate <see cref="IResource" /> handle.
    /// </returns>
    IResource GetResource(string location);
}
