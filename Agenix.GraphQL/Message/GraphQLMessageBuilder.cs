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
using Agenix.Api.Context;
using Agenix.Api.Message;
using Agenix.Core.Message.Builder;

namespace Agenix.GraphQL.Message;

/// <summary>
///     Builder class for constructing GraphQL messages using a fluent API.
/// </summary>
public class GraphQLMessageBuilder : StaticMessageBuilder
{
    /// <summary>
    ///     The GraphQL message being built.
    /// </summary>
    private readonly GraphQLMessage _message;

    /// <summary>
    ///     Initializes a new instance of the GraphQLMessageBuilder class.
    /// </summary>
    public GraphQLMessageBuilder() : this(new GraphQLMessage())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the GraphQLMessageBuilder class with an existing message.
    /// </summary>
    /// <param name="message">The existing GraphQL message to build upon.</param>
    public GraphQLMessageBuilder(GraphQLMessage? message) : base(message)
    {
        _message = message ?? new GraphQLMessage();
    }

    /// <summary>
    ///     Adds a cookie to the GraphQL request.
    /// </summary>
    /// <param name="name">The cookie name.</param>
    /// <param name="value">The cookie value.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GraphQLMessageBuilder Cookie(string name, string value)
    {
        _message.AddCookie(name, value);
        return this;
    }

    /// <summary>
    ///     Sets multiple cookies for the GraphQL request.
    /// </summary>
    /// <param name="cookies">The cookies dictionary.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GraphQLMessageBuilder Cookies(Dictionary<string, string> cookies)
    {
        _message.Cookies = cookies;
        return this;
    }

    /// <summary>
    ///     Adds a session cookie for authentication.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GraphQLMessageBuilder SessionCookie(string sessionId)
    {
        return Cookie("sessionId", sessionId);
    }


    /// <summary>
    ///     Sets the GraphQL query string.
    /// </summary>
    /// <param name="query">The GraphQL query string.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GraphQLMessageBuilder Query(string query)
    {
        _message.Payload = query;
        return this;
    }

    /// <summary>
    ///     Sets the GraphQL operation name.
    /// </summary>
    /// <param name="operationName">The name of the GraphQL operation.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GraphQLMessageBuilder OperationName(string operationName)
    {
        _message.SetOperationName(operationName);
        return this;
    }

    /// <summary>
    ///     Sets the GraphQL operation type.
    /// </summary>
    /// <param name="operationType">The type of GraphQL operation.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GraphQLMessageBuilder OperationType(GraphQLOperationType operationType)
    {
        _message.SetOperationType(operationType);
        return this;
    }

    /// <summary>
    ///     Sets the variables for the GraphQL operation.
    /// </summary>
    /// <param name="variables">The variables dictionary.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GraphQLMessageBuilder Variables(Dictionary<string, object?> variables)
    {
        _message.Variables = variables;
        return this;
    }


    /// <summary>
    ///     Adds a single variable to the GraphQL operation.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <param name="value">The variable value.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GraphQLMessageBuilder Variable(string name, object? value)
    {
        _message.SetVariable(name, value);

        return this;
    }

    /// <summary>
    ///     Sets the extensions for the GraphQL operation.
    /// </summary>
    /// <param name="extensions">The extensions dictionary.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GraphQLMessageBuilder Extensions(Dictionary<string, object>? extensions)
    {
        _message.Extensions = extensions;
        return this;
    }

    /// <summary>
    ///     Adds a single extension to the GraphQL operation.
    /// </summary>
    /// <param name="name">The extension name.</param>
    /// <param name="value">The extension value.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GraphQLMessageBuilder Extension(string name, object value)
    {
        _message.SetExtension(name, value);
        return this;
    }

    /// <summary>
    ///     Sets the subscription options for GraphQL subscriptions.
    /// </summary>
    /// <param name="subscriptionOptions">The subscription options.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GraphQLMessageBuilder SubscriptionOptions(GraphQLSubscriptionOptions subscriptionOptions)
    {
        _message.SubscriptionOptions = subscriptionOptions;
        return this;
    }

    /// <summary>
    ///     Sets the endpoint URL for the GraphQL request.
    /// </summary>
    /// <param name="endpointUrl">The GraphQL endpoint URL.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GraphQLMessageBuilder EndpointUrl(string endpointUrl)
    {
        _message.SetHeader(GraphQLMessageHeaders.EndpointUrl, endpointUrl);
        return this;
    }

    /// <summary>
    ///     Adds an authorization header.
    /// </summary>
    /// <param name="token">The authorization token.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GraphQLMessageBuilder Authorization(string token)
    {
        _message.SetHeader(GraphQLMessageHeaders.Authorization, token);
        return this;
    }

    /// <summary>
    ///     Adds a bearer token authorization header.
    /// </summary>
    /// <param name="token">The bearer token.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GraphQLMessageBuilder BearerToken(string token)
    {
        _message.SetHeader(GraphQLMessageHeaders.Authorization, $"Bearer {token}");
        return this;
    }

    /// <summary>
    ///     Sets whether to use GET method for queries.
    /// </summary>
    /// <param name="useGet">True to use GET method for queries, false to use POST.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GraphQLMessageBuilder UseGetForQueries(bool useGet = true)
    {
        _message.SetHeader(GraphQLMessageHeaders.UseGetForQueries, useGet.ToString().ToLowerInvariant());
        return this;
    }

    /// <summary>
    ///     Sets the query hash for persisted queries.
    /// </summary>
    /// <param name="queryHash">The query hash.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GraphQLMessageBuilder QueryHash(string queryHash)
    {
        _message.SetHeader(GraphQLMessageHeaders.QueryHash, queryHash);
        return this;
    }

