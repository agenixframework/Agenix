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

using Agenix.Api.Util;

#endregion

namespace Agenix.Api.IO;

/// <summary>
///     <see cref="IResource" /> adapter implementation for a
///     <see cref="System.IO.Stream" />.
/// </summary>
/// <remarks>
///     <p>
///         Should only be used if no other <see cref="IResource" />
///         implementation is applicable.
///     </p>
///     <p>
///         In contrast to other <see cref="IResource" />
///         implementations, this is an adapter for an <i>already opened</i>
///         resource - the <see cref="IsOpen" />
///         therefore always returns <see langword="true" />. Do not use this class
///         if you need to keep the resource descriptor somewhere, or if you need
///         to read a stream multiple times.
///     </p>
/// </remarks>
public class InputStreamResource : AbstractResource
{
    #region Constructor (s) / Destructor

    /// <summary>
    ///     Creates a new instance of the
    ///     <see cref="InputStreamResource" /> class.
    /// </summary>
    /// <param name="inputStream">
    ///     The input <see cref="System.IO.Stream" /> to use.
    /// </param>
    /// <param name="description">
    ///     Where the input <see cref="System.IO.Stream" /> comes from.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    ///     If the supplied <paramref name="inputStream" /> is
    ///     <see langword="null" />.
    /// </exception>
    public InputStreamResource(Stream inputStream, string description)
    {
        AssertUtils.ArgumentNotNull(inputStream, "inputStream");

        _inputStream = inputStream;
        _description = description ?? string.Empty;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     The input <see cref="System.IO.Stream" /> to use.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">
    ///     If the underlying <see cref="System.IO.Stream" /> has already
    ///     been read.
    /// </exception>
    public override Stream InputStream
    {
        get
        {
            if (_inputStream == null)
            {
                throw new InvalidOperationException(
                    "InputStream has already been read - " +
                    "do not use InputStreamResource if a stream " +
                    "needs to be read multiple times");
            }

            var result = _inputStream;
            _inputStream = null;
            return result;
        }
    }

    /// <summary>
    ///     Returns a description for this resource.
    /// </summary>
    /// <value>
    ///     A description for this resource.
    /// </value>
    /// <seealso cref="IResource.Description" />
    public override string Description => _description;

    /// <summary>
    ///     This implementation always returns true
    /// </summary>
    public override bool IsOpen => true;


    /// <summary>
    ///     This implementation always returns true
    /// </summary>
    public override bool Exists => true;

    #endregion

    #region Fields

    private Stream _inputStream;
    private readonly string _description;

    #endregion
}
