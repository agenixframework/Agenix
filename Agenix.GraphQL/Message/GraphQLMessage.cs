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
using System.Text.Json;
using Agenix.Api.Endpoint.Resolver;
using Agenix.Api.Message;
using Agenix.Core.Message;

namespace Agenix.GraphQL.Message;

/// <summary>
///     Represents a GraphQL message containing query, variables, and operation metadata.
///     This class extends the base message functionality to provide GraphQL-specific features.
/// </summary>
public class GraphQLMessage : DefaultMessage
{
    /// <summary>
    ///     Initializes a new instance of the GraphQLMessage class.
    /// </summary>
    public GraphQLMessage()
    {
    }

    /// <summary>
    /// Represents a GraphQL message containing query, variables, cookies, headers,
    /// and other relevant information for constructing and handling GraphQL requests.
    /// </summary>
    public GraphQLMessage(IMessage message) : this(message, false)
    {
    }

    /// Sets a new header name-value pair for the HTTP message.
    /// <param name="headerName">The name of the header.</param>
    /// <param name="headerValue">The value of the header.</param>
    /// <return>The altered HttpMessage instance.</return>
    public GraphQLMessage Header(string headerName, object headerValue)
    {
        return (GraphQLMessage)base.SetHeader(headerName, headerValue);
    }

    /// <summary>
    /// Represents a message specific to GraphQL, extending the functionality of DefaultMessage.
    /// Provides methods and properties tailored for GraphQL operations, such as defining queries,
    /// variables, operation names, headers, cookies, and other GraphQL-specific configuration.
    /// </summary>
    public GraphQLMessage(IMessage message, bool forceAgenixHeaderUpdate) : base(message, forceAgenixHeaderUpdate)
    {
    }

    /// Represents a HTTP message with methods to manage HTTP-specific
    /// details such as method, URI, headers, status codes, and payload.
    /// Inherits from DefaultMessage and offers extensions to manipulate
    /// HTTP message components effectively.
    public GraphQLMessage(object payload) : base(payload)
    {
    }

    /// Represents an HTTP message, providing methods to define and manipulate
    /// HTTP-specific properties such as method, URI, headers, status code, and more.
    /// Extends the DefaultMessage class to offer additional operations for managing
    /// HTTP message components, enabling customization and construction of messages
    /// for HTTP communication.
    public GraphQLMessage(object payload, Dictionary<string, object> headers) : base(payload, headers)
    {
    }

    /// <summary>
    /// Configures whether to use WebSocket for the GraphQL message.
    /// </summary>
    /// <param name="useWebSocket">A boolean indicating whether to enable WebSocket. Defaults to true.</param>
    /// <returns>The current instance of the <see cref="GraphQLMessage"/> with the updated configuration.</returns>
    public GraphQLMessage UseWebSocket(bool useWebSocket = true)
    {
        SetHeader(GraphQLMessageHeaders.UseWebSocket, useWebSocket.ToString());
        return this;
    }

    /// <summary>
    ///     Gets or sets the cookies for the GraphQL request.
    /// </summary>
    public Dictionary<string, string>? Cookies { get; set; }

    /// <summary>
    ///     Gets or sets the variables to be used with the GraphQL operation.
    /// </summary>
    public object? Variables { get; set; }

    /// <summary>
    /// Sets the type of the GraphQL operation for the message and updates the corresponding header.
    /// </summary>
    /// <param name="method">The <see cref="GraphQLOperationType"/> representing the type of GraphQL operation,
    /// such as QUERY, MUTATION, or SUBSCRIPTION.</param>
    /// <returns>
    /// The current instance of the <see cref="GraphQLMessage"/> for method chaining.
    /// </returns>
    public GraphQLMessage SetOperationType(GraphQLOperationType method)
    {
        SetHeader(GraphQLMessageHeaders.OperationType, method.ToString());
        return this;
    }

    /// <summary>
    /// Sets the HTTP response status code header.
    /// </summary>
    /// <param name="statusCode">The status code header values it to respond with.</param>
    /// <returns>The modified <see cref="GraphQLMessage"/> instance.</returns>
    public virtual GraphQLMessage Status(HttpStatusCode statusCode)
    {
        SetHeader(GraphQLMessageHeaders.StatusCode, (int)statusCode);
        var status = (HttpStatusCode)(int)statusCode;
        if (Enum.IsDefined(typeof(HttpStatusCode), (int)statusCode))
        {
            SetHeader(GraphQLMessageHeaders.ReasonPhrase, status.ToString());
        }

        return this;
    }

