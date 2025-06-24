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
///     Simple interface for objects that are sources for
///     <see cref="System.IO.Stream" />s.
/// </summary>
/// <seealso cref="IResource" />
public interface IInputStreamSource
{
    /// <summary>
    ///     Return an <see cref="System.IO.Stream" /> for this resource.
    /// </summary>
    /// <remarks>
    ///     <note type="caution">
    ///         Clients of this interface must be aware that every access of this
    ///         property will create a <i>fresh</i> <see cref="System.IO.Stream" />;
    ///         it is the responsibility of the calling code to close any such
    ///         <see cref="System.IO.Stream" />.
    ///     </note>
    /// </remarks>
    /// <value>
    ///     An <see cref="System.IO.Stream" />.
    /// </value>
    /// <exception cref="System.IO.IOException">
    ///     If the stream could not be opened.
    /// </exception>
    Stream InputStream { get; }
}
