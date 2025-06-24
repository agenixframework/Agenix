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

using Agenix.Api.Context;
using Agenix.Api.Endpoint;
using Agenix.Core.Endpoint;

namespace Agenix.GraphQL.Client;

/// <summary>
/// Represents a component responsible for creating GraphQL client endpoints based on a given URI resource
/// and associated parameters. It allows configuration of GraphQL-specific endpoint settings and supports
/// various GraphQL-related options such as operation types, WebSocket connections, and subscription handling.
/// Supports both HTTP and HTTPS protocols for secure communication.
/// </summary>
public class GraphQLEndpointComponent : AbstractEndpointComponent
{
    /// <summary>
    /// Represents a component responsible for creating GraphQL client endpoints based on a given URI resource
    /// and associated parameters. It allows configuration of GraphQL-specific endpoint settings and supports
    /// various GraphQL-related options such as operation types, WebSocket connections, and subscription handling.
    /// </summary>
    public GraphQLEndpointComponent() : this("graphql")
    {
    }

    /// <summary>
    /// Represents a specialized component used for creating GraphQL client endpoints based on specific
    /// resource paths and parameters. This component handles configuration and initialization of
    /// GraphQL endpoints, enabling customization of options such as operation types, WebSocket usage,
    /// and subscription management.
    /// </summary>
    /// <param name="name">The name identifier for this GraphQL endpoint component.</param>
    public GraphQLEndpointComponent(string name) : base(name)
    {
    }

    /// <summary>
    /// Gets the URI scheme used for GraphQL endpoints.
    /// Returns "http://" by default, but can be overridden for HTTPS support.
    /// </summary>
    /// <returns>A string representing the scheme (e.g., "http://").</returns>
    protected virtual string Scheme => "http://";

    /// <summary>
    /// Gets the WebSocket URI scheme used for GraphQL subscriptions.
    /// Returns "ws://" by default, but can be overridden for secure WebSocket support.
    /// </summary>
    /// <returns>A string representing the WebSocket scheme (e.g., "ws://").</returns>
    protected virtual string WebSocketScheme => "ws://";

    /// <summary>
    /// Creates a GraphQL endpoint based on the provided resource path, parameters, and execution context.
    /// This method constructs an endpoint with configuration details defined in the parameters and
    /// customizes specific GraphQL options such as the operation type, WebSocket usage, and subscription settings.
    /// Automatically handles both HTTP and HTTPS protocols based on the secure parameter or resource path.
    /// </summary>
    /// <param name="resourcePath">The relative path for the resource to be accessed by the GraphQL endpoint.</param>
    /// <param name="parameters">A dictionary of parameters used to configure the endpoint, including GraphQL-specific options.</param>
    /// <param name="context">The context for the test environment, providing configuration or dependencies.</param>
    /// <returns>An instance of IEndpoint configured as a GraphQL client endpoint.</returns>
    protected override IEndpoint CreateEndpoint(string resourcePath, IDictionary<string, string> parameters,
        TestContext context)
    {
        var client = new GraphQLClient();

        // Determine if HTTPS should be used
        var useHttps = DetermineHttpsUsage(resourcePath, parameters);
        var httpScheme = useHttps ? "https://" : Scheme;
        var wsScheme = useHttps ? "wss://" : WebSocketScheme;

        // Set the main endpoint URL
        client.EndpointConfiguration.EndpointUrl = httpScheme + resourcePath +
                                                 GetParameterString(parameters, typeof(GraphQLEndpointConfiguration));

        // Handle GraphQL-specific parameters
        if (parameters.Remove("useWebSocketForSubscriptions", out var useWebSocketValue))
        {
            if (bool.TryParse(useWebSocketValue, out var useWebSocket))
            {
                client.EndpointConfiguration.UseWebSocketForSubscriptions = useWebSocket;
                if (useWebSocket)
                {
                    client.EndpointConfiguration.WebSocketEndpointUrl = wsScheme + resourcePath +
                                                                      GetParameterString(parameters, typeof(GraphQLEndpointConfiguration));
                }
            }
        }

        if (parameters.Remove("charset", out var charsetValue))
        {
            client.EndpointConfiguration.Charset = charsetValue;
        }

        if (parameters.Remove("contentType", out var contentTypeValue))
        {
            client.EndpointConfiguration.ContentType = contentTypeValue;
        }

        if (parameters.Remove("handleCookies", out var handleCookiesValue))
        {
            if (bool.TryParse(handleCookiesValue, out var handleCookies))
            {
                client.EndpointConfiguration.HandleCookies = handleCookies;
            }
        }

        if (parameters.Remove("webSocketEndpointUrl", out var webSocketUrlValue))
        {
            client.EndpointConfiguration.WebSocketEndpointUrl = webSocketUrlValue;
        }

        // Authentication configuration
        ConfigureAuthentication(client.EndpointConfiguration, parameters);

        // Retry policy configuration
        ConfigureRetryPolicy(client.EndpointConfiguration, parameters);

        // Remove the secure parameter if it exists (already processed)
        parameters.Remove("secure");

        EnrichEndpointConfiguration(
            client.EndpointConfiguration,
            GetEndpointConfigurationParameters(parameters, typeof(GraphQLEndpointConfiguration)),
            context);

        return client;
    }

