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

using System.Net;
using Agenix.Api.Message;
using Agenix.Api.Util;
using Agenix.Core.Actions;
using Agenix.Core.Message.Builder;
using Agenix.GraphQL.Message;

namespace Agenix.GraphQL.Actions;

/// <summary>
/// Facilitates the creation and configuration of GraphQL client response actions.
/// This class extends the capabilities of ReceiveMessageAction.Builder to provide specialized support
/// for constructing GraphQL response messages. It allows users to define message properties such as
/// GraphQL data, errors, extensions, and metadata, which are tailored for handling GraphQL interactions.
/// </summary>
public class GraphQLClientResponseActionBuilder : ReceiveMessageAction.ReceiveMessageActionBuilder<ReceiveMessageAction,
    GraphQLClientResponseActionBuilder.GraphQLMessageBuilderSupport, GraphQLClientResponseActionBuilder>
{
    private readonly GraphQLMessage _graphQLMessage;

    public GraphQLClientResponseActionBuilder()
    {
        _graphQLMessage = new GraphQLMessage();
        Message(new GraphQLMessageBuilder(_graphQLMessage)).HeaderIgnoreCase = true;
    }

    public GraphQLClientResponseActionBuilder(IMessageBuilder messageBuilder, GraphQLMessage graphQLMessage)
    {
        _graphQLMessage = graphQLMessage;
        Message(messageBuilder).HeaderIgnoreCase = true;
    }

    /// <summary>
    /// Retrieves the GraphQL message builder support instance for customizing the construction of GraphQL messages.
    /// This method creates a new `GraphQLMessageBuilderSupport` instance if it has not been initialized.
    /// It ensures that the base implementation is invoked and the proper type is returned.
    /// </summary>
    /// <returns>The `GraphQLMessageBuilderSupport` instance associated with this builder.</returns>
    public override GraphQLMessageBuilderSupport GetMessageBuilderSupport()
    {
        messageBuilderSupport ??= new GraphQLMessageBuilderSupport(_graphQLMessage, this);

        return base.GetMessageBuilderSupport();
    }

    /// <summary>
    /// Retrieves the message payload as an optional string.
    /// Returns the payload from the associated GraphQLMessage if it exists and is a string.
    /// If no valid payload is found, retrieves the payload from the base implementation.
    /// </summary>
    /// <returns>An optional string containing the message payload or empty if no payload is present.</returns>
    protected override Api.Util.Optional<string> GetMessagePayload()
    {
        return _graphQLMessage.Payload is string
            ? Api.Util.Optional<string>.Of(_graphQLMessage.GetPayload<string>())
            : base.GetMessagePayload();
    }

    protected override ReceiveMessageAction DoBuild()
    {
        var builder = new ReceiveMessageAction.Builder();
        builder.Name(GetName());
        builder.Description(GetDescription());
        builder.Endpoint(GetEndpoint());
        builder.Endpoint(GetEndpointUri());
        builder.Timeout(_receiveTimeout);
        builder.Selector(_messageSelector);
        builder.Selector(_messageSelectors);
        builder.Validators(_validators);
        builder.Validate(ValidationContexts);

        if (_validationProcessor != null)
        {
            builder.Process(_validationProcessor);
        }

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

        foreach (var controlMessageProcessor in GetMessageBuilderSupport().ControlMessageProcessors)
        {
            builder.GetMessageBuilderSupport().ControlMessageProcessors.Add(controlMessageProcessor);
        }

        return new ReceiveMessageAction(builder);
    }

    /// <summary>
    /// Provides support for building GraphQL messages within the context of the ReceiveMessageAction.
    /// This class serves as a helper for configuring various properties of a GraphQL message, including
    /// the name, payload, data, errors, extensions, and associated metadata required for GraphQL message handling.
    /// </summary>
    public class GraphQLMessageBuilderSupport(GraphQLMessage graphQLMessage, GraphQLClientResponseActionBuilder dlg)
        : ReceiveMessageBuilderSupport<ReceiveMessageAction, GraphQLClientResponseActionBuilder,
            GraphQLMessageBuilderSupport>(dlg)
    {
        private GraphQLMessage graphQLMessage = graphQLMessage;

        /// <summary>
        /// Sets the name of the GraphQL message and returns the current builder instance for method chaining.
        /// This method updates the `Name` property of the associated GraphQL message and invokes the base implementation.
        /// </summary>
        /// <param name="name">The name to be assigned to the GraphQL message.</param>
        /// <returns>The current builder instance, allowing for method chaining.</returns>
        public override GraphQLMessageBuilderSupport Name(string name)
        {
            graphQLMessage.Name = name;
            return base.Name(name);
        }

        /// <summary>
        /// Updates the body (payload) of the GraphQL message and returns the current builder instance for method chaining.
        /// This method sets the `Payload` property of the associated `GraphQLMessage`.
        /// </summary>
        /// <param name="payload">The payload to be assigned to the GraphQL message body.</param>
        /// <returns>The current builder instance, allowing for method chaining.</returns>
        public override GraphQLMessageBuilderSupport Body(string payload)
        {
            graphQLMessage.Payload = payload;
            return this;
        }

        /// <summary>
        /// Copies the properties of the specified `IMessage` instance to the internal `GraphQLMessage` instance.
        /// This method utilizes the `GraphQLMessageUtils.Copy` utility to transfer data from the source message.
        /// </summary>
        /// <param name="controlMessage">The source message from which properties will be copied.</param>
        /// <returns>The current `GraphQLMessageBuilderSupport` instance, allowing for method chaining.</returns>
        public override GraphQLMessageBuilderSupport From(IMessage controlMessage)
        {
            GraphQLMessageUtils.Copy(controlMessage, graphQLMessage);
            return this;
        }

        /// <summary>
        /// Sets the GraphQL operation name for the message and returns the current builder instance for method chaining.
        /// </summary>
        /// <param name="operationName">The GraphQL operation name to be assigned to the message.</param>
        /// <returns>The current builder instance, allowing for method chaining.</returns>
        public GraphQLMessageBuilderSupport OperationName(string operationName)
        {
            graphQLMessage.SetOperationName(operationName);
            return this;
        }

        /// <summary>
        /// Sets the GraphQL query for the message and returns the current builder instance for method chaining.
        /// </summary>
        /// <param name="query">The GraphQL query to be assigned to the message.</param>
        /// <returns>The current builder instance, allowing for method chaining.</returns>
        public GraphQLMessageBuilderSupport Query(string query)
        {
            graphQLMessage.Payload = query;
            return this;
        }

        /// <summary>
        /// Sets the GraphQL variables for the message and returns the current builder instance for method chaining.
        /// </summary>
        /// <param name="variables">The variables dictionary to be assigned to the GraphQL message.</param>
        /// <returns>The current builder instance, allowing for method chaining.</returns>
        public GraphQLMessageBuilderSupport Variables(Dictionary<string, object> variables)
        {
            graphQLMessage.Variables = variables;
            return this;
        }

        /// <summary>
        /// Adds a single variable to the GraphQL message and returns the current builder instance for method chaining.
        /// </summary>
        /// <param name="name">The name of the variable to add.</param>
        /// <param name="value">The value of the variable to add.</param>
        /// <returns>The current builder instance, allowing for method chaining.</returns>
        public GraphQLMessageBuilderSupport Variable(string name, object value)
        {
            graphQLMessage.SetVariable(name, value);
            return this;
        }

        /// Sets the HTTP status code for the HTTP message and returns the current builder instance for method chaining.
        /// This method updates the `Status` property of the associated HTTP message and invokes the `Status` method of the `HttpMessage` instance.
        /// <param name="httpStatusCode">The HTTP status code to be assigned to the message.</param>
        /// <return>The current builder instance, allowing for method chaining.</return>
        public GraphQLMessageBuilderSupport Status(HttpStatusCode httpStatusCode)
        {
            graphQLMessage.Status(httpStatusCode);
            return this;
        }

        /// Assigns the provided HTTP status code as an integer to the message and updates the corresponding HTTP headers.
        /// This method sets the status code by converting it to the `HttpStatusCode` type and modifies the message accordingly.
        /// <param name="statusCode">The integer value of the status code to be assigned to the HTTP message.</param>
        /// <return>The current builder instance, enabling method chaining for further configuration.</return>
        public GraphQLMessageBuilderSupport StatusCode(int statusCode)
        {
            graphQLMessage.Status((HttpStatusCode)statusCode);
            return this;
        }

        /// <summary>
        /// Sets the expected GraphQL extensions and returns the current builder instance for method chaining.
        /// </summary>
        /// <param name="extensions">The extensions dictionary for the GraphQL response.</param>
        /// <returns>The current builder instance, allowing for method chaining.</returns>
        public GraphQLMessageBuilderSupport Extensions(Dictionary<string, object> extensions)
        {
            graphQLMessage.Extensions = (extensions);
            return this;
        }

        /// <summary>
        /// Adds a single extension to the GraphQL message and returns the current builder instance for method chaining.
        /// </summary>
        /// <param name="name">The name of the extension to add.</param>
        /// <param name="value">The value of the extension to add.</param>
        /// <returns>The current builder instance, allowing for method chaining.</returns>
        public GraphQLMessageBuilderSupport Extension(string name, object value)
        {
            graphQLMessage.Extensions[name] = value;
            return this;
        }

        /// <summary>
        /// Sets whether WebSocket should be used for this GraphQL message and returns the current builder instance for method chaining.
        /// </summary>
        /// <param name="useWebSocket">True to use WebSocket, false for HTTP.</param>
        /// <returns>The current builder instance, allowing for method chaining.</returns>
        public GraphQLMessageBuilderSupport UseWebSocket(bool useWebSocket = true)
        {
            graphQLMessage.UseWebSocket(useWebSocket);
            return this;
        }
    }
}
