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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Endpoint;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Core.Message;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Actions;

/// <summary>
///     Action expecting a timeout on a message destination, this means that no message should arrive at the destination.
/// </summary>
public class ReceiveTimeoutAction : AbstractTestActionAsync
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(ReceiveTimeoutAction));

    /// Represents an asynchronous action that expects a timeout where no messages
    /// should arrive at the specified endpoint destination.
    public ReceiveTimeoutAction(Builder builder) : base("receive-timeout", builder)
    {
        Endpoint = builder.MessageEndpoint;
        EndpointUri = builder.EndpointUri;
        Timeout = builder._timeout;
        MessageSelector = builder.MessageSelector;
        MessageSelectorDictionary = builder.MessageSelectorDictionary;
    }

    /// The timeout duration in milliseconds for the receive operation.
    /// /
    public long Timeout { get; }

    /// The communication endpoint used for sending and receiving messages.
    public IEndpoint Endpoint { get; }

    /// <summary>
    ///     URI of the endpoint.
    /// </summary>
    public string EndpointUri { get; }

    /// <summary>
    ///     A dictionary containing key-value pairs used to specify selectors
    ///     for message filtering and selection in the context of the action.
    /// </summary>
    public Dictionary<string, object> MessageSelectorDictionary { get; }

    /// <summary>
    ///     Defines the criteria used to select messages in the receive timeout action.
    /// </summary>
    public string MessageSelector { get; }

    /// Executes the action asynchronously, performing message reception with a timeout validation.
    /// @param context The test context that provides execution resources and state.
    /// @param cancellation The token to monitor for cancellation requests during execution.
    /// @return A task representing the asynchronous operation.
    public override async Task DoExecute(TestContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            IMessage receivedMessage;
            var consumer = GetOrCreateEndpoint(context).CreateConsumer();

            var selector = MessageSelectorBuilder.Build(MessageSelector, MessageSelectorDictionary, context);

            if (!string.IsNullOrWhiteSpace(MessageSelector) && consumer is ISelectiveConsumer selectiveConsumer)
            {
                receivedMessage = await selectiveConsumer.Receive(selector, context, Timeout);
            }
            else
            {
                receivedMessage = await consumer.Receive(context, Timeout);
            }

            if (receivedMessage != null)
            {
                if (Log.IsEnabled(LogLevel.Debug))
                {
                    Log.LogDebug("Received message: {ReceiveMessagePrint}\n", receivedMessage.Print(context));
                }

                throw new AgenixSystemException("Message timeout validation failed! " +
                                                "Received message while waiting for timeout on destination");
            }
        }
        catch (ActionTimeoutException e)
        {
            Log.LogInformation(e, "No messages received on destination. Message timeout validation OK!");
            Log.LogInformation(e, e.Message);
        }
    }

    /// Creates or gets the endpoint instance.
    /// @param context Test context containing the necessary configuration and endpoint factory.
    /// @return The created or retrieved endpoint instance.
    /// /
    public IEndpoint GetOrCreateEndpoint(TestContext context)
    {
        if (Endpoint != null)
        {
            return Endpoint;
        }

        if (!string.IsNullOrWhiteSpace(EndpointUri))
        {
            return context.EndpointFactory.Create(EndpointUri, context);
        }

        throw new AgenixSystemException("Neither endpoint nor endpoint uri is set properly!");
    }

    /// The Builder class assists in the construction of ReceiveTimeoutAction instances using a fluent interface.
    /// /
    public sealed class Builder : AbstractAsyncTestActionBuilder<IAsyncTestAction, dynamic>
    {
        // ReSharper disable once InconsistentNaming
        internal long _timeout = 1000L;

        /// <summary>
        ///     Represents the endpoint used for messaging processes within the builder configuration.
        ///     Provides an instance of <see cref="IEndpoint" /> that supports producing and consuming messages.
        /// </summary>
        public IEndpoint MessageEndpoint { get; private set; }

        /// <summary>
        ///     Represents the URI of the endpoint associated with the action.
        /// </summary>
        public string EndpointUri { get; private set; }

        /// <summary>
        ///     A dictionary that represents message selectors, where the keys are selector names
        ///     and the values are corresponding selector values, used to configure message filtering.
        /// </summary>
        public Dictionary<string, object> MessageSelectorDictionary { get; private set; } = new();

        /// <summary>
        ///     Defines a string-based criteria for selecting messages,
        ///     typically used to filter and determine the specific message
        ///     or messages of interest during execution.
        /// </summary>
        public string MessageSelector { get; private set; }


        /// Fluent API action building entry method used in C# DSL.
        /// @param endpointUri The URI of the endpoint to receive a timeout.
        /// @return The current instance of the Builder to allow for method chaining.
        /// /
        public Builder ExpectTimeout(string endpointUri)
        {
            return ReceiveTimeout(endpointUri);
        }

        /**
         * Fluent API action building entry method used in C# DSL.
         * @param endpoint
         * @return
         */
        public Builder ExpectTimeout(IEndpoint endpoint)
        {
            return ReceiveTimeout(endpoint);
        }

        /// Fluent API action building entry method used in C# DSL.
        /// @param endpointUri The URI of the endpoint to receive a timeout.
        /// @return The current instance of the Builder to allow for method chaining.
        /// /
        public static Builder ReceiveTimeout(string endpointUri)
        {
            var builder = new Builder();
            builder.Endpoint(endpointUri);
            return builder;
        }

        /// Fluent API action building entry method used in C# DSL.
        /// @param endpointUri
        /// @return
        /// /
        public static Builder ReceiveTimeout(IEndpoint endpoint)
        {
            var builder = new Builder();
            builder.Endpoint(endpoint);
            return builder;
        }

        /// Sets the message endpoint to receive a timeout with.
        /// @param messageEndpoint The endpoint where the message will be received.
        /// @return The current instance of the Builder.
        /// /
        public Builder Endpoint(IEndpoint messageEndpoint)
        {
            MessageEndpoint = messageEndpoint;
            return this;
        }

        /// Sets the message endpoint to receive a timeout with.
        /// @param messageEndpoint
        /// @return
        /// /
        public Builder Endpoint(string messageEndpointUri)
        {
            EndpointUri = messageEndpointUri;
            return this;
        }

        /// Sets time to wait for messages on destination.
        /// @param timeout Time in milliseconds to wait for a message.
        /// @return Builder instance with the specified timeout value.
        /// /
        public Builder Timeout(long timeout)
        {
            _timeout = timeout;
            return this;
        }

        /// Adds message selector string for selective consumer.
        /// @param messageSelector The message selector string.
        /// @return This builder instance for method chaining.
        /// /
        public Builder Selector(string messageSelector)
        {
            MessageSelector = messageSelector;
            return this;
        }

        /// Sets the message selector.
        /// @param messageSelector a dictionary containing the message selection criteria
        /// @return the builder instance for fluent chaining
        /// /
        public Builder Selector(Dictionary<string, object> messageSelector)
        {
            MessageSelectorDictionary = messageSelector;
            return this;
        }

        /// Constructs and returns a new instance of ReceiveTimeoutAction.
        /// <return>Returns a new instance of ReceiveTimeoutAction configured based on the builder setup.</return>
        public override ReceiveTimeoutAction Build()
        {
            return new ReceiveTimeoutAction(this);
        }
    }
}
