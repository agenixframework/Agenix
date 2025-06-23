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

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Agenix.Api.Context;
using Agenix.Api.Message;
using Agenix.GraphQL.Client;
using GraphQL;
using GraphQL.Client.Http;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Agenix.GraphQL.Message;

/// <summary>
///     Converts GraphQL messages to GraphQL requests and GraphQL responses back to messages.
/// </summary>
public class GraphQLMessageConverter : IMessageConverter<GraphQLResponse<object>, GraphQLRequest, GraphQLEndpointConfiguration>
{
    /// <summary>
    ///     Converts an inbound GraphQL response to a GraphQL message.
    /// </summary>
    /// <param name="inbound">The GraphQL response to convert.</param>
    /// <param name="endpointConfiguration">The GraphQL endpoint configuration.</param>
    /// <param name="context">The test context.</param>
    /// <returns>A GraphQL message containing the response data.</returns>
    public IMessage ConvertInbound(GraphQLResponse<object> inbound, GraphQLEndpointConfiguration endpointConfiguration,
        TestContext context)
    {
        var message = new GraphQLMessage(JsonConvert.SerializeObject(inbound.Data),
            GetHeaders(inbound.AsGraphQLHttpResponse().ResponseHeaders));

        // Set the status code
        message.Status(inbound.AsGraphQLHttpResponse().StatusCode);

        // Handle errors
        if (inbound.Errors?.Any() == true)
        {
            message.SetHeader(GraphQLMessageHeaders.HasErrors, "true");
            message.SetHeader(GraphQLMessageHeaders.ErrorCount, inbound.Errors.Length.ToString());

            var errorMessages = inbound.Errors.Select(e => e.Message).ToList();
            message.SetHeader(GraphQLMessageHeaders.ErrorMessages, string.Join("; ", errorMessages));
        }

        // Set extensions if present
        if (inbound.Extensions?.Any() == true)
        {
            message.Extensions = inbound.Extensions;
        }


        // Set standard headers
        message.SetHeader(GraphQLMessageHeaders.ContentType, endpointConfiguration.ContentType);
        message.SetHeader(GraphQLMessageHeaders.Charset, endpointConfiguration.Charset);

        return message;
    }

    /// <summary>
    ///     Converts an outbound GraphQL message to a GraphQLRequest for sending to a GraphQL endpoint.
    /// </summary>
    /// <param name="message">The GraphQL message to convert.</param>
    /// <param name="endpointConfiguration">The GraphQL endpoint configuration.</param>
    /// <param name="context">The test context.</param>
    /// <returns>A GraphQLRequest ready to be sent.</returns>
    public GraphQLRequest ConvertOutbound(IMessage message, GraphQLEndpointConfiguration endpointConfiguration,
        TestContext context)
    {
        var graphQLMessage = message as GraphQLMessage ??
                             throw new ArgumentException("Message must be a GraphQLMessage", nameof(message));

        var request = new GraphQLRequest
        {
            Query = graphQLMessage.GetPayload<string>(),
            OperationName = graphQLMessage.GetOperationName(),
            Variables = ConvertVariables(graphQLMessage.Variables),
            Extensions = graphQLMessage.Extensions
        };

        // Handle authentication
        ApplyAuthentication(request, graphQLMessage, endpointConfiguration);

        // Apply default headers from configuration
        ApplyDefaultHeaders(request, endpointConfiguration);

        // Apply message-specific headers
        ApplyMessageHeaders(request, graphQLMessage);

        // Apply cookies if needed
        ApplyCookies(request, graphQLMessage, endpointConfiguration);

        return request;
    }

