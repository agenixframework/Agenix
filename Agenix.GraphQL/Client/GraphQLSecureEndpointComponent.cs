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

namespace Agenix.GraphQL.Client;

/// <summary>
/// GraphQL endpoint component that enforces HTTPS for secure communication.
/// Automatically uses HTTPS for HTTP requests and WSS for WebSocket subscriptions.
/// </summary>
public class GraphQLSecureEndpointComponent : GraphQLEndpointComponent
{
    /// <summary>
    /// Creates a secure GraphQL endpoint component with HTTPS enabled by default.
    /// </summary>
    public GraphQLSecureEndpointComponent() : this("graphql-secure")
    {
    }

    /// <summary>
    /// Creates a secure GraphQL endpoint component with HTTPS enabled by default.
    /// </summary>
    /// <param name="name">The name identifier for this secure GraphQL endpoint component.</param>
    public GraphQLSecureEndpointComponent(string name) : base(name)
    {
    }

    /// <summary>
    /// Gets the HTTPS URI scheme used for secure GraphQL endpoints.
    /// </summary>
    /// <returns>A string representing the HTTPS scheme ("https://").</returns>
    protected override string Scheme => "https://";

    /// <summary>
    /// Gets the secure WebSocket URI scheme used for secure GraphQL subscriptions.
    /// </summary>
    /// <returns>A string representing the secure WebSocket scheme ("wss://").</returns>
    protected override string WebSocketScheme => "wss://";
}
