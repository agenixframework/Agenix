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
using Agenix.Api.Messaging;

namespace Agenix.Core.Endpoint;

/// <summary>
///     Abstract message endpoint handles send/receive timeout setting
/// </summary>
/// <remarks>
///     Default constructor using endpoint configuration.
/// </remarks>
/// <param name="endpointConfiguration">the endpoint configuration</param>
public abstract class AbstractEndpoint(IEndpointConfiguration endpointConfiguration) : IEndpoint
{
    /// <summary>
    ///     Gets the endpoints consumer name.
    /// </summary>
    public string ConsumerName => Name + ":consumer";

    /// <summary>
    ///     Gets the endpoints producer name.
    /// </summary>
    public string ProducerName => Name + ":producer";

    public virtual IEndpointConfiguration EndpointConfiguration { get; } = endpointConfiguration;

    /// <summary>
    ///     Gets/ Sets the endpoints producer name.
    /// </summary>
    public virtual string Name { get; set; } = nameof(AbstractEndpoint);

    public void SetName(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     Creates a message producer for this endpoint. Must be implemented by concrete subclasses.
    /// </summary>
    /// <returns>A message producer.</returns>
    public abstract IProducer CreateProducer();

    /// <summary>
    ///     Creates a message consumer for this endpoint. Must be implemented by concrete subclasses.
    /// </summary>
    /// <returns>A message consumer.</returns>
    public abstract IConsumer CreateConsumer();
}