    /// <summary>
    ///     Converts an outbound GraphQL request to a GraphQL message (for response handling).
    /// </summary>
    /// <param name="outbound">The GraphQL request to convert.</param>
    /// <param name="message">The original message.</param>
    /// <param name="endpointConfiguration">The GraphQL endpoint configuration.</param>
    /// <param name="context">The test context.</param>
    public void ConvertOutbound(GraphQLRequest outbound, IMessage message,
        GraphQLEndpointConfiguration endpointConfiguration, TestContext context)
    {
        var graphQLMessage = message as GraphQLMessage ??
                             throw new ArgumentException("Message must be a GraphQLMessage", nameof(message));

        // Update the message with request details
        graphQLMessage.Payload = outbound.Query;
        graphQLMessage.SetOperationName(outbound.OperationName);
        graphQLMessage.Variables = outbound.Variables;
        graphQLMessage.Extensions = outbound.Extensions;

        // Set operation type header
        if (!string.IsNullOrEmpty(outbound.Query))
        {
            var operationType = DetermineOperationType(outbound.Query);
            graphQLMessage.SetHeader(GraphQLMessageHeaders.OperationType, operationType);
        }

        // Set operation name header
        if (!string.IsNullOrEmpty(outbound.OperationName))
        {
            graphQLMessage.SetHeader(GraphQLMessageHeaders.OperationName, outbound.OperationName);
        }
    }

    /// <summary>
    ///     Extracts and converts HTTP headers into a dictionary of string keys with object values.
    /// </summary>
    /// <param name="httpHeaders">The collection of HTTP headers to process.</param>
    /// <returns>A dictionary representing the headers with their respective values.</returns>
    private Dictionary<string, object> GetHeaders(HttpHeaders httpHeaders)
    {
        var customHeaders = new Dictionary<string, object>();

        foreach (var header in httpHeaders)
        {
            customHeaders[header.Key] = string.Join(",", header.Value);
        }

        return customHeaders;
    }

    /// <summary>
    ///     Determines the operation type from a GraphQL query string.
    /// </summary>
    /// <param name="query">The GraphQL query string.</param>
    /// <returns>The operation type (Query, Mutation, Subscription).</returns>
    private static string DetermineOperationType(string query)
    {
        var trimmedQuery = query.Trim();

        if (trimmedQuery.StartsWith(nameof(GraphQLOperationType.MUTATION), StringComparison.OrdinalIgnoreCase))
        {
            return "Mutation";
        }

        return trimmedQuery.StartsWith(nameof(GraphQLOperationType.SUBSCRIPTION), StringComparison.OrdinalIgnoreCase)
            ? "Subscription"
            : "Query"; // Default to Query
    }

    /// <summary>
    ///     Converts variables object to a format suitable for GraphQLRequest.
    /// </summary>
    /// <param name="variables">The variables object.</param>
    /// <returns>Variables in the correct format.</returns>
    private static object? ConvertVariables(object? variables)
    {
        if (variables == null)
        {
            return null;
        }

        // If it's already a dictionary, return as-is
        if (variables is Dictionary<string, object?> dict)
        {
            return dict;
        }

        // If it's a JSON string, parse it
        if (variables is string jsonString)
        {
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object?>>(jsonString);
            }
            catch
            {
                return variables; // Return as-is if parsing fails
            }
        }

