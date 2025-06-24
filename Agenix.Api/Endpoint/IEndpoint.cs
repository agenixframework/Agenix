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

using Agenix.Api.Common;
using Agenix.Api.Messaging;

namespace Agenix.Api.Endpoint;

/// <summary>
///     Represents an endpoint for messaging processes that includes configurations,
///     and provides capabilities to produce and consume messages.
/// </summary>
public interface IEndpoint : INamed
{
    /// <summary>
    ///     Gets the endpoint configuration holding all endpoint specific properties such as endpoint uri, connection timeout,
    ///     ports, etc.
    /// </summary>
    IEndpointConfiguration EndpointConfiguration { get; }

    /// <summary>
    ///     Gets/ Sets the endpoint name
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Creates a message producer for this endpoint for sending messages to this endpoint.
    /// </summary>
    /// <returns></returns>
    IProducer CreateProducer();

    /// <summary>
    ///     Creates a message consumer for this endpoint. Consumer receives messages on this endpoint.
    /// </summary>
    /// <returns></returns>
    IConsumer CreateConsumer();
}
