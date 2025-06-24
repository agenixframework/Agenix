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

namespace Agenix.Api.Endpoint;

/// <summary>
/// Endpoint adapter represents a special message handler that delegates incoming request messages to some message endpoint.
/// Clients can receive request messages from endpoint and provide proper response messages that will be used as
/// adapter response.
/// </summary>
public interface IEndpointAdapter
{
    /// <summary>
    /// Handles a request message and returning a proper response.
    /// </summary>
    /// <param name="message">The request message.</param>
    /// <returns>The response message.</returns>
    IMessage HandleMessage(IMessage message);

    /// <summary>
    /// Gets message endpoint to interact with this endpoint adapter.
    /// </summary>
    /// <returns>The endpoint instance.</returns>
    IEndpoint GetEndpoint();

    /// <summary>
    /// Gets the endpoint configuration.
    /// </summary>
    /// <returns>The endpoint configuration.</returns>
    IEndpointConfiguration GetEndpointConfiguration();
}
