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

using Agenix.Api.Endpoint;

namespace Agenix.Core.Endpoint.Builder;

/// <summary>
///     Represents a builder that constructs client and server endpoints.
/// </summary>
/// <typeparam name="TC">The type of the client builder.</typeparam>
/// <typeparam name="TS">The type of the server builder.</typeparam>
public class ClientServerEndpointBuilder<TC, TS>
    where TC : IEndpointBuilder<IEndpoint>
    where TS : IEndpointBuilder<IEndpoint>
{
    private readonly TC _clientBuilder;
    private readonly TS _serverBuilder;

    /// <summary>
    ///     Constructs a ClientServerEndpointBuilder with the specified client and server builders.
    /// </summary>
    /// <typeparam name="TC">The type of the client builder.</typeparam>
    /// <typeparam name="TS">The type of the server builder.</typeparam>
    /// <param name="clientBuilder">The client builder implementation.</param>
    /// <param name="serverBuilder">The server builder implementation.</param>
    public ClientServerEndpointBuilder(TC clientBuilder, TS serverBuilder)
    {
        _clientBuilder = clientBuilder;
        _serverBuilder = serverBuilder;
    }

    /// <summary>
    ///     Gets the client builder.
    /// </summary>
    /// <returns>The client builder.</returns>
    public TC Client()
    {
        return _clientBuilder;
    }

    /// <summary>
    ///     Gets the server builder.
    /// </summary>
    /// <returns>The server builder.</returns>
    public TS Server()
    {
        return _serverBuilder;
    }
}
