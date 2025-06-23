
#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using Agenix.Core.Endpoint;
using GraphQL.Client.Serializer.Newtonsoft;

namespace Agenix.GraphQL.Client;

/// <summary>
/// Builder for creating and configuring GraphQL client endpoints with fluent API.
/// Provides methods to configure GraphQL-specific settings such as endpoint URLs,
/// authentication, retry policies, and WebSocket support for subscriptions.
/// </summary>
public class GraphQLClientBuilder : AbstractEndpointBuilder<GraphQLClient>
{
    /// Endpoint target
    private readonly GraphQLClient _endpoint = new();

    /// <summary>
    ///     Provides the implementation for retrieving the HTTP client endpoint instance
    ///     initialized within the builder.
    /// </summary>
    /// <returns>
    ///     The initialized instance of HttpClient representing the endpoint.
    /// </returns>
    protected override GraphQLClient GetEndpoint()
    {
        return _endpoint;
    }

    /// <summary>
    /// Sets the GraphQL endpoint URL for queries and mutations.
    /// </summary>
    /// <param name="endpointUrl">The GraphQL endpoint URL.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public GraphQLClientBuilder EndpointUrl(string endpointUrl)
    {
        _endpoint.EndpointConfiguration.EndpointUrl = endpointUrl;
        return this;
    }

    /// <summary>
    /// Sets the WebSocket endpoint URL for GraphQL subscriptions.
    /// If not specified, will be derived from the main endpoint URL.
    /// </summary>
    /// <param name="webSocketEndpointUrl">The WebSocket endpoint URL.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public GraphQLClientBuilder WebSocketEndpointUrl(string webSocketEndpointUrl)
    {
        _endpoint.EndpointConfiguration.WebSocketEndpointUrl = webSocketEndpointUrl;
        return this;
    }

    /// <summary>
    /// Configures whether to use WebSocket for GraphQL subscriptions.
    /// </summary>
    /// <param name="useWebSocket">True to use WebSocket for subscriptions, false otherwise.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public GraphQLClientBuilder UseWebSocketForSubscriptions(bool useWebSocket = true)
    {
        _endpoint.EndpointConfiguration.UseWebSocketForSubscriptions = useWebSocket;
        return this;
    }

    /// <summary>
    /// Sets the content type for GraphQL requests.
    /// </summary>
    /// <param name="contentType">The content type (default: application/json).</param>
    /// <returns>The builder instance for method chaining.</returns>
    public GraphQLClientBuilder ContentType(string contentType)
    {
        _endpoint.EndpointConfiguration.ContentType = contentType;
        return this;
    }

    /// <summary>
    /// Sets the character encoding for GraphQL communications.
    /// </summary>
    /// <param name="charset">The character encoding (default: UTF-8).</param>
    /// <returns>The builder instance for method chaining.</returns>
    public GraphQLClientBuilder Charset(string charset)
    {
        _endpoint.EndpointConfiguration.Charset = charset;
        return this;
    }

    /// <summary>
    /// Configures whether cookies should be handled automatically.
    /// </summary>
    /// <param name="handleCookies">True to handle cookies automatically, false otherwise.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public GraphQLClientBuilder HandleCookies(bool handleCookies = true)
    {
        _endpoint.EndpointConfiguration.HandleCookies = handleCookies;
        return this;
    }

    /// <summary>
    /// Adds a default header to be sent with every GraphQL request.
    /// </summary>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public GraphQLClientBuilder DefaultHeader(string name, string value)
    {
        _endpoint.EndpointConfiguration.DefaultHeaders[name] = value;
        return this;
    }

    /// <summary>
    /// Adds multiple default headers to be sent with every GraphQL request.
    /// </summary>
    /// <param name="headers">Dictionary of headers to add.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public GraphQLClientBuilder DefaultHeaders(IDictionary<string, string> headers)
    {
        foreach (var header in headers)
        {
            _endpoint.EndpointConfiguration.DefaultHeaders[header.Key] = header.Value;
        }
        return this;
    }

    /// <summary>
    /// Configures Bearer token authentication for GraphQL requests.
    /// </summary>
    /// <param name="token">The Bearer token.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public GraphQLClientBuilder BearerAuth(string token)
    {
        _endpoint.EndpointConfiguration.Authentication = new GraphQLEndpointConfiguration.GraphQLAuthenticationConfiguration
        {
            Type = "Bearer",
            Token = token
        };
        return this;
    }

