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
using Agenix.Api.Log;
using Agenix.Api.Report;
using Agenix.Core;
using Agenix.Core.Message;
using Microsoft.Extensions.Logging;

namespace Agenix.GraphQL.Interceptor;

/// <summary>
///     The LoggingGraphQLClientHandler class is a custom HTTP message handler that logs GraphQL request and response
///     messages.
///     It extends the DelegatingHandler, enabling modification or inspection of GraphQL requests and responses during HTTP
///     communication with enhanced GraphQL-specific formatting.
/// </summary>
public class LoggingGraphQLClientHandler(HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
{
    private static readonly string Newline = Environment.NewLine;
    private static readonly ILogger Log = LogManager.GetLogger(typeof(LoggingGraphQLClientHandler));
    private readonly TestContextFactory _contextFactory = TestContextFactory.NewInstance();
    private MessageListeners _messageListener = null!;

    /// <summary>
    ///     Sends an HTTP request asynchronously and processes both the GraphQL request and response by logging their contents
    ///     and notifying message listeners if any are registered.
    /// </summary>
    /// <param name="request">The HTTP request message containing the GraphQL operation to be sent.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation if needed.</param>
    /// <returns>
    ///     A task representing the asynchronous operation, with an HttpResponseMessage result containing the GraphQL response
    ///     from the server.
    /// </returns>
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content != null)
        {
            var requestBody = request.Content.ReadAsStringAsync(cancellationToken).Result;
            HandleGraphQLRequest(GetGraphQLRequestContent(request, requestBody));
        }
        else
        {
            HandleGraphQLRequest(GetGraphQLRequestContent(request, string.Empty));
        }

        // Execute GraphQL request
        var response = base.Send(request, cancellationToken);

        HandleGraphQLResponse(GetGraphQLResponseContent(response));

        return response;
    }

    /// <summary>
    ///     Processes the GraphQL request by logging the message and notifying message listeners if any are registered.
    ///     Provides enhanced formatting for GraphQL operations including query parsing and variable display.
    /// </summary>
    /// <param name="request">The GraphQL request content as a formatted string.</param>
    public void HandleGraphQLRequest(string request)
    {
        if (HasMessageListeners())
        {
            Log.LogDebug("Sending GraphQL request message");
            _messageListener.OnOutboundMessage(new RawMessage(request), _contextFactory.GetObject());
        }
        else
        {
            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug("Sending GraphQL request message: {}", Newline + request);
            }
        }
    }

    /// <summary>
    ///     Handles the GraphQL response by logging the message and notifying message listeners if any are registered.
    ///     Provides enhanced formatting for GraphQL responses including data, errors, and extensions.
    /// </summary>
    /// <param name="response">The GraphQL response content as a formatted string.</param>
    public void HandleGraphQLResponse(string response)
    {
        if (HasMessageListeners())
        {
            Log.LogDebug("Received GraphQL response message");
            _messageListener.OnInboundMessage(new RawMessage(response), _contextFactory.GetObject());
        }
        else
        {
            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug("Received GraphQL response message: {}", Newline + response);
            }
        }
    }

    /// <summary>
    ///     Retrieves the GraphQL response content as a formatted string, including status line, headers, and GraphQL-specific
    ///     body formatting.
    /// </summary>
    /// <param name="response">The HTTP response message containing the GraphQL response to be processed.</param>
    /// <returns>A formatted string containing the GraphQL response content with enhanced readability.</returns>
    private string GetGraphQLResponseContent(HttpResponseMessage? response)
    {
        if (response != null)
        {
            var builder = new StringBuilder();

            // Status line
            builder.Append("HTTP/");
            builder.Append(response.Version);
            builder.Append(' ');
            builder.Append((int)response.StatusCode);
            builder.Append(' ');
            builder.Append(response.ReasonPhrase);
            builder.Append(Newline);

            // Headers
            AppendHeadersCommon(response.Headers, builder);

            if (response.Content != null)
            {
                AppendHeadersCommon(response.Content.Headers, builder);
                builder.Append(Newline);

                var responseBody = response.Content.ReadAsStringAsync().Result;
                builder.Append(FormatGraphQLResponseBody(responseBody));
            }

            return builder.ToString();
        }

        return string.Empty;
    }

    /// <summary>
    ///     Formats the GraphQL request content with enhanced readability including operation type detection and variable
    ///     formatting.
    /// </summary>
    /// <param name="request">The HTTP request message containing the GraphQL operation.</param>
    /// <param name="body">The request body containing the GraphQL query and variables.</param>
    /// <returns>A formatted string representation of the GraphQL request.</returns>
    private string GetGraphQLRequestContent(HttpRequestMessage request, string body)
    {
        var builder = new StringBuilder();

        // Request line
        builder.Append(request.Method);
        builder.Append(' ');
        builder.Append(request.RequestUri);
        builder.Append(" HTTP/");
        builder.Append(request.Version);
        builder.Append(Newline);

        // Headers
        AppendHeadersCommon(request.Headers, builder);
        if (request.Content != null)
        {
            AppendHeadersCommon(request.Content.Headers, builder);
        }

        builder.Append(Newline);

        // GraphQL-specific body formatting
        builder.Append(FormatGraphQLRequestBody(body));

        return builder.ToString();
    }

    /// <summary>
    ///     Formats the GraphQL request body with enhanced readability for queries, mutations, and subscriptions.
    /// </summary>
    /// <param name="body">The raw GraphQL request body JSON.</param>
    /// <returns>A formatted string representation of the GraphQL request body.</returns>
    private string FormatGraphQLRequestBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return body;
        }

        try
        {
            var jsonDocument = JsonDocument.Parse(body);
            var builder = new StringBuilder();

            builder.AppendLine("=== GraphQL Request ===");

            // Extract and format query/mutation/subscription
            if (jsonDocument.RootElement.TryGetProperty("query", out var queryElement))
            {
                var query = queryElement.GetString();
                var operationType = DetectOperationType(query);

                builder.AppendLine($"Operation Type: {operationType}");
                builder.AppendLine("Query:");
                builder.AppendLine(FormatGraphQLQuery(query));
            }

            // Extract and format variables
            if (jsonDocument.RootElement.TryGetProperty("variables", out var variablesElement))
            {
                builder.AppendLine("Variables:");
                builder.AppendLine(JsonSerializer.Serialize(variablesElement,
                    new JsonSerializerOptions { WriteIndented = true }));
            }

            // Extract operation name if present
            if (jsonDocument.RootElement.TryGetProperty("operationName", out var operationNameElement))
            {
                var operationName = operationNameElement.GetString();
                if (!string.IsNullOrEmpty(operationName))
                {
                    builder.AppendLine($"Operation Name: {operationName}");
                }
            }

            return builder.ToString();
        }
        catch (JsonException)
        {
            // If JSON parsing fails, return the original body
            return body;
        }
    }

    /// <summary>
    ///     Formats the GraphQL response body with enhanced readability for data, errors, and extensions.
    /// </summary>
    /// <param name="body">The raw GraphQL response body JSON.</param>
    /// <returns>A formatted string representation of the GraphQL response body.</returns>
    private string FormatGraphQLResponseBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return body;
        }

        try
        {
            var jsonDocument = JsonDocument.Parse(body);
            var builder = new StringBuilder();

            builder.AppendLine("=== GraphQL Response ===");

            // Format data section
            if (jsonDocument.RootElement.TryGetProperty("data", out var dataElement))
            {
                builder.AppendLine("Data:");
                builder.AppendLine(JsonSerializer.Serialize(dataElement,
                    new JsonSerializerOptions { WriteIndented = true }));
            }

            // Format errors section
            if (jsonDocument.RootElement.TryGetProperty("errors", out var errorsElement))
            {
                builder.AppendLine("Errors:");
                builder.AppendLine(JsonSerializer.Serialize(errorsElement,
                    new JsonSerializerOptions { WriteIndented = true }));
            }

            // Format extensions section
            if (jsonDocument.RootElement.TryGetProperty("extensions", out var extensionsElement))
            {
                builder.AppendLine("Extensions:");
                builder.AppendLine(JsonSerializer.Serialize(extensionsElement,
                    new JsonSerializerOptions { WriteIndented = true }));
            }

            return builder.ToString();
        }
        catch (JsonException)
        {
            // If JSON parsing fails, return the original body
            return body;
        }
    }

    /// <summary>
    ///     Detects the GraphQL operation type from the query string.
    /// </summary>
    /// <param name="query">The GraphQL query string.</param>
    /// <returns>The detected operation type (Query, Mutation, Subscription, or Unknown).</returns>
    private string DetectOperationType(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return "Unknown";
        }

        var trimmedQuery = query.Trim().ToLowerInvariant();

        if (trimmedQuery.StartsWith("query") ||
            (!trimmedQuery.StartsWith("mutation") && !trimmedQuery.StartsWith("subscription")))
        {
            return "Query";
        }

        if (trimmedQuery.StartsWith("mutation"))
        {
            return "Mutation";
        }

        if (trimmedQuery.StartsWith("subscription"))
        {
            return "Subscription";
        }

        return "Unknown";
    }

    /// <summary>
    ///     Formats a GraphQL query string with basic indentation for better readability.
    /// </summary>
    /// <param name="query">The raw GraphQL query string.</param>
    /// <returns>A formatted GraphQL query string.</returns>
    private string FormatGraphQLQuery(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return string.Empty;
        }

        // Basic formatting - add newlines and indentation
        var formatted = query
            .Replace("{", "{\n  ")
            .Replace("}", "\n}")
            .Replace(",", ",\n  ")
            .Replace("\n  \n", "\n");

        return formatted;
    }

    /// <summary>
    ///     Appends HTTP headers to the specified StringBuilder in a formatted manner.
    /// </summary>
    /// <param name="headers">The collection of HTTP headers to be appended.</param>
    /// <param name="builder">The StringBuilder to which headers are appended.</param>
    private void AppendHeadersCommon(HttpHeaders headers, StringBuilder builder)
    {
        foreach (var header in headers)
        {
            builder.Append(header.Key);
            builder.Append(": ");
            builder.Append(string.Join(", ", header.Value));
            builder.Append(Newline);
        }
    }

    /// <summary>
    ///     Determines whether there are any message listeners registered.
    /// </summary>
    /// <returns>Returns true if there are message listeners registered; otherwise, false.</returns>
    public bool HasMessageListeners()
    {
        return _messageListener != null && !_messageListener.IsEmpty();
    }

    /// <summary>
    ///     Sets the message listener for handling GraphQL request and response messages.
    /// </summary>
    /// <param name="messageListener">The message listener to be set.</param>
    public void SetMessageListener(MessageListeners messageListener)
    {
        _messageListener = messageListener;
    }
}
