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
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Message;

namespace Agenix.Core.Endpoint.Direct;

using Microsoft.Extensions.Logging;

/// <summary>
/// Endpoint adapter forwards incoming requests to message queue and waits synchronously for response
/// on reply queue. Provides simple endpoint for clients to connect to message queue in order to provide proper
/// response message.
/// </summary>
public class DirectEndpointAdapter : AbstractEndpointAdapter
{
    /// <summary>
    /// Endpoint handling incoming requests
    /// </summary>
    private readonly DirectSyncEndpoint _endpoint;

    private readonly DirectSyncProducer _producer;

    /// <summary>
    /// Endpoint configuration
    /// </summary>
    private readonly DirectSyncEndpointConfiguration _endpointConfiguration;

    /// <summary>
    /// Logger
    /// </summary>
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(DirectEndpointAdapter));

    /// <summary>
    /// Constructor using endpoint.
    /// </summary>
    /// <param name="endpoint">The direct sync endpoint.</param>
    public DirectEndpointAdapter(DirectSyncEndpoint endpoint) : base(Logger)
    {
        _endpointConfiguration = endpoint.EndpointConfiguration;

        endpoint.SetName(Name);
        _producer = new DirectSyncProducer(endpoint.ProducerName, _endpointConfiguration);
        _endpoint = endpoint;
    }

    /// <summary>
    /// Constructor using endpoint configuration.
    /// </summary>
    /// <param name="endpointConfiguration">The endpoint configuration.</param>
    public DirectEndpointAdapter(DirectSyncEndpointConfiguration endpointConfiguration) : base(Logger)
    {
        _endpointConfiguration = endpointConfiguration;

        _endpoint = new DirectSyncEndpoint(endpointConfiguration);
        _endpoint.SetName(Name);
        _producer = new DirectSyncProducer(_endpoint.ProducerName, endpointConfiguration);
    }

    /// <summary>
    /// Handles the incoming request message internally.
    /// </summary>
    /// <param name="request">The request message.</param>
    /// <returns>The reply message.</returns>
    protected override IMessage HandleMessageInternal(IMessage request)
    {
        Logger.LogDebug("Forwarding request to message queue ...");

        var context = GetTestContext();
        IMessage replyMessage = null;

        try
        {
            _producer.Send(request, context);

            if (_endpointConfiguration.Correlator != null)
            {
                replyMessage = _producer.Receive(
                    _endpointConfiguration.Correlator.GetCorrelationKey(request),
                    context,
                    _endpointConfiguration.Timeout);
            }
            else
            {
                replyMessage = _producer.Receive(context, _endpointConfiguration.Timeout);
            }
        }
        catch (ActionTimeoutException e)
        {
            Logger.LogWarning(e.Message);
        }

        return replyMessage;
    }

    /// <summary>
    /// Gets the direct endpoint.
    /// </summary>
    /// <returns>The direct endpoint.</returns>
    public override IEndpoint GetEndpoint()
    {
        return _endpoint;
    }

    /// <summary>
    /// Gets the endpoint configuration.
    /// </summary>
    /// <returns>The direct sync endpoint configuration.</returns>
    public override IEndpointConfiguration GetEndpointConfiguration()
    {
        return _endpointConfiguration;
    }
}
