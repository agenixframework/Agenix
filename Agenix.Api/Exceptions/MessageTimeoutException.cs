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
///     Exception thrown when a message times out.
/// </summary>
public class MessageTimeoutException : ActionTimeoutException
{
    protected readonly string Endpoint;

    /// <summary>
    ///     Default constructor
    /// </summary>
    public MessageTimeoutException()
        : this(0L, "")
    {
    }

    /// <summary>
    ///     Default constructor
    /// </summary>
    public MessageTimeoutException(long timeout, string endpoint)
        : base(timeout)
    {
        Endpoint = endpoint;
    }

    /// <summary>
    ///     Exception thrown when a message times out.
    /// </summary>
    public MessageTimeoutException(long timeout, string endpoint, SystemException cause)
        : base(timeout, cause)
    {
        Endpoint = endpoint;
    }

    /// <summary>
    ///     Overrides the detail message
    /// </summary>
    /// <returns>A detailed message as a string.</returns>
    protected override string GetDetailMessage()
    {
        if (Timeout <= 0 && Endpoint == null)
        {
            return "Failed to receive message.";
        }

        return $"Failed to receive message on endpoint: '{Endpoint}'";
    }
}