    /// <summary>
    ///     Sets the request timeout.
    /// </summary>
    /// <param name="timeout">The request timeout.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GraphQLMessageBuilder RequestTimeout(TimeSpan timeout)
    {
        _message.SetHeader(GraphQLMessageHeaders.RequestTimeout, timeout.ToString());
        return this;
    }

    /// <summary>
    ///     Sets the client version.
    /// </summary>
    /// <param name="version">The client version.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GraphQLMessageBuilder ClientVersion(string version)
    {
        _message.SetHeader(GraphQLMessageHeaders.ClientVersion, version);
        return this;
    }

    /// <summary>
    ///     Adds a custom header to the GraphQL message.
    /// </summary>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GraphQLMessageBuilder Header(string name, object value)
    {
        _message.SetHeader(name, value);
        return this;
    }

    /// <summary>
    ///     Sets the message name.
    /// </summary>
    /// <param name="name">The message name.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GraphQLMessageBuilder Name(string name)
    {
        _message.Name = name;
        return this;
    }

    /// <summary>
    ///     Sets the message payload directly.
    /// </summary>
    /// <param name="payload">The message payload.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GraphQLMessageBuilder Payload(object payload)
    {
        _message.Payload = payload;
        return this;
    }

    /// <summary>
    /// Constructs and returns a GraphQL message using the specified test execution context and message type.
    /// This method extends the functionality of the base message building process by creating an GraphQL-specific message,
    /// ensuring that additional configurations such as cookies and headers are handled appropriately.
    /// </summary>
    /// <param name="context">
    /// The test execution context used to construct the message and manage its state within a test
    /// scenario.
    /// </param>
    /// <param name="messageType">
    /// The type identifier for the message being constructed, influencing its structure and
    /// behavior.
    /// </param>
    /// <returns>
    /// An IMessage instance representing the newly constructed GraphQL message, complete with headers, cookies, and other
    /// HTTP-specific attributes.
    /// </returns>
    public override IMessage Build(TestContext context, string messageType)
    {
        //Copy the initial message so that it is not manipulated during the test.
        var message = new GraphQLMessage(base.GetMessage(),
            AgenixSettings.IsHttpMessageBuilderForceHeaderUpdateEnabled());

        var constructed = base.Build(context, messageType);

        message.Name = constructed.Name;
        message.SetType(constructed.GetType());
        message.Payload = constructed.Payload;
        ReplaceHeaders(constructed, message);

        return message;
    }

    /// Replaces the headers in the target message with headers from the source message, excluding filtered headers.
    /// <param name="from">The source message from which to retrieve headers.</param>
    /// <param name="to">The target message to which the headers should be set.</param>
    private static void ReplaceHeaders(IMessage from, IMessage to)
    {
        var headerKeys = new HashSet<string>(to.GetHeaders().Keys)
            .Where(key => !FilteredHeaders.Contains(key))
            .ToHashSet();

        foreach (var key in headerKeys)
        {
            to.GetHeaders().Remove(key);
        }

        foreach (var entry in from.GetHeaders().Where(entry => !FilteredHeaders.Contains(entry.Key)))
        {
            to.GetHeaders()[entry.Key] = entry.Value;
        }
    }

    public override GraphQLMessage GetMessage()
    {
        return (GraphQLMessage)base.GetMessage();
    }

    /// <summary>
    ///     Creates a new GraphQLMessageBuilder for building a query.
    /// </summary>
    /// <param name="query">The GraphQL query string.</param>
    /// <returns>A new GraphQLMessageBuilder configured for a query.</returns>
    public static GraphQLMessageBuilder CreateQuery(string query)
    {
        return new GraphQLMessageBuilder()
            .Query(query)
            .OperationType(GraphQLOperationType.QUERY);
    }

    /// <summary>
    ///     Creates a new GraphQLMessageBuilder for building a mutation.
    /// </summary>
    /// <param name="mutation">The GraphQL mutation string.</param>
    /// <returns>A new GraphQLMessageBuilder configured for a mutation.</returns>
    public static GraphQLMessageBuilder CreateMutation(string mutation)
    {
        return new GraphQLMessageBuilder()
            .Query(mutation)
            .OperationType(GraphQLOperationType.MUTATION);
    }

    /// <summary>
    ///     Creates a new GraphQLMessageBuilder for building a subscription.
    /// </summary>
    /// <param name="subscription">The GraphQL subscription string.</param>
    /// <returns>A new GraphQLMessageBuilder configured for a subscription.</returns>
    public static GraphQLMessageBuilder CreateSubscription(string subscription)
    {
        return new GraphQLMessageBuilder()
            .Query(subscription)
            .OperationType(GraphQLOperationType.SUBSCRIPTION);
    }

    /// <summary>
    ///     Creates a new GraphQLMessageBuilder for building an introspection query.
    /// </summary>
    /// <returns>A new GraphQLMessageBuilder configured for introspection.</returns>
    public static GraphQLMessageBuilder CreateIntrospection()
    {
        var introspectionMessage = GraphQLMessageUtils.CreateIntrospectionQuery();
        return new GraphQLMessageBuilder(introspectionMessage);
    }

    /// <summary>
    ///     Creates a new GraphQLMessageBuilder from a JSON payload.
    /// </summary>
    /// <param name="jsonPayload">The JSON string containing the GraphQL request.</param>
    /// <returns>A new GraphQLMessageBuilder.</returns>
    public static GraphQLMessageBuilder FromJson(string jsonPayload)
    {
        var message = GraphQLMessageUtils.FromJsonPayload(jsonPayload);
        return new GraphQLMessageBuilder(message);
    }
}
