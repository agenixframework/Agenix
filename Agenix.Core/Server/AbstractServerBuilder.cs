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
using Agenix.Core.Endpoint;

namespace Agenix.Core.Server;

/// <summary>
/// Abstract server builder providing fluent API for configuring server instances.
/// </summary>
/// <typeparam name="TServer">The server type that extends AbstractServer.</typeparam>
/// <typeparam name="TBuilder">The builder type that extends AbstractServerBuilder.</typeparam>
public abstract class AbstractServerBuilder<TServer, TBuilder> : AbstractEndpointBuilder<TServer>
    where TServer : AbstractServer
    where TBuilder : AbstractServerBuilder<TServer, TBuilder>
{
    private readonly TBuilder _self;

    protected AbstractServerBuilder()
    {
        _self = (TBuilder)this;
    }

    /// <summary>
    /// Sets the autoStart property.
    /// </summary>
    /// <param name="autoStart">Whether to auto-start the server.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TBuilder AutoStart(bool autoStart)
    {
        GetEndpoint().AutoStart = autoStart;
        return _self;
    }

    /// <summary>
    /// Sets the endpoint adapter.
    /// </summary>
    /// <param name="endpointAdapter">The endpoint adapter to set.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TBuilder EndpointAdapter(IEndpointAdapter endpointAdapter)
    {
        GetEndpoint().EndpointAdapter = endpointAdapter;
        return _self;
    }

    /// <summary>
    /// Sets the debug logging enabled flag.
    /// </summary>
    /// <param name="enabled">Whether debug logging is enabled.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TBuilder DebugLogging(bool enabled)
    {
        GetEndpoint().DebugLogging = enabled;
        return _self;
    }

    /// <summary>
    /// Sets the default timeout.
    /// </summary>
    /// <param name="timeout">The timeout value in milliseconds.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public virtual TBuilder Timeout(long timeout)
    {
        if (GetEndpoint()?.EndpointConfiguration != null)
        {
            GetEndpoint().EndpointConfiguration.Timeout = timeout;
        }

        GetEndpoint().DefaultTimeout = timeout;
        return _self;
    }
}
