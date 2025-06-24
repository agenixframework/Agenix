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

namespace Agenix.Api.Exceptions;

/// <summary>
///     Represents an exception that is thrown when an action times out.
/// </summary>
public class ActionTimeoutException : AgenixSystemException
{
    protected readonly long Timeout;

    /// <summary>
    ///     Default constructor
    /// </summary>
    public ActionTimeoutException()
        : this(0L)
    {
    }

    /// <summary>
    ///     Default constructor
    /// </summary>
    public ActionTimeoutException(long timeout)
    {
        Timeout = timeout;
    }

    /// <summary>
    ///     Default constructor
    /// </summary>
    public ActionTimeoutException(long timeout, SystemException cause)
        : base(cause.Message)
    {
        Timeout = timeout;
    }

    /// <summary>
    ///     Overrides the Message property to provide a custom timeout message, indicating the duration of the timeout, if
    ///     specified.
    /// </summary>
    public override string Message => Timeout <= 0
        ? $"Action timeout. {GetDetailMessage()}".Trim()
        : $"Action timeout after {Timeout} milliseconds. {GetDetailMessage()}".Trim();

    /// <summary>
    ///     Provides a detailed message which can be overridden.
    /// </summary>
    /// <returns>A detailed message as a string.</returns>
    protected virtual string GetDetailMessage()
    {
        return string.Empty;
    }
}