    /// <summary>
    /// Sets the operation name for the GraphQL message.
    /// </summary>
    /// <param name="operation">
    /// The name of the operation to be executed in the GraphQL query.
    /// </param>
    /// <returns>
    /// The updated <see cref="GraphQLMessage"/> instance with the specified operation name set.
    /// </returns>
    public GraphQLMessage SetOperationName(string operation)
    {
        SetHeader(GraphQLMessageHeaders.OperationName, operation);
        return this;
    }

    /// Sets the HTTP request URI header values. This method is used to configure
    /// the URI associated with the HTTP request by setting the appropriate headers.
    /// <param name="requestUri">The URI string to be set as the request URI header.</param>
    /// <returns>The modified instance of HttpMessage with the updated URI headers.</returns>
    public GraphQLMessage Uri(string requestUri)
    {
        SetHeader(IEndpointUriResolver.EndpointUriHeaderName, requestUri);
        SetHeader(GraphQLMessageHeaders.EndpointUrl, requestUri);
        return this;
    }

    /// <summary>
    /// Retrieves the name of the GraphQL operation from the headers.
    /// </summary>
    /// <returns>
    /// A string representing the operation name if it exists in the headers; otherwise, an empty string.
    /// </returns>
    public string GetOperationName()
    {
        var operation = GetHeader(GraphQLMessageHeaders.OperationName);

        return operation?.ToString()!;
    }


    /// <summary>
    /// Retrieves the operation type of the current GraphQL message.
    /// </summary>
    /// <returns>
    /// The operation type of the message as a <see cref="GraphQLOperationType"/>.
    /// Returns <see cref="GraphQLOperationType.QUERY"/> if the operation type is not explicitly set in the message headers.
    /// </returns>
    public GraphQLOperationType GetOperationType()
    {
        var operation = GetHeader(GraphQLMessageHeaders.OperationType);

        return operation == null
            ? GraphQLOperationType.QUERY
            : Enum.Parse<GraphQLOperationType>(operation.ToString() ?? "Query", ignoreCase: true);
    }


    /// Sets the HTTP request content type header.
    /// <param name="contentType">The content type header value to use</param>
    /// <return>The altered HttpMessage</return>
    public GraphQLMessage ContentType(string contentType)
    {
        SetHeader("Content-Type", contentType);
        return this;
    }

    /// Sets the HTTP accepted content type header for the response.
    /// <param name="accept">The accept header value to set.</param>
    /// <return>The altered HttpMessage.</return>
    public GraphQLMessage Accept(string accept)
    {
        SetHeader("Accept", accept);
        return this;
    }

    /// <summary>
    ///     Gets or sets additional HTTP headers for the GraphQL request.
    /// </summary>
    public Dictionary<string, object> Headers { get; set; } = new();

    /// <summary>
    ///     Gets or sets the timeout for the GraphQL operation.
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    ///     Gets or sets extensions for the GraphQL request.
    ///     Extensions are additional data that can be sent alongside the GraphQL operation.
    /// </summary>
    public Dictionary<string, object?> Extensions { get; set; } = new();

    /// <summary>
    ///     Gets or sets whether to use the GET method for queries instead of POST.
    ///     By default, GraphQL operations use POST method.
    /// </summary>
    public bool UseGetForQueries { get; set; }

    /// <summary>
    ///     Gets or sets the subscription options for GraphQL subscriptions.
    /// </summary>
    public GraphQLSubscriptionOptions? SubscriptionOptions { get; set; }

    /// <summary>
    ///     Adds a cookie to the GraphQL request.
    /// </summary>
    /// <param name="name">The cookie name.</param>
    /// <param name="value">The cookie value.</param>
    public void AddCookie(string name, string value)
    {
        Cookies ??= new Dictionary<string, string>();
        Cookies[name] = value;
    }

    /// <summary>
    ///     Gets the value of a specific cookie.
    /// </summary>
    /// <param name="name">The cookie name.</param>
    /// <returns>The cookie value if found, null otherwise.</returns>
    public string? GetCookie(string name)
    {
        return Cookies?.GetValueOrDefault(name);
    }

    /// <summary>
    ///     Removes a cookie from the GraphQL request.
    /// </summary>
    /// <param name="name">The cookie name to remove.</param>
    /// <returns>True if the cookie was removed, false if it didn't exist.</returns>
    public bool RemoveCookie(string name)
    {
        return Cookies?.Remove(name) ?? false;
    }