    /// <summary>
    /// Configures Basic authentication for GraphQL requests.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public GraphQLClientBuilder BasicAuth(string username, string password)
    {
        _endpoint.EndpointConfiguration.Authentication = new GraphQLEndpointConfiguration.GraphQLAuthenticationConfiguration
        {
            Type = "Basic",
            Username = username,
            Password = password
        };
        return this;
    }

    /// <summary>
    /// Configures custom authentication headers for GraphQL requests.
    /// </summary>
    /// <param name="customHeaders">Dictionary of custom authentication headers.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public GraphQLClientBuilder CustomAuth(IDictionary<string, string> customHeaders)
    {
        if (_endpoint.EndpointConfiguration.Authentication == null)
        {
            _endpoint.EndpointConfiguration.Authentication = new GraphQLEndpointConfiguration.GraphQLAuthenticationConfiguration();
        }

        foreach (var header in customHeaders)
        {
            _endpoint.EndpointConfiguration.Authentication.CustomHeaders[header.Key] = header.Value;
        }
        return this;
    }

    /// <summary>
    /// Configures retry policy for failed GraphQL requests.
    /// </summary>
    /// <param name="maxRetries">Maximum number of retry attempts.</param>
    /// <param name="retryDelayMilliseconds">Delay between retries in milliseconds.</param>
    /// <param name="useExponentialBackoff">Whether to use exponential backoff.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public GraphQLClientBuilder RetryPolicy(int maxRetries, int retryDelayMilliseconds = 1000, bool useExponentialBackoff = true)
    {
        _endpoint.EndpointConfiguration.RetryPolicy = new GraphQLEndpointConfiguration.GraphQLRetryPolicy
        {
            MaxRetries = maxRetries,
            RetryDelayMilliseconds = retryDelayMilliseconds,
            UseExponentialBackoff = useExponentialBackoff
        };
        return this;
    }

    /// <summary>
    /// Configures custom HTTP status codes that should trigger a retry.
    /// </summary>
    /// <param name="statusCodes">List of HTTP status codes that should trigger retries.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public GraphQLClientBuilder RetryableStatusCodes(params int[] statusCodes)
    {
        _endpoint.EndpointConfiguration.RetryPolicy.RetryableStatusCodes = statusCodes.ToList();
        return this;
    }

    /// <summary>
    /// Configures JSON serialization options for GraphQL requests and responses.
    /// </summary>
    /// <param name="indentJson">Whether to indent JSON output for readability.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public GraphQLClientBuilder JsonSerializerOptions(NewtonsoftJsonSerializer serializer)
    {
        _endpoint.EndpointConfiguration.SerializerOptions = serializer;
        return this;
    }

    /// <summary>
    /// Adds a custom HTTP message handler to the GraphQL client pipeline.
    /// </summary>
    /// <param name="handler">The delegating handler to add.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public GraphQLClientBuilder ClientHandler(DelegatingHandler handler)
    {
        _endpoint.EndpointConfiguration.ClientHandlers.Add(handler);
        return this;
    }

    /// <summary>
    /// Adds multiple custom HTTP message handlers to the GraphQL client pipeline.
    /// </summary>
    /// <param name="handlers">The delegating handlers to add.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public GraphQLClientBuilder ClientHandlers(params DelegatingHandler[] handlers)
    {
        foreach (var handler in handlers)
        {
            _endpoint.EndpointConfiguration.ClientHandlers.Add(handler);
        }
        return this;
    }

    /// <summary>
    /// Sets the request timeout for GraphQL operations.
    /// </summary>
    /// <param name="timeoutMilliseconds">The timeout in milliseconds.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public GraphQLClientBuilder Timeout(int timeoutMilliseconds)
    {
        _endpoint.EndpointConfiguration.Timeout = timeoutMilliseconds;
        return this;
    }

    /// <summary>
    /// Builds and returns the configured GraphQL client instance.
    /// </summary>
    /// <returns>A configured GraphQL client.</returns>
    public override GraphQLClient Build()
    {
        return new GraphQLClient(_endpoint.EndpointConfiguration);
    }
}
