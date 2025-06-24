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

using Agenix.Api.Messaging;

namespace Agenix.Core.Endpoint.Direct;

/// <summary>
///     Direct message endpoint implementation sends and receives message from in memory message queue.
/// </summary>
public class DirectSyncEndpoint : DirectEndpoint
{
    /**
     * Cached producer or consumer
     */
    private DirectSyncConsumer _syncConsumer;

    private DirectSyncProducer _syncProducer;

    /// A class representing a direct message endpoint.
    /// This class is responsible for creating producers and consumers for the direct endpoint.
    /// /
    public DirectSyncEndpoint() : base(new DirectSyncEndpointConfiguration())
    {
    }

    /// <summary>
    ///     Direct message endpoint implementation sends and receives message from in memory message queue.
    /// </summary>
    public DirectSyncEndpoint(DirectSyncEndpointConfiguration configuration) : base(configuration)
    {
    }

    /// <summary>
    ///     Provides the specific configuration settings used by the DirectSyncEndpoint.
    /// </summary>
    public override DirectSyncEndpointConfiguration EndpointConfiguration =>
        (DirectSyncEndpointConfiguration)base.EndpointConfiguration;

    /// <summary>
    ///     Creates a new producer instance for the direct endpoint.
    /// </summary>
    /// <returns>Returns an instance of <see cref="IProducer" />.</returns>
    public override IProducer CreateProducer()
    {
        return _syncProducer ??= new DirectSyncProducer(ProducerName, EndpointConfiguration);
    }

    /// Creates a consumer for the direct message endpoint.
    /// This method instantiates and returns a new DirectConsumer if one does not already exist.
    /// <returns>An IConsumer instance representing the message consumer.</returns>
    public override ISelectiveConsumer CreateConsumer()
    {
        return _syncConsumer ??= new DirectSyncConsumer(ConsumerName, EndpointConfiguration);
    }
}
