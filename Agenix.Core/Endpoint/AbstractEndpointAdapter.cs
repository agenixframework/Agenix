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

using System;
using Agenix.Api.Context;
using Agenix.Api.Endpoint;
using Agenix.Api.Log;
using Agenix.Api.Message;

namespace Agenix.Core.Endpoint;

using Microsoft.Extensions.Logging;

/// <summary>
/// Abstract endpoint adapter adds fallback endpoint adapter in case no response was provided.
/// </summary>
public abstract class AbstractEndpointAdapter : IEndpointAdapter
{
    /// <summary>
    /// Fallback adapter
    /// </summary>
    private IEndpointAdapter _fallbackEndpointAdapter;

    /// <summary>
    /// Endpoint adapter name
    /// </summary>
    private string _name;

    private TestContextFactory? _testContextFactory;

    /// <summary>
    /// Logger
    /// </summary>
    private readonly ILogger _logger = LogManager.GetLogger(typeof(AbstractEndpointAdapter));

    /// <summary>
    /// Initializes a new instance of the AbstractEndpointAdapter class.
    /// </summary>
    protected AbstractEndpointAdapter(ILogger logger)
    {
        _logger = logger;
        _name = GetType().Name;
    }

    /// <summary>
    /// Handles a request message and returns a proper response.
    /// </summary>
    /// <param name="request">The request message.</param>
    /// <returns>The response message.</returns>
    public virtual IMessage HandleMessage(IMessage request)
    {
        var replyMessage = HandleMessageInternal(request);

        if (replyMessage == null || replyMessage.Payload == null)
        {
            if (_fallbackEndpointAdapter != null)
            {
                _logger.LogDebug("Did not receive reply message - " +
                                 "delegating to fallback endpoint adapter");

                replyMessage = _fallbackEndpointAdapter.HandleMessage(request);
            }
            else
            {
                _logger.LogDebug("Did not receive reply message - no response is simulated");
            }
        }

        return replyMessage;
    }

    /// <summary>
    /// Subclasses must implement this method in order to handle incoming request message. If
    /// this method does not return any response message fallback endpoint adapter is invoked for processing.
    /// </summary>
    /// <param name="message">The request message.</param>
    /// <returns>The response message.</returns>
    protected abstract IMessage? HandleMessageInternal(IMessage message);

    /// <summary>
    /// Gets or sets the name of this endpoint adapter.
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets or sets the fallback endpoint adapter.
    /// </summary>
    public IEndpointAdapter? FallbackEndpointAdapter
    {
        get => _fallbackEndpointAdapter;
        set => _fallbackEndpointAdapter = value;
    }

    /// <summary>
    /// Gets or sets the test context factory.
    /// </summary>
    public TestContextFactory? TestContextFactory
    {
        get
        {
            if (_testContextFactory == null)
            {
                _logger.LogWarning("Could not identify proper test context factory from dependency injection - " +
                                   "constructing own test context factory. This restricts test context capabilities to an " +
                                   "absolute minimum! You could do better when enabling proper dependency injection for this server instance.");

                _testContextFactory = TestContextFactory.NewInstance();
            }

            return _testContextFactory;
        }
        set => _testContextFactory = value;
    }

    /// <summary>
    /// Gets new test context from factory.
    /// </summary>
    /// <returns>The test context.</returns>
    protected TestContext GetTestContext()
    {
        return TestContextFactory!.GetObject();
    }

    /// <summary>
    /// Gets message endpoint to interact with this endpoint adapter.
    /// </summary>
    /// <returns>The endpoint instance.</returns>
    public abstract IEndpoint GetEndpoint();

    /// <summary>
    /// Gets the endpoint configuration.
    /// </summary>
    /// <returns>The endpoint configuration.</returns>
    public abstract IEndpointConfiguration GetEndpointConfiguration();
}
