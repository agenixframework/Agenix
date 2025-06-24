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

namespace Agenix.Api.Message;

/// <summary>
///     Specific message headers.
/// </summary>
public class MessageHeaders
{
    /// <summary>
    ///     Common header name prefix
    /// </summary>
    public static readonly string Prefix = "agenix_";

    /// <summary>
    ///     Message-related header prefix
    /// </summary>
    public static readonly string MessagePrefix = Prefix + "message_";

    /// <summary>
    ///     Unique message id
    /// </summary>
    public static readonly string Id = MessagePrefix + "id";

    /// <summary>
    ///     Time message was created
    /// </summary>
    public static readonly string Timestamp = MessagePrefix + "timestamp";

    /// <summary>
    ///     Header indicating the message type (e.g., XML, JSON, csv, plaintext, etc.)
    /// </summary>
    public static readonly string MessageType = MessagePrefix + "type";

    /// <summary>
    ///     Synchronous message correlation
    /// </summary>
    public static readonly string MessageCorrelationKey = MessagePrefix + "correlator";

    /// <summary>
    ///     Synchronous reply to message destination name
    /// </summary>
    public static readonly string MessageReplyTo = MessagePrefix + "replyTo";

    /// <summary>
    ///     Prevent instantiation.
    /// </summary>
    private MessageHeaders()
    {
    }
}
