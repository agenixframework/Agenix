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

using Agenix.Api.Context;
using Agenix.Api.Endpoint;
using Agenix.Api.Message;

namespace Agenix.Api.Messaging;

/// <summary>
///     Abstract base class for message consumers, providing common functionality
///     for handling messages within an endpoint configuration context.
/// </summary>
public abstract class AbstractMessageConsumer : IConsumer
{
    private readonly IEndpointConfiguration _endpointConfiguration;

    /// <summary>
    ///     Default constructor using receive timeout setting.
    /// </summary>
    /// <param name="name">The name of the consumer</param>
    /// <param name="endpointConfiguration">Endpoint configuration</param>
    public AbstractMessageConsumer(string name, IEndpointConfiguration endpointConfiguration)
    {
        Name = name;
        _endpointConfiguration = endpointConfiguration;
    }

    public string Name { get; }

    /// <summary>
    ///     Synchronously receives a message with a context provided by TestContext.
    /// </summary>
    /// <param name="context">The context containing the state and environment for the message reception.</param>
    /// <returns>The received message as an instance of IMessage.</returns>
    public IMessage Receive(TestContext context)
    {
        return Receive(context, _endpointConfiguration.Timeout);
    }

    // Abstract method to be implemented by subclasses
    public abstract IMessage Receive(TestContext context, long timeout);
}
