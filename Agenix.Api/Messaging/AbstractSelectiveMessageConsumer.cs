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
///     An abstract class that represents a selective message consumer capable of receiving messages
///     based on a given selector and context.
/// </summary>
public abstract class AbstractSelectiveMessageConsumer(string name, IEndpointConfiguration endpointConfiguration)
    : AbstractMessageConsumer(name, endpointConfiguration), ISelectiveConsumer
{
    private readonly IEndpointConfiguration _endpointConfiguration1 = endpointConfiguration;

    /// <summary>
    ///     Receives a message from a queue based on the specified selector and context.
    /// </summary>
    /// <param name="selector">The message selector to filter messages.</param>
    /// <param name="context">The context containing information about the test and execution environment.</param>
    /// <returns>An instance of IMessage if a message is received, otherwise null.</returns>
    public IMessage Receive(string selector, TestContext context)
    {
        return Receive(selector, context, _endpointConfiguration1.Timeout);
    }

    /// <summary>
    ///     Receives a message from a queue based on the specified selector and context.
    /// </summary>
    /// <param name="context">The context containing information about the test and execution environment.</param>
    /// <param name="timeout">The maximum time to wait for a message, in milliseconds.</param>
    /// <returns>An instance of <see cref="IMessage" /> if a message is received, otherwise null.</returns>
    public override IMessage Receive(TestContext context, long timeout)
    {
        return Receive(null, context, timeout);
    }

    /// <summary>
    ///     Receives a message from a queue based on the specified selector, context, and timeout.
    /// </summary>
    /// <param name="selector">The message selector to filter messages.</param>
    /// <param name="context">The context containing information about the test and execution environment.</param>
    /// <param name="timeout">The maximum time to wait for a message.</param>
    /// <returns>An instance of IMessage if a message is received, otherwise null.</returns>
    public abstract IMessage Receive(string selector, TestContext context, long timeout);
}
