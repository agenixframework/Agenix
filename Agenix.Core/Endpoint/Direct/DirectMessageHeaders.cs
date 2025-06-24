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

using Agenix.Api.Message;

namespace Agenix.Core.Endpoint.Direct;

/// <summary>
///     Represents headers for direct messages within the Agenix.Core.Endpoint.Direct namespace.
/// </summary>
public class DirectMessageHeaders
{
    /**
     * Message reply queue name header
     */
    public static readonly string ReplyQueue = MessageHeaders.Prefix + "reply_queue";

    /// Provides constants for various message headers specific to direct messaging.
    /// These headers are used to facilitate direct communication between endpoints.
    /// /
    private DirectMessageHeaders()
    {
        // utility class
    }
}
