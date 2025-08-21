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
public class LoggingGraphQlClientHandler(HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
{
    private static readonly string Newline = Environment.NewLine;
    private static readonly ILogger Log = LogManager.GetLogger(typeof(LoggingGraphQlClientHandler));
    private readonly TestContextFactory _contextFactory = TestContextFactory.NewInstance();
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };
    private AsyncMessageListeners? _messageListener;

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
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Content != null)
        {
            var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            await HandleGraphQlRequest(await GetGraphQlRequestContent(request, requestBody));
        }
        else
        {
            await HandleGraphQlRequest(await GetGraphQlRequestContent(request, string.Empty));
        }

        // Execute GraphQL request
        var response = await base.SendAsync(request, cancellationToken);

        await HandleGraphQlResponse(await GetGraphQlResponseContent(response));

        return response;
    }

    /// <summary>
    ///     Processes the GraphQL request by logging the message and notifying message listeners if any are registered.
    ///     Provides enhanced formatting for GraphQL operations, including query parsing and variable display.
    /// </summary>
    /// <param name="request">The GraphQL request content as a formatted string.</param>
    public async Task HandleGraphQlRequest(string request)
    {
        if (HasMessageListeners())
        {
            Log.LogDebug("Sending GraphQL request message");
            await _messageListener!.OnOutboundMessage(new RawMessage(request), _contextFactory.GetObject());
        }
        else
        {
            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug("Sending GraphQL request message: {Request}", Newline + request);
            }
        }
    }

    /// <summary>
    ///     Handles the GraphQL response by logging the message and notifying message listeners if any are registered.
    ///     Provides enhanced formatting for GraphQL responses including data, errors, and extensions.
    /// </summary>
    /// <param name="response">The GraphQL response content as a formatted string.</param>
    public async Task HandleGraphQlResponse(string response)
    {
        if (HasMessageListeners())
        {
            Log.LogDebug("Received GraphQL response message");
            await _messageListener!.OnInboundMessage(new RawMessage(response), _contextFactory.GetObject());
        }
        else
        {
            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug("Received GraphQL response message: {Response}", Newline + response);
            }
        }
    }

    /// <summary>
    ///     Retrieves the GraphQL response content as a formatted string, including status line, headers, and GraphQL-specific
    ///     body formatting.
    /// </summary>
    /// <param name="response">The HTTP response message containing the GraphQL response to be processed.</param>
    /// <returns>A formatted string containing the GraphQL response content with enhanced readability.</returns>
    public async Task<string> GetGraphQlResponseContent(HttpResponseMessage? response)
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

                var responseBody = await response.Content.ReadAsStringAsync();
                builder.Append(await FormatGraphQlResponseBody(responseBody));
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
    private async Task<string> GetGraphQlRequestContent(HttpRequestMessage request, string body)
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
        builder.Append(await FormatGraphQlRequestBody(body));

        return builder.ToString();
    }

    private static async Task<Stream> StringToStreamAsync(string text, Encoding? encoding = null,
        CancellationToken ct = default)
    {
        encoding ??= Encoding.UTF8;
        var ms = new MemoryStream();
        await using (var writer = new StreamWriter(ms, encoding, 1024, true))
        {
            await writer.WriteAsync(text.AsMemory(), ct);
            await writer.FlushAsync(ct);
        }

        ms.Position = 0;
        return ms;
    }


    /// <summary>
    ///     Formats the GraphQL request body with enhanced readability for queries, mutations, and subscriptions.
    /// </summary>
    /// <param name="body">The raw GraphQL request body JSON.</param>
    /// <returns>A formatted string representation of the GraphQL request body.</returns>
    public async Task<string> FormatGraphQlRequestBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return await Task.FromResult(body);
        }

        try
        {
            var jsonDocument = await JsonDocument.ParseAsync(await StringToStreamAsync(body));
            var builder = new StringBuilder();

            builder.AppendLine("=== GraphQL Request ===");

            // Extract and format query/mutation/subscription
            if (jsonDocument.RootElement.TryGetProperty("query", out var queryElement))
            {
                var query = queryElement.GetString();
                var operationType = DetectOperationType(query);

                builder.AppendLine($"Operation Type: {operationType}");
                builder.AppendLine("Query:");
                builder.AppendLine(FormatGraphQlQuery(query));
            }

            // Extract and format variables
            if (jsonDocument.RootElement.TryGetProperty("variables", out var variablesElement))
            {
                builder.AppendLine("Variables:");
                builder.AppendLine(JsonSerializer.Serialize(variablesElement, _jsonSerializerOptions));
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
    public async Task<string> FormatGraphQlResponseBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return body;
        }

        try
        {
            var jsonDocument = await JsonDocument.ParseAsync(await StringToStreamAsync(body));
            var builder = new StringBuilder();

            builder.AppendLine("=== GraphQL Response ===");

            // Format data section
            if (jsonDocument.RootElement.TryGetProperty("data", out var dataElement))
            {
                builder.AppendLine("Data:");
                builder.AppendLine(JsonSerializer.Serialize(dataElement, _jsonSerializerOptions));
            }

            // Format errors section
            if (jsonDocument.RootElement.TryGetProperty("errors", out var errorsElement))
            {
                builder.AppendLine("Errors:");
                builder.AppendLine(JsonSerializer.Serialize(errorsElement, _jsonSerializerOptions));
            }

            // Format extensions section
            if (jsonDocument.RootElement.TryGetProperty("extensions", out var extensionsElement))
            {
                builder.AppendLine("Extensions:");
                builder.AppendLine(JsonSerializer.Serialize(extensionsElement, _jsonSerializerOptions));
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
    private static string DetectOperationType(string? query)
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

        return trimmedQuery.StartsWith("subscription") ? "Subscription" : "Unknown";
    }

    /// <summary>
    ///     Formats a GraphQL query string with basic indentation for better readability.
    /// </summary>
    /// <param name="query">The raw GraphQL query string.</param>
    /// <returns>A formatted GraphQL query string.</returns>
    private static string FormatGraphQlQuery(string? query)
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
    private static void AppendHeadersCommon(HttpHeaders headers, StringBuilder builder)
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
    public void SetMessageListener(AsyncMessageListeners messageListener)
    {
        _messageListener = messageListener;
    }
}
