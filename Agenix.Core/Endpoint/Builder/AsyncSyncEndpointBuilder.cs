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
///     Provides a builder for both asynchronous and synchronous endpoints, enabling the construction of endpoints that
///     support both types of operations.
/// </summary>
/// <typeparam name="TA">The type of the asynchronous endpoint builder.</typeparam>
/// <typeparam name="TS">The type of the synchronous endpoint builder.</typeparam>
public class AsyncSyncEndpointBuilder<TA, TS>
    where TA : IEndpointBuilder<IEndpoint>
    where TS : IEndpointBuilder<IEndpoint>
{
    private readonly TA _asyncEndpointBuilder;
    private readonly TS _syncEndpointBuilder;

    /// <summary>
    ///     Default constructor setting the sync and async builder implementation.
    /// </summary>
    /// <param name="asyncEndpointBuilder">The asynchronous endpoint builder.</param>
    /// <param name="syncEndpointBuilder">The synchronous endpoint builder.</param>
    public AsyncSyncEndpointBuilder(TA asyncEndpointBuilder, TS syncEndpointBuilder)
    {
        _asyncEndpointBuilder = asyncEndpointBuilder;
        _syncEndpointBuilder = syncEndpointBuilder;
    }

    /// <summary>
    ///     Gets the async endpoint builder.
    /// </summary>
    /// <returns>The asynchronous endpoint builder.</returns>
    public TA Asynchronous()
    {
        return _asyncEndpointBuilder;
    }

    /// <summary>
    ///     Gets the sync endpoint builder.
    /// </summary>
    /// <returns>The synchronous endpoint builder.</returns>
    public TS Synchronous()
    {
        return _syncEndpointBuilder;
    }
}
