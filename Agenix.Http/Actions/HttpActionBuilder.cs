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

using Agenix.Api;
using Agenix.Api.Spi;
using Agenix.Core.Util;
using HttpClient = Agenix.Http.Client.HttpClient;

namespace Agenix.Http.Actions;

/// Provides a fluent API for building and configuring HTTP actions.
/// Supports initiating client or server HTTP operations.
/// Extends behavior to resolve references dynamically during action construction.
/// /
public class HttpActionBuilder : AbstractReferenceResolverAwareTestActionBuilder<ITestAction>
{
    /// Static entrance method for the HTTP fluent action builder.
    /// @return The HTTP action builder instance.
    /// /
    public static HttpActionBuilder Http()
    {
        return new HttpActionBuilder();
    }

    /// Initiates an HTTP client action for use with the specified HTTP client.
    /// Dynamically builds a client-side HTTP action, allowing customization of headers,
    /// endpoints, and payload handling through a fluent API.
    /// <param name="httpClient">The HTTP client to be used for this action.</param>
    /// <returns>A builder instance for further configuration of the client action.</returns>
    public HttpClientActionBuilder Client(HttpClient httpClient)
    {
        var clientActionBuilder = new HttpClientActionBuilder(httpClient)
            .WithReferenceResolver(referenceResolver);
        _delegate = clientActionBuilder;
        return clientActionBuilder;
    }

    /// Initializes and returns an HTTP client action builder for configuring
    /// HTTP client operations with the specified HTTP client identifier.
    /// <param name="httpClient">The identifier of the HTTP client used to construct the action builder.</param>
    /// <returns>An instance of HttpClientActionBuilder to configure and build HTTP client actions.</returns>
    public HttpClientActionBuilder Client(string httpClient)
    {
        var clientActionBuilder = new HttpClientActionBuilder(httpClient)
            .WithReferenceResolver(referenceResolver);
        _delegate = clientActionBuilder;
        return clientActionBuilder;
    }

    /// Sets the bean reference resolver to be used for resolving references during the building process.
    /// <param name="referenceResolver">The reference resolver instance to set.</param>
    /// <return>This instance of HttpActionBuilder for method chaining.</return>
    public HttpActionBuilder WithReferenceResolver(IReferenceResolver referenceResolver)
    {
        this.referenceResolver = referenceResolver;
        return this;
    }

    public override ITestAction Build()
    {
        ObjectHelper.AssertNotNull(_delegate, "Missing delegate action to build");
        return _delegate.Build();
    }
}
