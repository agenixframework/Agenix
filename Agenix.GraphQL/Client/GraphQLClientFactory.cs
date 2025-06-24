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

namespace Agenix.GraphQL.Client;

/// <summary>
///     Represents a factory for creating message handlers for GraphQL operations.
///     This factory allows the configuration of multiple delegating handlers that form a pipeline
///     for processing GraphQL HTTP requests and responses.
/// </summary>
public class GraphQLClientFactory
{
    private readonly List<DelegatingHandler> _clientHandlers = [];

    /// <summary>
    ///     Creates a message handler pipeline that can be used by GraphQL clients.
    ///     This is the primary method used for GraphQL client configuration.
    /// </summary>
    /// <returns>A configured HttpMessageHandler for use with GraphQLHttpClient.</returns>
    public HttpMessageHandler CreateMessageHandler()
    {
        return CreateHandlerPipeline(new HttpClientHandler(), _clientHandlers);
    }

    /// <summary>
    ///     Adds a DelegatingHandler to the collection of handlers within the GraphQLClientFactory.
    ///     This handler will be part of the pipeline used by the GraphQL clients created by the factory.
    /// </summary>
    /// <param name="handler">The DelegatingHandler instance to be added to the collection of handlers.</param>
    /// <returns>The modified GraphQLClientFactory instance, allowing for method chaining.</returns>
    public GraphQLClientFactory AddHandler(DelegatingHandler handler)
    {
        _clientHandlers.Add(handler);
        return this;
    }

    /// <summary>
    ///     Adds a collection of DelegatingHandler instances to the existing set of handlers in the GraphQLClientFactory.
    ///     These handlers will be incorporated into the request processing pipeline of GraphQL clients created by the
    ///     factory.
    /// </summary>
    /// <param name="handlers">The list of DelegatingHandler instances to be added to the collection.</param>
    /// <returns>The modified GraphQLClientFactory instance, allowing for method chaining.</returns>
    public GraphQLClientFactory AddHandlers(List<DelegatingHandler> handlers)
    {
        _clientHandlers.AddRange(handlers);
        return this;
    }

    /// <summary>
    ///     Creates a handler pipeline by chaining a collection of DelegatingHandler instances with a specified inner handler.
    ///     It constructs the pipeline by setting each handler's InnerHandler property to the next handler in the sequence,
    ///     with the specified inner handler serving as the final handler in the chain.
    /// </summary>
    /// <param name="innerHandler">
    ///     The innermost handler, typically an instance of <see cref="HttpMessageHandler" />, which
    ///     processes the HTTP response and request.
    /// </param>
    /// <param name="handlers">An enumerable collection of DelegatingHandler instances to be chained together in the pipeline.</param>
    /// <returns>
    ///     A <see cref="HttpMessageHandler" /> that represents the completed handler chain, starting with the first
    ///     handler in the collection and ending with the specified inner handler.
    /// </returns>
    private HttpMessageHandler CreateHandlerPipeline(HttpMessageHandler innerHandler,
        IEnumerable<DelegatingHandler> handlers)
    {
        var current = innerHandler;
        foreach (var handler in handlers)
        {
            handler.InnerHandler = current;
            current = handler;
        }

        return current;
    }
}
