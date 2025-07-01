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
using Agenix.Api.Endpoint;
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Agenix.Core.Util;
using Agenix.GraphQL.Client;
using Agenix.GraphQL.Message;

namespace Agenix.GraphQL.Actions;

/// <summary>
/// Provides a fluent API for building and configuring GraphQL actions.
/// Supports initiating client GraphQL operations including queries, mutations, and subscriptions.
/// Extends behavior to resolve references dynamically during action construction.
/// </summary>
public class GraphQLClientActionBuilder : AbstractReferenceResolverAwareTestActionBuilder<ITestAction>
{
    /// Represents the target HTTP client that interacts with a defined endpoint.
    private readonly IEndpoint? _graphQLClient;

    private readonly string? _graphQLClientUri;

    /// <summary>
    /// Facilitates the creation and configuration of GraphQL client actions.
    /// Provides functionality to build and chain actions such as sending and receiving
    /// GraphQL operations, while supporting dynamic reference resolution.
    /// </summary>
    public GraphQLClientActionBuilder(IEndpoint graphQLClient)
    {
        _graphQLClient = graphQLClient;
    }

    /// <summary>
    /// Serves as a concrete implementation for building and configuring GraphQL client actions.
    /// Enables the setup of client operations such as sending, receiving, and subscription handling,
    /// while embedding tailored mechanisms for reference resolution support.
    /// </summary>
    public GraphQLClientActionBuilder(string graphQLClientUri)
    {
        _graphQLClientUri = graphQLClientUri;
    }

    /// <summary>
    /// Initiates the creation of a GraphQL client receive action.
    /// Configures and builds the receive action, which is used to handle responses from a GraphQL client.
    /// </summary>
    /// <returns>A builder for further configuration of the client receive action.</returns>
    public GraphQLClientReceiveActionBuilder Receive()
    {
        return new GraphQLClientReceiveActionBuilder(_graphQLClient, _graphQLClientUri, referenceResolver, _delegate);
    }

    /// <summary>
    /// Initiates the creation of a GraphQL client send action.
    /// Configures and builds the send action, which is used to define and execute requests for a GraphQL client.
    /// </summary>
    /// <returns>A builder for further configuration of the client send action.</returns>
    public GraphQLClientSendActionBuilder Send()
    {
        return new GraphQLClientSendActionBuilder(_graphQLClient, _graphQLClientUri, referenceResolver, _delegate);
    }

    /// <summary>
    /// Sets the bean reference resolver to be used for resolving references during the building process.
    /// </summary>
    /// <param name="referenceResolver">The reference resolver instance to set.</param>
    /// <returns>This instance of GraphQLClientActionBuilder for method chaining.</returns>
    public GraphQLClientActionBuilder WithReferenceResolver(IReferenceResolver referenceResolver)
    {
        this.referenceResolver = referenceResolver;
        return this;
    }

    /// <summary>
    /// Builds and returns the configured test action instance.
    /// Ensures that the delegate action is not null and invokes its build process.
    /// </summary>
    /// <returns>A fully constructed instance of a test action.</returns>
    public override ITestAction Build()
    {
        ObjectHelper.AssertNotNull(_delegate, "Missing delegate action to build");
        return _delegate.Build();
    }

    /// <summary>
    /// Facilitates the construction and configuration of actions for receiving responses from a GraphQL client or endpoint.
    /// Provides specialized methods to define how received GraphQL responses are processed, including integration with reference resolvers.
    /// </summary>
    public sealed class GraphQLClientReceiveActionBuilder(
        IEndpoint? httpClient,
        string? httpClientUri,
        IReferenceResolver referenceResolver,
        ITestActionBuilder<ITestAction> newDelegate)
    {
        /// Configures the GraphQL response action builder for receiving a response from an GraphQL client or URI.
        /// If the provided GraphQL client endpoint is not null, it sets the endpoint for the action builder
        /// using the specified GraphQL client. Otherwise, it sets the endpoint using the provided GraphQL client URI.
        /// The method also names the action as "http:receive-response" and applies the given reference resolver to the builder.
        /// <returns>
        ///     An instance of GraphQLClientResponseActionBuilder that can be further configured for receiving GraphQL responses.
        /// </returns>
        public GraphQLClientResponseActionBuilder Response()
        {
            var builder = new GraphQLClientResponseActionBuilder();
            if (httpClient != null)
            {
                builder.Endpoint(httpClient);
            }
            else
            {
                builder.Endpoint(httpClientUri);
            }

            builder.Name("graphql:receive-response");
            builder.WithReferenceResolver(referenceResolver);
            newDelegate = builder;
            return builder;
        }
    }

    /// <summary>
    /// Provides methods for building and configuring GraphQL client send actions.
    /// Enables the construction of client-side GraphQL send operations with specified parameters and options.
    /// Part of the fluent API for configuring and executing GraphQL interactions.
    /// </summary>
    public sealed class GraphQLClientSendActionBuilder(
        IEndpoint? httpClient,
        string? httpClientUri,
        IReferenceResolver referenceResolver,
        ITestActionBuilder<ITestAction> newDelegate)
    {
        /// <summary>
        /// Creates and configures a GraphQL client request action builder for executing a specified GraphQL operation.
        /// </summary>
        /// <param name="operation">The type of GraphQL operation to be executed (e.g., query, mutation, or subscription).</param>
        /// <param name="query">The GraphQL query or mutation string to execute, if applicable. Defaults to an empty string.</param>
        /// <returns>A configured GraphQL client request action builder instance.</returns>
        private GraphQLClientRequestActionBuilder Request(GraphQLOperationType operation, string query = "")
        {
            var builder = new GraphQLClientRequestActionBuilder();
            if (httpClient != null)
            {
                builder.Endpoint(httpClient);
            }
            else
            {
                builder.Endpoint(httpClientUri);
            }

            builder.Name("graphql:send-request");
            builder.WithReferenceResolver(referenceResolver);
            builder.OperationType(operation);

            if (StringUtils.HasText(query))
            {
                builder.Query(query);
            }


            newDelegate = builder;
            return builder;
        }

        /// <summary>
        /// Creates a new GraphQL query action.
        /// Configures and builds the action to execute a query against a GraphQL endpoint.
        /// </summary>
        /// <param name="query">The GraphQL query string to execute. Defaults to an empty string if not specified.</param>
        /// <returns>A builder that allows further configuration of a GraphQL client request action.</returns>
        public GraphQLClientRequestActionBuilder Query(string query = "")
        {
            return Request(GraphQLOperationType.QUERY, query);
        }

        /// <summary>
        /// Initiates the creation of a GraphQL client mutation action.
        /// Configures and builds the mutation action for sending GraphQL mutation requests.
        /// </summary>
        /// <param name="mutation">The GraphQL mutation string to be executed. Defaults to an empty string if not specified.</param>
        /// <returns>A builder for further configuration of the client mutation action.</returns>
        public GraphQLClientRequestActionBuilder Mutation(string mutation = "")
        {
            return Request(GraphQLOperationType.MUTATION, mutation);
        }

        /// <summary>
        /// Initiates the creation of a GraphQL client subscription action.
        /// Configures and builds the subscription action, which is used to handle real-time updates
        /// from a subscription-based GraphQL operation.
        /// </summary>
        /// <param name="subscription">The GraphQL subscription operation to be executed, represented as a string. Default is an empty string.</param>
        /// <returns>A builder for further configuration of the client subscription action.</returns>
        public GraphQLClientRequestActionBuilder Subscription(string subscription = "")
        {
            return Request(GraphQLOperationType.SUBSCRIPTION, subscription);
        }
    }
}
