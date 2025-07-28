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

using Agenix.Core.Endpoint.Builder;
using Agenix.GraphQL.Client;
using Agenix.GraphQL.Server;

namespace Agenix.GraphQL.Endpoint.Builder;

/// <summary>
///     GraphQL endpoints builder that extends ClientServerEndpointBuilder.
///     Provides factory methods for creating GraphQL client and server endpoint builders.
/// </summary>
public sealed class GraphQLEndpoints : ClientServerEndpointBuilder<GraphQLClientBuilder, GraphQLServerBuilder>
{
    /// <summary>
    ///     Private constructor setting the client and server builder implementation.
    /// </summary>
    private GraphQLEndpoints() : base(new GraphQLClientBuilder(), new GraphQLServerBuilder())
    {
    }

    /// <summary>
    ///     Static entry method for GraphQL endpoint builder.
    /// </summary>
    /// <returns>A new GraphQLEndpoints instance</returns>
    public static GraphQLEndpoints GraphQL()
    {
        return new GraphQLEndpoints();
    }
}
