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

using Agenix.Api.Message;
using Agenix.Core.Actions;
using Agenix.Core.Message.Builder;
using Agenix.GraphQL.Message;

namespace Agenix.GraphQL.Actions;

/// <summary>
/// Provides functionality for building GraphQL client requests with customizable
/// attributes such as GraphQL query, mutation, subscription, variables, and more.
/// This builder extends the capabilities of `SendMessageAction.Builder`
/// by adding GraphQL-specific configurations and behaviors.
/// </summary>
public class GraphQLClientRequestActionBuilder : SendMessageAction.SendMessageActionBuilder<SendMessageAction,
    GraphQLClientRequestActionBuilder.GraphQLMessageBuilderSupport, GraphQLClientRequestActionBuilder>
{
    private readonly GraphQLMessage _graphQLMessage;

    /// <summary>
    /// Initializes a new instance of the GraphQLClientRequestActionBuilder with a default GraphQL message.
    /// </summary>
    public GraphQLClientRequestActionBuilder()
    {
        _graphQLMessage = new GraphQLMessage();
        Message(new GraphQLMessageBuilder(_graphQLMessage));
    }


    /// <summary>
    /// Initializes a new instance of the GraphQLClientRequestActionBuilder with a specific message builder and GraphQL message.
    /// </summary>
    /// <param name="messageBuilder">The message builder to use.</param>
    /// <param name="graphQLMessage">The GraphQL message to configure.</param>
    public GraphQLClientRequestActionBuilder(IMessageBuilder messageBuilder, GraphQLMessage graphQLMessage)
    {
        _graphQLMessage = graphQLMessage;
        Message(messageBuilder);
    }

    /// <summary>
    /// Retrieves the instance of the GraphQLMessageBuilderSupport associated with the current builder.
    /// </summary>
    /// <returns>The instance of GraphQLMessageBuilderSupport initialized for this request.</returns>
    public override GraphQLMessageBuilderSupport GetMessageBuilderSupport()
    {
        messageBuilderSupport ??= new GraphQLMessageBuilderSupport(_graphQLMessage, this);
        return base.GetMessageBuilderSupport();
    }

    /// <summary>
    /// Sets the GraphQL query for the request.
    /// </summary>
    /// <param name="query">The GraphQL query string.</param>
    /// <returns>The updated instance of GraphQLClientRequestActionBuilder.</returns>
    public GraphQLClientRequestActionBuilder Query(string query)
    {
        _graphQLMessage.Payload = query;
        return this;
    }

    /// <summary>
    /// Sets the GraphQL mutation for the request.
    /// </summary>
    /// <param name="mutation">The GraphQL mutation string.</param>
    /// <returns>The updated instance of GraphQLClientRequestActionBuilder.</returns>
    public GraphQLClientRequestActionBuilder Mutation(string mutation)
    {
        return Query(mutation);
    }

    /// <summary>
    /// Sets the GraphQL subscription for the request.
    /// </summary>
    /// <param name="subscription">The GraphQL subscription string.</param>
    /// <returns>The updated instance of GraphQLClientRequestActionBuilder.</returns>
    public GraphQLClientRequestActionBuilder Subscription(string subscription)
    {
        return Query(subscription);
    }

    /// <summary>
    /// Sets the operation name for the GraphQL request.
    /// </summary>
    /// <param name="operationName">The operation name.</param>
    /// <returns>The updated instance of GraphQLClientRequestActionBuilder.</returns>
    public GraphQLClientRequestActionBuilder OperationName(string operationName)
    {
        _graphQLMessage.SetOperationName(operationName);
        return this;
    }

    /// <summary>
    /// Sets the operation name for the GraphQL request.
    /// </summary>
    /// <param name="operationName">The operation name.</param>
    /// <returns>The updated instance of GraphQLClientRequestActionBuilder.</returns>
    public GraphQLClientRequestActionBuilder OperationType(GraphQLOperationType operationType)
    {
        _graphQLMessage.SetOperationType(operationType);
        return this;
    }

    /// <summary>
    /// Adds a variable to the GraphQL request.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <param name="value">The variable value.</param>
    /// <returns>The updated instance of GraphQLClientRequestActionBuilder.</returns>
    public GraphQLClientRequestActionBuilder Variable(string name, object value)
    {
        _graphQLMessage.SetVariable(name, value);
        return this;
    }

    /// <summary>
    /// Sets multiple variables for the GraphQL request.
    /// </summary>
    /// <param name="variables">Dictionary of variables to set.</param>
    /// <returns>The updated instance of GraphQLClientRequestActionBuilder.</returns>
    public GraphQLClientRequestActionBuilder Variables(IDictionary<string, object> variables)
    {
        _graphQLMessage.Variables = variables;
        return this;
    }

    /// <summary>
    /// Sets the GraphQL endpoint URI for the request.
    /// </summary>
    /// <param name="endpointUri">The GraphQL endpoint URI.</param>
    /// <returns>The updated instance of GraphQLClientRequestActionBuilder.</returns>
    public GraphQLClientRequestActionBuilder Uri(string endpointUri)
    {
        _graphQLMessage.Uri(endpointUri);
        return this;
    }

    /// <summary>
    /// Enables WebSocket for subscriptions.
    /// </summary>
    /// <param name="useWebSocket">Whether to use WebSocket for subscriptions.</param>
    /// <returns>The updated instance of GraphQLClientRequestActionBuilder.</returns>
    public GraphQLClientRequestActionBuilder UseWebSocket(bool useWebSocket = true)
    {
        _graphQLMessage.UseWebSocket(useWebSocket);
        return this;
    }

    /// <summary>
    /// Sets an authorization header for the GraphQL request.
    /// </summary>
    /// <param name="token">The authorization token.</param>
    /// <param name="scheme">The authorization scheme (default: Bearer).</param>
    /// <returns>The updated instance of GraphQLClientRequestActionBuilder.</returns>
    public GraphQLClientRequestActionBuilder Authorization(string token, string scheme = "Bearer")
    {
        _graphQLMessage.SetHeader("Authorization", $"{scheme} {token}");
        return this;
    }

    /// <summary>
    /// Adds a custom header to the GraphQL request.
    /// </summary>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The updated instance of GraphQLClientRequestActionBuilder.</returns>
    public GraphQLClientRequestActionBuilder Header(string name, object value)
    {
        _graphQLMessage.SetHeader(name, value);
        return this;
    }

    /// <summary>
    /// Sets the content type for the GraphQL request.
    /// </summary>
    /// <param name="contentType">The content type (default: application/json).</param>
    /// <returns>The updated instance of GraphQLClientRequestActionBuilder.</returns>
    public GraphQLClientRequestActionBuilder ContentType(string contentType)
    {
        _graphQLMessage.SetHeader("Content-Type", contentType);
        return this;
    }

    /// <summary>
    /// Sets the accept header for the GraphQL response.
    /// </summary>
    /// <param name="accept">The accept header value.</param>
    /// <returns>The updated instance of GraphQLClientRequestActionBuilder.</returns>
    public GraphQLClientRequestActionBuilder Accept(string accept)
    {
        _graphQLMessage.SetHeader("Accept", accept);
        return this;
    }

    /// <summary>
    /// Builds the SendMessageAction for executing the GraphQL request.
    /// </summary>
    /// <returns>The configured SendMessageAction.</returns>
    protected override SendMessageAction DoBuild()
    {
        var builder = new SendMessageAction.Builder();
        builder.Name(GetName());
        builder.Description(GetDescription());
        builder.Fork(ForkMode);
        builder.Endpoint(GetEndpoint());
        builder.Endpoint(GetEndpointUri());

        foreach (var extractor in GetVariableExtractors())
        {
            builder.Process(extractor);
        }

        foreach (var processor in GetMessageProcessors())
        {
            builder.Process(processor);
        }

        builder.GetMessageBuilderSupport().From(GetMessageBuilderSupport().GetMessageBuilder());
        builder.GetMessageBuilderSupport().Type(GetMessageBuilderSupport().GetMessageType());

        return new SendMessageAction(builder);
    }

    /// <summary>
    /// Builder support class for GraphQL message configuration.
    /// Provides additional GraphQL-specific configuration methods.
    /// </summary>
    public class GraphQLMessageBuilderSupport(
        GraphQLMessage graphQLMessage,
        GraphQLClientRequestActionBuilder newDelegate)
        : SendMessageBuilderSupport<SendMessageAction, GraphQLClientRequestActionBuilder, GraphQLMessageBuilderSupport>(
            newDelegate)
    {
        /// <summary>
        /// Sets the GraphQL query through the builder support.
        /// </summary>
        /// <param name="query">The GraphQL query string.</param>
        /// <returns>The GraphQLMessageBuilderSupport instance.</returns>
        public GraphQLMessageBuilderSupport Query(string query)
        {
            graphQLMessage.Payload = query;
            return this;
        }

        /// <summary>
        /// Sets the body of the GraphQL message to the specified content.
        /// </summary>
        /// <param name="body">The content to set as the body of the GraphQL message.</param>
        /// <returns>Returns the current instance of <see cref="GraphQLMessageBuilderSupport"/> after setting the body.</returns>
        public override GraphQLMessageBuilderSupport Body(string body)
        {
            return Query(body);
        }

        /// Sets the name for the HTTP message in the builder.
        /// <param name="name">The name to assign to the HTTP message.</param>
        /// <return>The updated instance of HttpMessageBuilderSupport.</return>
        public override GraphQLMessageBuilderSupport Name(string name)
        {
            graphQLMessage.Name = name;
            return base.Name(name);
        }

        /// Copies the details from the provided control message to the current HTTP message instance.
        /// <param name="controlMessage">The control message from which to copy information.</param>
        /// <return>The updated instance of HttpMessageBuilderSupport.</return>
        public override GraphQLMessageBuilderSupport From(IMessage controlMessage)
        {
            GraphQLMessageUtils.Copy(controlMessage, graphQLMessage);
            return this;
        }

        /// <summary>
        /// Adds a variable through the builder support.
        /// </summary>
        /// <param name="name">The variable name.</param>
        /// <param name="value">The variable value.</param>
        /// <returns>The GraphQLMessageBuilderSupport instance.</returns>
        public GraphQLMessageBuilderSupport Variable(string name, object value)
        {
            graphQLMessage.SetVariable(name, value);
            return this;
        }

        /// <summary>
        /// Sets multiple variables through the builder support.
        /// </summary>
        /// <param name="variables">Dictionary of variables to set.</param>
        /// <returns>The GraphQLMessageBuilderSupport instance.</returns>
        public GraphQLMessageBuilderSupport Variables(IDictionary<string, object> variables)
        {
            graphQLMessage.Variables = variables;
            return this;
        }

        /// <summary>
        /// Adds or updates an extension for the GraphQL message.
        /// </summary>
        /// <param name="name">The name of the extension to add or update.</param>
        /// <param name="value">The value of the extension.</param>
        /// <returns>The current instance of <see cref="GraphQLMessageBuilderSupport"/> for method chaining.</returns>
        public GraphQLMessageBuilderSupport Extension(string name, object value)
        {
            graphQLMessage.SetExtension(name, value);
            return this;
        }

        /// <summary>
        /// Sets the extensions for the GraphQL message through the builder support.
        /// </summary>
        /// <param name="extensions">Dictionary of extensions to set.</param>
        /// <returns>The GraphQLMessageBuilderSupport instance.</returns>
        public GraphQLMessageBuilderSupport Extensions(Dictionary<string, object> extensions)
        {
            graphQLMessage.Extensions = extensions;
            return this;
        }

        /// Sets the HTTP message content type header in the builder.
        /// <param name="contentType">The content type header value to assign to the HTTP message.</param>
        /// <return>The updated instance of HttpMessageBuilderSupport.</return>
        public GraphQLMessageBuilderSupport ContentType(string contentType)
        {
            graphQLMessage.ContentType(contentType);
            return this;
        }

        /// Sets the HTTP "Accept" header for the message being built.
        /// <param name="accept">The value to assign to the "Accept" header.</param>
        /// <return>The updated instance of HttpMessageBuilderSupport.</return>
        public GraphQLMessageBuilderSupport Accept(string accept)
        {
            graphQLMessage.Accept(accept);
            return this;
        }
    }
}
