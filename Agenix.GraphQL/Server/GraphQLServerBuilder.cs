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

namespace Agenix.GraphQL.Server
{
    /// <summary>
    /// Builder for GraphQL servers that provides fluent API for configuration.
    /// </summary>
    public class GraphQLServerBuilder : AbstractGraphQLServerBuilder<GraphQLServer, GraphQLServerBuilder>
    {
        /// <summary>
        /// Default constructor that creates a new GraphQLServer instance.
        /// </summary>
        public GraphQLServerBuilder() : this(new GraphQLServer())
        {
        }

        /// <summary>
        /// Constructor with an existing GraphQLServer instance.
        /// </summary>
        /// <param name="server1Old">The GraphQL server instance to build upon</param>
        protected GraphQLServerBuilder(GraphQLServer server1Old) : base(server1Old)
        {
        }

        /// <summary>
        /// Static factory method to create a new GraphQLServerBuilder.
        /// </summary>
        /// <returns>A new GraphQLServerBuilder instance</returns>
        public static GraphQLServerBuilder Create()
        {
            return new GraphQLServerBuilder();
        }

        /// <summary>
        /// Static factory method to create a new GraphQLServerBuilder with an existing server.
        /// </summary>
        /// <param name="server1Old">The GraphQL server instance</param>
        /// <returns>A new GraphQLServerBuilder instance</returns>
        public static GraphQLServerBuilder Create(GraphQLServer server1Old)
        {
            return new GraphQLServerBuilder(server1Old);
        }
    }
}