    /// <summary>
    ///     Converts the GraphQL message to a JSON string representation.
    /// </summary>
    /// <returns>A JSON string containing the GraphQL operation.</returns>
    public string ToJson()
    {
        var request = new Dictionary<string, object> { ["query"] = Payload?.ToString() ?? string.Empty };

        if (Variables != null)
        {
            request["variables"] = Variables;
        }

        var operationName = GetOperationName();
        if (!string.IsNullOrEmpty(operationName))
        {
            request["operationName"] = operationName;
        }

        if (Extensions is { Count: > 0 })
        {
            request["extensions"] = Extensions;
        }

        return JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    ///     Creates a GraphQL message from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string containing the GraphQL operation.</param>
    /// <returns>A GraphQLMessage instance parsed from the JSON.</returns>
    public static GraphQLMessage FromJson(string json)
    {
        var jsonDocument = JsonDocument.Parse(json);
        var root = jsonDocument.RootElement;

        var query = root.TryGetProperty("query", out var queryElement)
            ? queryElement.GetString() ?? string.Empty
            : string.Empty;
        var message = new GraphQLMessage(query);

        if (root.TryGetProperty("variables", out var variablesElement))
        {
            message.Variables = JsonSerializer.Deserialize<Dictionary<string, object>>(variablesElement.GetRawText());
        }

        if (root.TryGetProperty("operationName", out var operationNameElement))
        {
            message.SetOperationName(operationNameElement.GetString());
        }

        if (root.TryGetProperty("extensions", out var extensionsElement))
        {
            message.Extensions = JsonSerializer.Deserialize<Dictionary<string, object>>(extensionsElement.GetRawText());
        }

        return message;
    }

    /// <summary>
    ///     Detects the operation type from the GraphQL query string.
    /// </summary>
    /// <param name="query">The GraphQL query string.</param>
    /// <returns>The detected GraphQL operation type.</returns>
    private static GraphQLOperationType DetectOperationType(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return GraphQLOperationType.QUERY;
        }

        var trimmedQuery = query.Trim().ToLowerInvariant();

        if (trimmedQuery.StartsWith("mutation"))
        {
            return GraphQLOperationType.MUTATION;
        }

        return trimmedQuery.StartsWith("subscription")
            ? GraphQLOperationType.SUBSCRIPTION
            :
            // Default to Query (queries can omit the "query" keyword)
            GraphQLOperationType.QUERY;
    }

    /// <summary>
    ///     Adds or updates a header value for the GraphQL request.
    /// </summary>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value.</param>
    public void AddHeader(string name, object value)
    {
        Headers[name] = value;
        SetHeader(name, value);
    }

    /// <summary>
    ///     Adds or updates multiple headers for the GraphQL request.
    /// </summary>
    /// <param name="headers">The headers to add.</param>
    public void AddHeaders(Dictionary<string, object> headers)
    {
        foreach (var header in headers)
        {
            AddHeader(header.Key, header.Value);
        }
    }

    /// <summary>
    ///     Sets a variable value for the GraphQL operation.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <param name="value">The variable value.</param>
    public GraphQLMessage SetVariable(string name, object? value)
    {
        Variables ??= new Dictionary<string, object?>();

        if (Variables is Dictionary<string, object?> variablesDict)
        {
            variablesDict[name] = value;
        }
        else
        {
            // Convert to dictionary if it's not already
            var newVariables = new Dictionary<string, object?> { [name] = value };
            Variables = newVariables;
        }

        return this;
    }

    /// <summary>
    /// Sets an extension in the GraphQLMessage by adding or updating the key-value pair in the extensions dictionary.
    /// </summary>
    /// <param name="name">
    /// The name of the extension to be added or updated.
    /// </param>
    /// <param name="value">
    /// The value of the extension associated with the specified name.
    /// </param>
    /// <returns>
    /// The current instance of the GraphQLMessage with the updated extensions.
    /// </returns>
    public GraphQLMessage SetExtension(string name, object? value)
    {
        if (Extensions is { } variablesDict)
        {
            variablesDict[name] = value;
        }
        else
        {
            // Convert to dictionary if it's not already
            var newVariables = new Dictionary<string, object?> { [name] = value };
            Extensions = newVariables;
        }

        return this;
    }

    /// <summary>
    ///     Gets a variable value from the GraphQL operation.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <returns>The variable value, or null if not found.</returns>
    public object? GetVariable(string name)
    {
        if (Variables is Dictionary<string, object?> variablesDict)
        {
            return variablesDict.GetValueOrDefault(name);
        }

        return null;
    }

    /// <summary>
    /// Retrieves the value of a specified extension by its name.
    /// </summary>
    /// <param name="name">
    /// The name of the extension to retrieve.
    /// </param>
    /// <returns>
    /// The value of the extension if it exists, or null if the specified extension is not found.
    /// </returns>
    public object? GetExtension(string name)
    {
        if (Extensions is { } variablesDict)
        {
            return variablesDict.GetValueOrDefault(name);
        }

        return null;
    }
}
