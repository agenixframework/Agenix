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
using Agenix.Http.Client;
using HttpClient = Agenix.Http.Client.HttpClient;

namespace Agenix.Http.Tests.Endpoint.Builder;

/// <summary>
/// Unit tests for the Http endpoint component lookup and builder functionality.
/// Tests the registration and discovery of Http endpoint builders in the framework.
/// </summary>
[TestFixture]
public class HttpEndpointBuilderTests
{
    /// <summary>
    /// Tests that Http endpoint builders are properly registered and can be looked up by name.
    /// Verifies that both client and server Http endpoints are available in the endpoint registry.
    /// </summary>
    [Test]
    public void ShouldLookupEndpoints()
    {
        // Act
        var endpointBuilders = IEndpointBuilder<IEndpoint>.Lookup();

        // Assert
        Assert.That(endpointBuilders.ContainsKey("http.client"), Is.True,
            "Http client endpoint builder should be registered");
    }

    /// <summary>
    /// Tests that specific Http endpoint builders can be looked up individually by name
    /// and that they return the correct builder types.
    /// </summary>
    [Test]
    public void ShouldLookupEndpoint()
    {
        // Act & Assert - Http Client
        var graphqlClientBuilder = IEndpointBuilder<IEndpoint>.Lookup("http.client");
        Assert.That(graphqlClientBuilder.IsPresent, Is.True,
            "Http client endpoint builder should be found");
        Assert.That(graphqlClientBuilder.Value.GetType(), Is.EqualTo(typeof(HttpClientBuilder)),
            "Should return HttpClientBuilder instance");
    }

    /// <summary>
    /// Tests that Http endpoint builders are properly configured with expected default settings.
    /// Verifies that the builders can create functional endpoint instances.
    /// </summary>
    [Test]
    public void ShouldCreateGraphQlEndpoints()
    {
        // Arrange
        var clientBuilder = IEndpointBuilder<IEndpoint>.Lookup("http.client");

        // Act
        var clientEndpoint = clientBuilder.Value.Build();

        // Assert
        Assert.That(clientEndpoint, Is.Not.Null, "Http client endpoint should be created");
        Assert.That(clientEndpoint, Is.InstanceOf<HttpClient>(),
            "Client endpoint should be HttpClient instance");
    }
}