        return variables;
    }

    /// <summary>
    ///     Applies authentication configuration to the GraphQL request.
    /// </summary>
    /// <param name="request">The GraphQL request.</param>
    /// <param name="message">The GraphQL message.</param>
    /// <param name="endpointConfiguration">The endpoint configuration.</param>
    private static void ApplyAuthentication(GraphQLRequest request, GraphQLMessage message,
        GraphQLEndpointConfiguration endpointConfiguration)
    {
        // Check message-level authorization first
        if (message.GetHeader(GraphQLMessageHeaders.Authorization) is string authHeader)
        {
            // Authorization will be handled at the HTTP level
            return;
        }

        // Apply configuration-level authentication
        var auth = endpointConfiguration.Authentication;
        if (auth == null)
        {
            return;
        }

        switch (auth.Type.ToUpperInvariant())
        {
            case "BEARER":
                if (!string.IsNullOrEmpty(auth.Token))
                {
                    message.SetHeader(GraphQLMessageHeaders.Authorization, $"Bearer {auth.Token}");
                }

                break;

            case "BASIC":
                if (!string.IsNullOrEmpty(auth.Username) && !string.IsNullOrEmpty(auth.Password))
                {
                    var credentials =
                        Convert.ToBase64String(Encoding.UTF8.GetBytes($"{auth.Username}:{auth.Password}"));
                    message.SetHeader(GraphQLMessageHeaders.Authorization, $"Basic {credentials}");
                }

                break;
        }

        // Apply custom authentication headers
        foreach (var header in auth.CustomHeaders)
        {
            message.SetHeader(header.Key, header.Value);
        }
    }

    /// <summary>
    ///     Applies default headers from configuration to the request.
    /// </summary>
    /// <param name="request">The GraphQL request.</param>
    /// <param name="endpointConfiguration">The endpoint configuration.</param>
    private static void ApplyDefaultHeaders(GraphQLRequest request, GraphQLEndpointConfiguration endpointConfiguration)
    {
        foreach (var header in endpointConfiguration.DefaultHeaders)
        {
            request.Extensions ??= new Dictionary<string, object?>();
            if (!request.Extensions.ContainsKey($"header_{header.Key}"))
            {
                request.Extensions[$"header_{header.Key}"] = header.Value;
            }
        }
    }

    /// <summary>
    ///     Applies message-specific headers to the request.
    /// </summary>
    /// <param name="request">The GraphQL request.</param>
    /// <param name="message">The GraphQL message.</param>
    private static void ApplyMessageHeaders(GraphQLRequest request, GraphQLMessage message)
    {
        foreach (var header in message.GetHeaders().Where(header =>
                     !header.Key.StartsWith(GraphQLMessageHeaders.GraphQLPrefix) &&
                     header.Key != GraphQLMessageHeaders.Authorization &&
                     header.Key != GraphQLMessageHeaders.ContentType))
        {
            request.Extensions ??= new Dictionary<string, object?>();
            request.Extensions[$"header_{header.Key}"] = header.Value;
        }
    }

    /// <summary>
    ///     Applies cookies to the request if cookie handling is enabled.
    /// </summary>
    /// <param name="request">The GraphQL request.</param>
    /// <param name="message">The GraphQL message.</param>
    /// <param name="endpointConfiguration">The endpoint configuration.</param>
    private static void ApplyCookies(GraphQLRequest request, GraphQLMessage message,
        GraphQLEndpointConfiguration endpointConfiguration)
    {
        if (!endpointConfiguration.HandleCookies || message.Cookies == null || message.Cookies.Count == 0)
        {
            return;
        }

        var cookieHeader = string.Join("; ", message.Cookies.Select(c => $"{c.Key}={c.Value}"));
        request.Extensions ??= new Dictionary<string, object?>();
        request.Extensions["header_Cookie"] = cookieHeader;
    }

    /// <summary>
    ///     Copies relevant HTTP headers to the GraphQL message.
    /// </summary>
    /// <param name="httpResponse">The HTTP response.</param>
    /// <param name="message">The GraphQL message.</param>
    private static void CopyHttpHeaders(HttpResponseMessage httpResponse, GraphQLMessage message)
    {
        // Copy content headers
        foreach (var header in httpResponse.Content.Headers)
        {
            message.SetHeader($"Content-{header.Key}", string.Join(", ", header.Value));
        }

        // Copy response headers
        foreach (var header in httpResponse.Headers)
        {
            // Skip headers that are handled specially
            if (header.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase))
            {
                // Handle cookies specially
                HandleSetCookieHeaders(header.Value, message);
                continue;
            }

            message.SetHeader(header.Key, string.Join(", ", header.Value));
        }
    }

    /// <summary>
    ///     Handles Set-Cookie headers from HTTP response.
    /// </summary>
    /// <param name="cookieHeaders">The cookie header values.</param>
    /// <param name="message">The GraphQL message.</param>
    private static void HandleSetCookieHeaders(IEnumerable<string> cookieHeaders, GraphQLMessage message)
    {
        foreach (var cookieHeader in cookieHeaders)
        {
            // Parse cookie (simplified - just get name=value part)
            var parts = cookieHeader.Split(';')[0].Split('=', 2);
            if (parts.Length == 2)
            {
                message.AddCookie(parts[0].Trim(), parts[1].Trim());
            }
        }
    }
}
