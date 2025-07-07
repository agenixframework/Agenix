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

using System.Net;
using Agenix.Api;
using Agenix.Api.Endpoint;
using Agenix.Api.Spi;
using Agenix.Core.Util;

namespace Agenix.Http.Actions;

/// <summary>
///     Action executes http client operations such as sending requests and receiving responses.
/// </summary>
public class HttpClientActionBuilder : AbstractReferenceResolverAwareTestActionBuilder<ITestAction>
{
    /// Represents the target HTTP client that interacts with a defined endpoint.
    private readonly IEndpoint? _httpClient;

    private readonly string? _httpClientUri;

    /// Represents a builder for configuring and constructing HTTP client actions.
    /// This class provides methods to configure an HTTP client, send and receive HTTP messages,
    /// and resolve references for constructing test actions.
    public HttpClientActionBuilder(IEndpoint httpClient)
    {
        _httpClient = httpClient;
    }

    /// Represents a builder for configuring and constructing HTTP client actions.
    /// This class allows for sending and receiving HTTP messages, while enabling the usage
    /// of a reference resolver and delegation of actions for building test executions.
    public HttpClientActionBuilder(string httpClientUri)
    {
        _httpClientUri = httpClientUri;
    }

    /// Sets a new reference resolver for the HTTP client action builder.
    /// This allows the action builder to resolve references to objects or resources using the provided resolver.
    /// The method assigns the provided reference resolver to the builder's internal referenceResolver field.
    /// <param name="newReferenceResolver">
    ///     The reference resolver to be used for resolving references in the HTTP client action builder.
    /// </param>
    /// <returns>
    ///     The current instance of HttpClientActionBuilder with the updated reference resolver, allowing for further
    ///     configuration.
    /// </returns>
    public HttpClientActionBuilder WithReferenceResolver(IReferenceResolver newReferenceResolver)
    {
        referenceResolver = newReferenceResolver;
        return this;
    }

    /// Creates a new instance of HttpClientReceiveActionBuilder for configuring HTTP receive operations.
    /// This method initializes the receive action builder with the current HTTP client, URI, reference resolver,
    /// and delegate, enabling further specification of HTTP receive-related actions.
    /// <returns>
    ///     An instance of HttpClientReceiveActionBuilder configured with the current HTTP client and context,
    ///     allowing the user to define receive-specific actions.
    /// </returns>
    public HttpClientReceiveActionBuilder Receive()
    {
        return new HttpClientReceiveActionBuilder(_httpClient, _httpClientUri, referenceResolver, _delegate);
    }

    public HttpClientSendActionBuilder Send()
    {
        return new HttpClientSendActionBuilder(_httpClient, _httpClientUri, referenceResolver, _delegate);
    }

    /// Builds and returns an instance of an ITestAction.
    /// This method ensures the delegate action has been properly configured before creating the test action.
    /// <return>The built instance of an ITestAction.</return>
    public override ITestAction Build()
    {
        ObjectHelper.AssertNotNull(_delegate, "Missing delegate action to build");
        return _delegate.Build();
    }

    /// <summary>
    ///     Configures actions for receiving HTTP responses from an HTTP client or URI.
    /// </summary>
    /// <remarks>
    ///     This builder allows for detailed configuration of how HTTP responses are handled
    ///     when interacting with an HTTP client or endpoint.
    /// </remarks>
    public sealed class HttpClientReceiveActionBuilder(
        IEndpoint? httpClient,
        string? httpClientUri,
        IReferenceResolver referenceResolver,
        ITestActionBuilder<ITestAction> newDelegate)
    {
        /// Configures the HTTP response action builder for receiving a response from an HTTP client or URI.
        /// If the provided HTTP client endpoint is not null, it sets the endpoint for the action builder
        /// using the specified HTTP client. Otherwise, it sets the endpoint using the provided HTTP client URI.
        /// The method also names the action as "http:receive-response" and applies the given reference resolver to the builder.
        /// <returns>
        ///     An instance of HttpClientResponseActionBuilder that can be further configured for receiving HTTP responses.
        /// </returns>
        public HttpClientResponseActionBuilder Response()
        {
            var builder = new HttpClientResponseActionBuilder();
            if (httpClient != null)
            {
                builder.Endpoint(httpClient);
            }
            else
            {
                builder.Endpoint(httpClientUri);
            }

            builder.Name("http:receive-response");
            builder.WithReferenceResolver(referenceResolver);
            newDelegate = builder;
            return builder;
        }

        /// Configures the HTTP response action with the specified HTTP status code.
        /// This method creates and initializes a new instance of HttpClientResponseActionBuilder
        /// with the provided status code. The method also assigns the relevant endpoint,
        /// updates the action name, applies the reference resolver, and sets the HTTP status
        /// code in the message builder.
        /// <param name="status">
        ///     The HTTP status code to set for the response action, represented as a System.Net.HttpStatusCode.
        /// </param>
        /// <returns>
        ///     An instance of HttpClientResponseActionBuilder, configured for handling the specified HTTP status code.
        /// </returns>
        public HttpClientResponseActionBuilder Response(HttpStatusCode status)
        {
            var builder = new HttpClientResponseActionBuilder();
            if (httpClient != null)
            {
                builder.Endpoint(httpClient);
            }
            else
            {
                builder.Endpoint(httpClientUri);
            }

            builder.Name("http:receive-response");
            builder.WithReferenceResolver(referenceResolver);
            builder.Message().Status(status);
            newDelegate = builder;
            return builder;
        }
    }

