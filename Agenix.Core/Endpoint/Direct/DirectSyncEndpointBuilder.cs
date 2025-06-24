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
///     Provides functionality for building <see cref="DirectSyncEndpoint" /> instances.
/// </summary>
public class DirectSyncEndpointBuilder : AbstractEndpointBuilder<DirectSyncEndpoint>
{
    /// <summary>
    ///     Endpoint target
    /// </summary>
    private readonly DirectSyncEndpoint _endpoint = new();

    /// <summary>
    ///     Gets the instance of <see cref="DirectSyncEndpoint" /> being built.
    /// </summary>
    /// <returns>
    ///     The <see cref="DirectSyncEndpoint" /> instance.
    /// </returns>
    protected override DirectSyncEndpoint GetEndpoint()
    {
        return _endpoint;
    }

    /// <summary>
    ///     Sets the queue name for the endpoint.
    /// </summary>
    /// <param name="queueName">The name of the queue to be set for the endpoint.</param>
    /// <returns>The current instance of <see cref="DirectSyncEndpointBuilder" />.</returns>
    public DirectSyncEndpointBuilder Queue(string queueName)
    {
        _endpoint.EndpointConfiguration.SetQueueName(queueName);
        return this;
    }

    /// <summary>
    ///     Sets the given queue for the endpoint.
    /// </summary>
    /// <param name="queue">The queue to be set for the endpoint.</param>
    /// <returns>The current instance of <see cref="DirectSyncEndpointBuilder" />.</returns>
    public DirectSyncEndpointBuilder Queue(IMessageQueue queue)
    {
        _endpoint.EndpointConfiguration.SetQueue(queue);
        return this;
    }

    /// <summary>
    ///     Sets the polling interval for the endpoint.
    /// </summary>
    /// <param name="pollingInterval">The polling interval, in milliseconds.</param>
    /// <returns>The current <see cref="DirectSyncEndpointBuilder" /> instance.</returns>
    public DirectSyncEndpointBuilder PollingInterval(int pollingInterval)
    {
        _endpoint.EndpointConfiguration.PollingInterval = pollingInterval;
        return this;
    }

    /// <summary>
    ///     Sets the message correlator.
    /// </summary>
    /// <param name="correlator">The message correlator to set.</param>
    /// <returns>The current <see cref="DirectSyncEndpointBuilder" /> instance.</returns>
    public DirectSyncEndpointBuilder Correlator(IMessageCorrelator correlator)
    {
        _endpoint.EndpointConfiguration.Correlator = correlator;
        return this;
    }

    /// <summary>
    ///     Sets the default timeout.
    /// </summary>
    /// <param name="timeout">The timeout value in milliseconds.</param>
    /// <returns>The current instance of <see cref="DirectSyncEndpointBuilder" />.</returns>
    public DirectSyncEndpointBuilder Timeout(long timeout)
    {
        _endpoint.EndpointConfiguration.Timeout = timeout;
        return this;
    }
}
