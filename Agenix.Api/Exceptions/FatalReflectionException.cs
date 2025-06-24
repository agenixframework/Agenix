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

using System.Runtime.Serialization;

#endregion

namespace Agenix.Api.Exceptions;

/// <summary>
///     Thrown on an unrecoverable problem encountered in the
///     objects namespace or sub-namespaces, e.g. bad class or field.
/// </summary>
[Serializable]
public class FatalReflectionException : ReflectionException
{
    /// <summary>
    ///     Creates a new instance of the FatalObjectException class.
    /// </summary>
    public FatalReflectionException()
    {
    }

    /// <summary>
    ///     Creates a new instance of the FatalObjectException class with the
    ///     specified message.
    /// </summary>
    /// <param name="message">
    ///     A message about the exception.
    /// </param>
    public FatalReflectionException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Creates a new instance of the FatalObjectException class with the
    ///     specified message.
    /// </summary>
    /// <param name="message">
    ///     A message about the exception.
    /// </param>
    /// <param name="rootCause">
    ///     The root exception that is being wrapped.
    /// </param>
    public FatalReflectionException(string message, Exception rootCause)
        : base(message, rootCause)
    {
    }

    /// <summary>
    ///     Creates a new instance of the FatalObjectException class.
    /// </summary>
    /// <param name="info">
    ///     The <see cref="System.Runtime.Serialization.SerializationInfo" />
    ///     that holds the serialized object data about the exception being thrown.
    /// </param>
    /// <param name="context">
    ///     The <see cref="System.Runtime.Serialization.StreamingContext" />
    ///     that contains contextual information about the source or destination.
    /// </param>
    protected FatalReflectionException(
        SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
