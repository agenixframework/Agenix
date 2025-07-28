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

using System.Diagnostics.CodeAnalysis;
using Agenix.Http.Client;
using Agenix.Http.Endpoint.Builder;

namespace Agenix.Endpoint.Catalog.Dsl.Endpoint.Http;

/// <summary>
///     HTTP endpoint catalog for creating HTTP client builders.
///     Provides convenient access to HTTP endpoint functionality.
/// </summary>
public class HttpEndpointCatalog
{
    /// <summary>
    ///     Private constructor setting the client and server builder implementation.
    /// </summary>
    private HttpEndpointCatalog()
    {
        // prevent direct instantiation
    }

    /// <summary>
    ///     Static entry method for HTTP endpoint catalog.
    /// </summary>
    /// <returns>A new HttpEndpointCatalog instance</returns>
    public static HttpEndpointCatalog Http()
    {
        return new HttpEndpointCatalog();
    }

    /// <summary>
    ///     Gets the client builder.
    /// </summary>
    /// <returns>The HttpClientBuilder instance</returns>
    [SuppressMessage("csharpsquid", "S2325", Justification = "Intentionally non-static for fluent API pattern")]
    public HttpClientBuilder Client()
    {
        return HttpEndpoints.Http().Client();
    }
}