    /// <summary>
    ///     Builder class providing fluent interface methods for constructing and sending HTTP requests.
    /// </summary>
    /// <remarks>
    ///     Supports various HTTP methods such as GET, POST, PUT, DELETE, HEAD, OPTIONS, TRACE, and PATCH.
    ///     Allows sending requests with or without specified paths.
    /// </remarks>
    public sealed class HttpClientSendActionBuilder(
        IEndpoint? httpClient,
        string? httpClientUri,
        IReferenceResolver referenceResolver,
        ITestActionBuilder<ITestAction> newDelegate)
    {
        /// Configures and initiates an HTTP request with the specified HTTP method and optional path.
        /// This method sets up the necessary configurations including the HTTP method, endpoint, resolver, and path
        /// for the HTTP client request and returns a builder instance for further customization.
        /// <param name="method">
        ///     The HTTP method for the request, e.g., GET, POST, PUT, etc.
        /// </param>
        /// <param name="path">
        ///     The optional path for the HTTP request. Can be null or empty if a path is not required.
        /// </param>
        /// <returns>
        ///     An instance of HttpClientRequestActionBuilder configured with the specified HTTP method and path.
        ///     This allows further chaining and customization of the HTTP request action.
        /// </returns>
        private HttpClientRequestActionBuilder Request(HttpMethod method, string? path)
        {
            var builder = new HttpClientRequestActionBuilder();
            if (httpClient != null)
            {
                builder.Endpoint(httpClient);
            }
            else
            {
                builder.Endpoint(httpClientUri);
            }

            builder.Name("http:send-request");
            builder.WithReferenceResolver(referenceResolver);
            builder.Method(method);

            if (!string.IsNullOrWhiteSpace(path))
            {
                builder.Path(path);
            }

            newDelegate = builder;
            return builder;
        }

        /// Initiates the process of sending an HTTP GET request to the server.
        /// This method creates a new HTTP GET request without a predefined path and prepares it for further configuration or execution.
        /// <returns>
        ///     A new instance of HttpClientRequestActionBuilder configured for an HTTP GET request.
        /// </returns>
        public HttpClientRequestActionBuilder Get()
        {
            return Request(HttpMethod.Get, null);
        }

        /// Sends an HTTP GET request to the specified path as a server client.
        /// This method uses the GET HTTP method to send requests to the server.
        /// <param name="path">
        ///     The specific path for the HTTP GET request. This can include query parameters or other details
        ///     relevant to the resource being accessed on the server.
        /// </param>
        /// <returns>
        ///     An instance of HttpClientRequestActionBuilder that allows for additional configuration
        ///     of the request or the ability to send the constructed request.
        /// </returns>
        public HttpClientRequestActionBuilder Get(string path)
        {
            return Request(HttpMethod.Get, path);
        }

        /// Sends an HTTP POST request as the client to the server.
        /// This method constructs an HTTP POST request without specifying a path
        /// and returns a builder for further customization or execution of the request.
        /// <returns>
        ///     An instance of HttpClientRequestActionBuilder configured for sending an HTTP POST request.
        /// </returns>
        public HttpClientRequestActionBuilder Post()
        {
            return Request(HttpMethod.Post, null);
        }

        /// Sends an HTTP POST request to the specified path as a client to the server.
        /// This method constructs the request using the POST HTTP method and includes the given relative path for the resource.
        /// <param name="path">
        ///     The relative path of the resource to which the POST request is sent.
        /// </param>
        /// <returns>
        ///     A new instance of HttpClientRequestActionBuilder configured to send the POST request to the specified path.
        ///     This allows further customization of the request before execution.
        /// </returns>
        public HttpClientRequestActionBuilder Post(string path)
        {
            return Request(HttpMethod.Post, path);
        }

        /// Sends an HTTP PUT request as a client to a server.
        /// This method prepares a PUT request without specifying a path. The request is constructed
        /// using the HTTP client and configuration defined in the builder.
        /// <returns>
        ///     A new instance of HttpClientRequestActionBuilder to further customize and finalize the HTTP PUT request.
        /// </returns>
        public HttpClientRequestActionBuilder Put()
        {
            return Request(HttpMethod.Put, null);
        }

        /// Sends an HTTP PUT request to the specified path.
        /// This method constructs a PUT request using the provided path and prepares it
        /// for further configuration or execution within the HTTP client action builder.
        /// <param name="path">
        ///     The URI path to which the HTTP PUT request will be sent. This path can be relative to the base URI of the HTTP
        ///     client.
        /// </param>
        /// <returns>
        ///     A new instance of HttpClientRequestActionBuilder configured for an HTTP PUT request
        ///     to the specified path. Allows for further customization of the request before sending.
        /// </returns>
        public HttpClientRequestActionBuilder Put(string path)
        {
            return Request(HttpMethod.Put, path);
        }

        /// Sends an HTTP DELETE request to the server as a client.
        /// This method initializes a DELETE request without specifying a path.
        /// <returns>
        ///     A new instance of HttpClientRequestActionBuilder configured for sending an HTTP DELETE request.
        /// </returns>
        public HttpClientRequestActionBuilder Delete()
        {
            return Request(HttpMethod.Delete, null);
        }

        /// Sends an HTTP DELETE request to the specified path as the client to the server.
        /// <param name="path">
        ///     The relative or absolute path to which the DELETE request is sent. This path is appended to the base URI of the
        ///     HTTP client.
        /// </param>
        /// <returns>
        ///     A new instance of HttpClientRequestActionBuilder that allows further configuration or the execution of the HTTP
        ///     DELETE request.
        /// </returns>
        public HttpClientRequestActionBuilder Delete(string path)
        {
            return Request(HttpMethod.Delete, path);
        }

        /// Sends an HTTP HEAD request to the server without specifying a path.
        /// This method constructs an HTTP request with the HEAD method and sends it to the server.
        /// <returns>
        ///     The current instance of HttpClientRequestActionBuilder configured to send an HTTP HEAD request,
        ///     allowing for further customization or execution of the request.
        /// </returns>
        public HttpClientRequestActionBuilder Head()
        {
            return Request(HttpMethod.Head, null);
        }

        /// Sends an HTTP HEAD request with the specified path to the server.
        /// Allows for fluent configuration of the HTTP request using the returned builder.
        /// <param name="path">
        ///     The relative path to be appended to the base URI for the HTTP HEAD request.
        /// </param>
        /// <returns>
        ///     An instance of HttpClientRequestActionBuilder, enabling further configuration of the HTTP request.
        /// </returns>
        public HttpClientRequestActionBuilder Head(string path)
        {
            return Request(HttpMethod.Head, path);
        }

        /// Sends an HTTP OPTIONS request to the server.
        /// This method constructs and prepares an HTTP OPTIONS request without
        /// any specific path, allowing for server-wide OPTIONS queries.
        /// <returns>
        ///     The current instance of HttpClientRequestActionBuilder configured
        ///     to send an OPTIONS request, enabling additional setup before execution.
        /// </returns>
        public HttpClientRequestActionBuilder Options()
        {
            return Request(HttpMethod.Options, null);
        }

        /// Sends an HTTP OPTIONS request to the specified path.
        /// This method allows the client to query the server for communication options available.
        /// <param name="path">
        ///     The relative path to send the OPTIONS request to. This path is appended to the base URI of the HTTP client.
        /// </param>
        /// <returns>
        ///     An instance of HttpClientRequestActionBuilder configured for an OPTIONS request. This allows further customization
        ///     and execution of the request.
        /// </returns>
        public HttpClientRequestActionBuilder Options(string path)
        {
            return Request(HttpMethod.Options, path);
        }

        /// Sends an HTTP TRACE request to the server.
        /// This method constructs an HTTP TRACE request, which is typically used to obtain diagnostic information
        /// about the connection between the client and the server. TRACE requests are processed by the server and
        /// the response returns the exact content that was received.
        /// <returns>
        ///     The current instance of HttpClientRequestActionBuilder configured for sending a TRACE request.
        ///     This allows for further customization or direct invocation of the request
        /// </returns>
        public HttpClientRequestActionBuilder Trace()
        {
            return Request(HttpMethod.Trace, null);
        }

        /// Sends an HTTP TRACE request to the server with the specified path.
        /// The TRACE method allows the client to see what is being received at the other end of the request chain, useful for testing or diagnostic purposes.
        /// <param name="path">
        ///     The resource path to which the TRACE request will be sent. This is appended to the base URI of the HTTP client.
        /// </param>
        /// <returns>
        ///     An instance of HttpClientRequestActionBuilder to enable further configuration of the TRACE request.
        /// </returns>
        public HttpClientRequestActionBuilder Trace(string path)
        {
            return Request(HttpMethod.Trace, path);
        }

        /// Sends an HTTP PATCH request to the server without specifying a path.
        /// The method builds the request and returns an instance of the HttpClientRequestActionBuilder
        /// for further configuration and execution of the HTTP PATCH request.
        /// <returns>
        ///     An instance of HttpClientRequestActionBuilder configured for an HTTP PATCH request.
        /// </returns>
        public HttpClientRequestActionBuilder Patch()
        {
            return Request(HttpMethod.Patch, null);
        }

        /// Sends an HTTP PATCH request to the specified path.
        /// This method constructs a PATCH request by resolving the given path and preparing it as part of the HTTP client's request flow.
        /// <param name="path">
        ///     The relative or absolute path for the PATCH request. This is used to determine the target endpoint of the request.
        /// </param>
        /// <returns>
        ///     An instance of HttpClientRequestActionBuilder configured for sending the PATCH request.
        /// </returns>
        public HttpClientRequestActionBuilder Patch(string path)
        {
            return Request(HttpMethod.Patch, path);
        }
    }
}
