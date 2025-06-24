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
///     Exception thrown when a synchronous reply message times out.
/// </summary>
public class ReplyMessageTimeoutException : MessageTimeoutException
{
    /// <summary>
    ///     Exception thrown when a synchronous reply message times out.
    /// </summary>
    public ReplyMessageTimeoutException(long timeout, string endpoint)
        : base(timeout, endpoint)
    {
    }

    /// <summary>
    ///     Exception thrown when a synchronous reply message times out.
    /// </summary>
    public ReplyMessageTimeoutException(long timeout, string endpoint, SystemException cause)
        : base(timeout, endpoint, cause)
    {
    }

    /// <summary>
    ///     Gets an error message describing the reason for the failure to receive a synchronous reply message.
    /// </summary>
    /// <remarks>
    ///     If the Timeout property is less than or equal to zero and the Endpoint property is not set,
    ///     this property returns a default error message.
    ///     Otherwise, it includes the endpoint information in the error message.
    /// </remarks>
    public override string Message
    {
        get
        {
            if (Timeout <= 0 && Endpoint == null)
            {
                return "Failed to receive synchronous reply message.";
            }

            return $"Failed to receive synchronous reply message on endpoint: '{Endpoint}'";
        }
    }
}