    /// <summary>
    /// Determines whether HTTPS should be used based on the resource path and parameters.
    /// </summary>
    /// <param name="resourcePath">The resource path that may contain protocol information.</param>
    /// <param name="parameters">Parameters that may contain a 'secure' flag.</param>
    /// <returns>True if HTTPS should be used, false otherwise.</returns>
    private static bool DetermineHttpsUsage(string resourcePath, IDictionary<string, string> parameters)
    {
        // Check if a resource path already contains a protocol
        if (resourcePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            resourcePath.StartsWith("wss://", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (resourcePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            resourcePath.StartsWith("ws://", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Check for explicit secure parameter
        if (parameters.TryGetValue("secure", out var secureValue))
        {
            return bool.TryParse(secureValue, out var secure) && secure;
        }

        // Check for port 443 (standard HTTPS port)
        return resourcePath.Contains(":443");
        // Default to HTTP for backward compatibility
    }

    /// <summary>
    /// Configures authentication settings from parameters.
    /// </summary>
    /// <param name="config">The GraphQL endpoint configuration.</param>
    /// <param name="parameters">The parameters dictionary.</param>
    private static void ConfigureAuthentication(GraphQLEndpointConfiguration config, IDictionary<string, string> parameters)
    {
        var hasAuthParams = parameters.ContainsKey("authType") ||
                            parameters.ContainsKey("authToken") ||
                            parameters.ContainsKey("authUsername") ||
                            parameters.ContainsKey("authPassword");

        if (!hasAuthParams) return;

        config.Authentication ??= new GraphQLEndpointConfiguration.GraphQLAuthenticationConfiguration();

        if (parameters.Remove("authType", out var authTypeValue))
        {
            config.Authentication.Type = authTypeValue;
        }

        if (parameters.Remove("authToken", out var authTokenValue))
        {
            config.Authentication.Token = authTokenValue;
        }

        if (parameters.Remove("authUsername", out var authUsernameValue))
        {
            config.Authentication.Username = authUsernameValue;
        }

        if (parameters.Remove("authPassword", out var authPasswordValue))
        {
            config.Authentication.Password = authPasswordValue;
        }
    }

    /// <summary>
    /// Configures retry policy settings from parameters.
    /// </summary>
    /// <param name="config">The GraphQL endpoint configuration.</param>
    /// <param name="parameters">The parameter dictionary.</param>
    private static void ConfigureRetryPolicy(GraphQLEndpointConfiguration config, IDictionary<string, string> parameters)
    {
        if (parameters.Remove("maxRetries", out var maxRetriesValue))
        {
            if (int.TryParse(maxRetriesValue, out var maxRetries))
            {
                config.RetryPolicy.MaxRetries = maxRetries;
            }
        }

        if (parameters.Remove("retryDelay", out var retryDelayValue))
        {
            if (int.TryParse(retryDelayValue, out var retryDelay))
            {
                config.RetryPolicy.RetryDelayMilliseconds = retryDelay;
            }
        }

        if (parameters.Remove("useExponentialBackoff", out var useExponentialBackoffValue))
        {
            if (bool.TryParse(useExponentialBackoffValue, out var useExponentialBackoff))
            {
                config.RetryPolicy.UseExponentialBackoff = useExponentialBackoff;
            }
        }
    }
}
