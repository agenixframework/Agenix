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

namespace Agenix.GraphQL.Message;

/// <summary>
///     Represents configuration options for GraphQL subscriptions.
/// </summary>
public class GraphQLSubscriptionOptions
{
    /// <summary>
    ///     Gets or sets the maximum duration to keep the subscription alive.
    /// </summary>
    public TimeSpan? MaxDuration { get; set; }

    /// <summary>
    ///     Gets or sets whether to automatically reconnect on connection loss.
    /// </summary>
    public bool AutoReconnect { get; set; } = true;

    /// <summary>
    ///     Gets or sets the reconnection delay.
    /// </summary>
    public TimeSpan ReconnectDelay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    ///     Gets or sets the maximum number of reconnection attempts.
    /// </summary>
    public int MaxReconnectAttempts { get; set; } = 3;

    /// <summary>
    ///     Gets or sets the WebSocket sub-protocol to use.
    /// </summary>
    public string? WebSocketSubProtocol { get; set; } = "graphql-ws";

    /// <summary>
    ///     Gets or sets additional connection parameters for the subscription.
    /// </summary>
    public Dictionary<string, object>? ConnectionParams { get; set; }

    /// <summary>
    ///     Gets or sets the heartbeat interval for keeping the connection alive.
    /// </summary>
    public TimeSpan? HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    ///     Gets or sets whether to buffer messages when the connection is temporarily unavailable.
    /// </summary>
    public bool BufferMessages { get; set; } = true;

    /// <summary>
    ///     Gets or sets the maximum number of messages to buffer.
    /// </summary>
    public int MaxBufferSize { get; set; } = 1000;
}
