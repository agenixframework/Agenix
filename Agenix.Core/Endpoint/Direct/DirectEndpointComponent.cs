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

using System.Collections.Generic;
using Agenix.Api.Context;
using Agenix.Api.Endpoint;
using Agenix.Core.Message;

namespace Agenix.Core.Endpoint.Direct;

public class DirectEndpointComponent(string name = "direct") : AbstractEndpointComponent(name)
{
    public DirectEndpointComponent() : this("direct")
    {
    }

    /// <summary>
    ///     Creates an endpoint for the given resource path, parameters, and context.
    /// </summary>
    /// <param name="resourcePath">The resource path that determines the type and configuration of the endpoint.</param>
    /// <param name="parameters">A dictionary of parameters used to further configure the endpoint.</param>
    /// <param name="context">The test context that provides additional configuration and reference resolution.</param>
    /// <returns>Returns an instance of an object that implements the IEndpoint interface.</returns>
    protected override IEndpoint CreateEndpoint(string resourcePath, IDictionary<string, string> parameters,
        TestContext context)
    {
        DirectEndpoint endpoint;
        string queueName;

        if (resourcePath.StartsWith("sync:"))
        {
            var endpointConfiguration = new DirectSyncEndpointConfiguration();
            endpoint = new DirectSyncEndpoint(endpointConfiguration);
            queueName = resourcePath["sync:".Length..];
        }
        else
        {
            endpoint = new DirectEndpoint();
            queueName = resourcePath;
        }

        endpoint.EndpointConfiguration.SetQueueName(queueName);

        if (!context.ReferenceResolver.IsResolvable(queueName))
        {
            context.ReferenceResolver.Bind(queueName, new DefaultMessageQueue(queueName));
        }

        EnrichEndpointConfiguration(endpoint.EndpointConfiguration, parameters, context);

        return endpoint;
    }
}
