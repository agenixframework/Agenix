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
using Agenix.Api.Message;
using Agenix.Api.Util;
using Agenix.Core.Actions;
using Agenix.Core.Message.Builder;
using Agenix.Http.Message;

namespace Agenix.Http.Actions;

/// Facilitates the creation and configuration of HTTP client response actions.
/// This class extends the capabilities of ReceiveMessageAction.Builder to provide specialized support
/// for constructing HTTP response messages. It allows users to define message properties such as headers,
/// payload, and metadata, which are tailored for handling HTTP interactions.
public class HttpClientResponseActionBuilder : ReceiveMessageAction.ReceiveMessageActionBuilder<ReceiveMessageAction,
    HttpClientResponseActionBuilder.HttpMessageBuilderSupport, HttpClientResponseActionBuilder>
{
    private readonly HttpMessage _httpMessage;

    /// Represents a builder for constructing HTTP client response actions with specified configurations and settings.
    /// This builder initializes an HTTP message and provides default configurations such as case-insensitive headers.
    /// It extends the functionality of `ReceiveMessageActionBuilder` with specific support for HTTP client responses.
    public HttpClientResponseActionBuilder()
    {
        _httpMessage = new HttpMessage();
        Message(new HttpMessageBuilder(_httpMessage)).HeaderIgnoreCase = true;
    }

    /// Represents a specialized action builder for handling HTTP client responses with custom configurations and functionalities.
    /// This class extends the `ReceiveMessageAction.ReceiveMessageActionBuilder` to provide specific support for HTTP message handling.
    /// It initializes an instance of `HttpMessage` and enables customization such as case-insensitive header settings.
    /// Provides methods and functionality to construct and manage HTTP response message actions effectively.
    public HttpClientResponseActionBuilder(IMessageBuilder messageBuilder, HttpMessage httpMessage)
    {
        _httpMessage = httpMessage;
        Message(messageBuilder).HeaderIgnoreCase = true;
    }

    /// Retrieves the HTTP message builder support instance for customizing the construction of HTTP messages.
    /// This method creates a new `HttpMessageBuilderSupport` instance if it has not been initialized.
    /// It ensures that the base implementation is invoked and the proper type is returned.
    /// <return>The `HttpMessageBuilderSupport` instance associated with this builder.</return>
    public override HttpMessageBuilderSupport GetMessageBuilderSupport()
    {
        messageBuilderSupport ??= new HttpMessageBuilderSupport(_httpMessage, this);

        return base.GetMessageBuilderSupport();
    }

    /// Retrieves the message payload as an optional string.
    /// Returns the payload from the associated HttpMessage if it exists and is a string.
    /// If no valid payload is found, retrieves the payload from the base implementation.
    /// <return>An optional string containing the message payload or empty if no payload is present.</return>
    protected override Optional<string> GetMessagePayload()
    {
        return _httpMessage.Payload is string
            ? Optional<string>.Of(_httpMessage.GetPayload<string>())
            : base.GetMessagePayload();
    }

    /// Constructs the `ReceiveMessageAction` instance using the configured properties of the builder.
    /// This method initializes a `ReceiveMessageAction.Builder` and sets its attributes such as name,
    /// description, endpoint, timeout, message selectors, validators, and processors based on the builder's configuration.
    /// Additionally, message builder support and control message processors are applied to the constructed builder.
    /// Implements the logic required to fully construct a `ReceiveMessageAction` before returning it.
    /// <return>A fully constructed `ReceiveMessageAction` instance.</return>
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

    /// Provides support for building HTTP messages within the context of the ReceiveMessageAction.
    /// This class serves as a helper for configuring various properties of an HTTP message, including
    /// the name, payload, status, and associated metadata required for HTTP message handling.
    public class HttpMessageBuilderSupport(HttpMessage httpMessage, HttpClientResponseActionBuilder dlg)
        : ReceiveMessageBuilderSupport<ReceiveMessageAction, HttpClientResponseActionBuilder,
            HttpMessageBuilderSupport>(dlg)
    {
        private readonly HttpMessage httpMessage = httpMessage;


        /// Sets the name of the HTTP message and returns the current builder instance for method chaining.
        /// This method updates the `Name` property of the associated HTTP message and invokes the base implementation.
        /// <param name="name">The name to be assigned to the HTTP message.</param>
        /// <return>The current builder instance, allowing for method chaining.</return>
        public override HttpMessageBuilderSupport Name(string name)
        {
            httpMessage.Name = name;
            return base.Name(name);
        }

        /// Updates the body (payload) of the HTTP message and returns the current builder instance for method chaining.
        /// This method sets the `Payload` property of the associated `HttpMessage`.
        /// <param name="payload">The payload to be assigned to the HTTP message body.</param>
        /// <return>The current builder instance, allowing for method chaining.</return>
        public override HttpMessageBuilderSupport Body(string payload)
        {
            httpMessage.Payload = payload;
            return this;
        }

        /// Copies the properties of the specified `IMessage` instance to the internal `HttpMessage` instance.
        /// This method utilizes the `HttpMessageUtils.Copy` utility to transfer data from the source message.
        /// <param name="controlMessage">The source message from which properties will be copied.</param>
        /// <return>The current `HttpMessageBuilderSupport` instance, allowing for method chaining.</return>
        public override HttpMessageBuilderSupport From(IMessage controlMessage)
        {
            HttpMessageUtils.Copy(controlMessage, httpMessage);
            return this;
        }

        /// Sets the HTTP status code for the HTTP message and returns the current builder instance for method chaining.
        /// This method updates the `Status` property of the associated HTTP message and invokes the `Status` method of the `HttpMessage` instance.
        /// <param name="httpStatusCode">The HTTP status code to be assigned to the message.</param>
        /// <return>The current builder instance, allowing for method chaining.</return>
        public HttpMessageBuilderSupport Status(HttpStatusCode httpStatusCode)
        {
            httpMessage.Status(httpStatusCode);
            return this;
        }

        /// Assigns the provided HTTP status code as an integer to the message and updates the corresponding HTTP headers.
        /// This method sets the status code by converting it to the `HttpStatusCode` type and modifies the message accordingly.
        /// <param name="statusCode">The integer value of the status code to be assigned to the HTTP message.</param>
        /// <return>The current builder instance, enabling method chaining for further configuration.</return>
        public HttpMessageBuilderSupport StatusCode(int statusCode)
        {
            httpMessage.Status((HttpStatusCode)statusCode);
            return this;
        }

        /// Sets the HTTP response reason phrase and returns the current builder instance for method chaining.
        /// This method updates the `ReasonPhrase` property of the associated HTTP message.
        /// <param name="reasonPhrase">The reason phrase to be assigned to the HTTP message.</param>
        /// <return>The current builder instance, enabling continued method chaining.</return>
        public HttpMessageBuilderSupport ReasonPhrase(string reasonPhrase)
        {
            httpMessage.ReasonPhrase(reasonPhrase);
            return this;
        }

        /// Sets the HTTP version of the message and returns the current builder instance for method chaining.
        /// This method updates the `Version` property of the associated HTTP message and invokes the base implementation.
        /// <param name="version">The HTTP version to be assigned to the message.</param>
        /// <return>The current builder instance, allowing for method chaining.</return>
        public HttpMessageBuilderSupport Version(string version)
        {
            httpMessage.Version(version);
            return this;
        }

        /// Sets the content type of the HTTP message and returns the current builder instance for method chaining.
        /// This method updates the `ContentType` property of the associated HTTP message.
        /// <param name="contentType">The content type value to be assigned to the HTTP message.</param>
        /// <return>The current builder instance, allowing for method chaining.</return>
        public HttpMessageBuilderSupport ContentType(string contentType)
        {
            httpMessage.ContentType(contentType);
            return this;
        }

        /// Adds a cookie to the HTTP message and returns the current builder instance for method chaining.
        /// This method adds or updates the cookie in the associated HTTP message and modifies the headers accordingly.
        /// <param name="cookie">The Cookie object to be added or updated in the HTTP message.</param>
        /// <return>The current builder instance, enabling method chaining.</return>
        public HttpMessageBuilderSupport Cookie(Cookie cookie)
        {
            httpMessage.Cookie(cookie);
            return this;
        }
    }
}
