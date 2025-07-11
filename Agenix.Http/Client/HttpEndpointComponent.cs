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
using Agenix.Core.Endpoint;

namespace Agenix.Http.Client;

/// Represents a component responsible for creating HTTP client endpoints based on a given URI
/// resource and associated parameters. It supports configuration of HTTP-specific endpoint settings.
/// /
public class HttpEndpointComponent : AbstractEndpointComponent
{
    /// Represents a component responsible for creating HTTP client endpoints based on a given URI resource
    /// and associated parameters. It allows configuration of HTTP-specific endpoint settings and supports
    /// various HTTP-related options such as request methods and error handling strategies.
    /// /
    public HttpEndpointComponent() : this("http")
    {
    }

    /// Represents a specialized component used for creating HTTP client endpoints based on specific
    /// resource paths and parameters. This component handles configuration and initialization of
    /// HTTP endpoints, enabling customization of options such as request methods and error handling.
    public HttpEndpointComponent(string name) : base(name)
    {
    }

    /// Gets the URI scheme used for HTTP endpoints.
    /// @return A string representing the scheme (e.g., "http://").
    /// /
    protected virtual string Scheme => "http://";

    /// Creates an HTTP endpoint based on the provided resource path, parameters, and execution context.
    /// This method constructs an endpoint with configuration details defined in the parameters and
    /// customizes specific HTTP options such as the request method and URL structure.
    /// <param name="resourcePath">The relative path for the resource to be accessed by the HTTP endpoint.</param>
    /// <param name="parameters">A dictionary of parameters used to configure the endpoint, including HTTP-specific options.</param>
    /// <param name="context">The context for the test environment, providing configuration or dependencies.</param>
    /// <return>An instance of IEndpoint configured as an HTTP client endpoint.</return>
    protected override IEndpoint CreateEndpoint(string resourcePath, IDictionary<string, string> parameters,
        TestContext context)
    {
        var client = new HttpClient
        {
            EndpointConfiguration =
            {
                RequestUrl = Scheme + resourcePath +
                             GetParameterString(parameters, typeof(HttpEndpointConfiguration))
            }
        };


        if (parameters.Remove("requestMethod", out var value))
        {
            client.EndpointConfiguration.RequestMethod = HttpMethod.Parse(value);
        }


        EnrichEndpointConfiguration(
            client.EndpointConfiguration,
            GetEndpointConfigurationParameters(parameters, typeof(HttpEndpointConfiguration)),
            context);
        return client;
    }
}
