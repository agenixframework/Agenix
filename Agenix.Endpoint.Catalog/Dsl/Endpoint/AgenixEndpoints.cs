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

using Agenix.Core.Endpoint.Direct;
using Agenix.Endpoint.Catalog.Dsl.Endpoint.GraphQL;
using Agenix.Endpoint.Catalog.Dsl.Endpoint.Http;
using Agenix.Endpoint.Catalog.Dsl.Endpoint.Selenium;

namespace Agenix.Endpoint.Catalog.Dsl.Endpoint;

/// <summary>
///     Abstract base class for Agenix endpoint builders.
///     Provides static factory methods for creating various types of endpoints.
/// </summary>
public abstract class AgenixEndpoints
{
    /// <summary>
    ///     Prevent public instantiation.
    /// </summary>
    protected AgenixEndpoints()
    {
    }

    /// <summary>
    ///     Creates a new DirectEndpoint sync or async builder.
    /// </summary>
    /// <returns>A new DirectEndpoints instance</returns>
    public static DirectEndpoints Direct()
    {
        return DirectEndpoints.Direct();
    }

    /// <summary>
    ///     Creates a new HttpClient or HttpServer builder.
    /// </summary>
    /// <returns>A new HttpEndpointCatalog instance</returns>
    public static HttpEndpointCatalog Http()
    {
        return HttpEndpointCatalog.Http();
    }

    /// <summary>
    ///     Creates a new GraphQL endpoint builder.
    /// </summary>
    /// <returns>A new GraphQLEndpointCatalog instance</returns>
    public static GraphQLEndpointCatalog GraphQL()
    {
        return GraphQLEndpointCatalog.GraphQL();
    }

    /// <summary>
    ///     Creates a new Selenium endpoint builder.
    /// </summary>
    /// <returns>A new SeleniumEndpointCatalog instance</returns>
    public static SeleniumEndpointCatalog Selenium()
    {
        return SeleniumEndpointCatalog.Selenium();
    }
}
