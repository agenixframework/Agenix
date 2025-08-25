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
///     Represents a resource that is backed by a byte array. Provides methods to access the contents
///     of the byte array as a <see cref="Stream" /> and a description associated with the resource.
/// </summary>
public class ByteArrayResource(byte[] byteArray, string description = "byte array resource")
    : AbstractResource
{
    private readonly byte[] _byteArray = byteArray ?? throw new ArgumentNullException(nameof(byteArray));

    /// <summary>
    /// Represents a stream that facilitates reading data from a specified input source.
    /// This property provides an instance of <see cref="System.IO.Stream"/> for reading purposes.
    /// </summary>
    /// <remarks>
    /// The stream returned is intended to provide access to the underlying data of the input source
    /// in a sequential or random read-access manner, depending on the implementation of the source.
    /// </remarks>
    /// <value>
    /// A <see cref="System.IO.Stream"/> instance that supports reading operations on the input source.
    /// </value>
    /// <exception cref="System.ObjectDisposedException">
    /// Thrown if the input source is no longer available or has already been disposed.
    /// </exception>
    /// <exception cref="System.IO.IOException">
    /// Thrown if an I/O error occurs while accessing the input source.
    /// </exception>
    public override Stream InputStream => new MemoryStream(_byteArray);

    /// <summary>
    /// Indicates whether a specific condition, state, or entity exists.
    /// This property determines the presence or validity of a required value or object.
    /// </summary>
    /// <remarks>
    /// The result of this property is a boolean value, which evaluates to true if the condition,
    /// state, or entity is present as expected, and false otherwise.
    /// </remarks>
    /// <value>
    /// A <see cref="bool"/> value representing the existence of the specified condition, state, or entity.
    /// </value>
    public override bool Exists => true;

    /// <summary>
    /// Gets a description of the resource.
    /// This property provides additional details or metadata about the resource, typically represented as a string.
    /// </summary>
    /// <remarks>
    /// The description is intended to offer a meaningful label or context for the resource, which can be useful
    /// in logging, debugging, or user-facing interfaces.
    /// </remarks>
    /// <value>
    /// A <see cref="string"/> representing the description of the resource.
    /// </value>
    public override string Description => description;

    /// <summary>
    /// Gets a value indicating whether the resource is currently open.
    /// </summary>
    /// <remarks>
    /// This property is used to determine if the underlying resource is open or permanently closed.
    /// For the <see cref="ByteArrayResource"/> implementation, this value is always <see langword="false"/>,
    /// indicating that the resource does not manage any open state.
    /// </remarks>
    /// <value>
    /// A <see cref="bool"/> indicating whether the resource is open. This value is always <see langword="false"/> for this implementation.
    /// </value>
    public override bool IsOpen => false;
}
