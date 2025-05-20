using System.Collections.Generic;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Endpoint;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Core.Endpoint;
using Agenix.Core.Message;
using log4net;

namespace Agenix.Core.Actions;

/// <summary>
///     Action expecting a timeout on a message destination, this means that no message should arrive at the destination.
/// </summary>
public class ReceiveTimeoutAction : AbstractTestAction
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(ReceiveTimeoutAction));

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

    /// Executes the main operations based on the provided configuration and parameters.
    /// @param configuration The configuration settings required for execution.
    /// @param parameters A collection of parameters necessary for the operation.
    public override void DoExecute(TestContext context)
    {
        try
        {
            IMessage receivedMessage;
            var consumer = GetOrCreateEndpoint(context).CreateConsumer();

            var selector = MessageSelectorBuilder.Build(MessageSelector, MessageSelectorDictionary, context);

            if (!string.IsNullOrWhiteSpace(MessageSelector) && consumer is ISelectiveConsumer selectiveConsumer)
                receivedMessage = selectiveConsumer.Receive(selector, context, Timeout);
            else
                receivedMessage = consumer.Receive(context, Timeout);

            if (receivedMessage != null)
            {
                if (Log.IsDebugEnabled) Log.Debug("Received message:\n" + receivedMessage.Print(context));

                throw new AgenixSystemException("Message timeout validation failed! " +
                                              "Received message while waiting for timeout on destination");
            }
        }
        catch (ActionTimeoutException e)
        {
            Log.Info("No messages received on destination. Message timeout validation OK!");
            Log.Info(e.Message);
        }
    }

    /// Creates or gets the endpoint instance.
    /// @param context Test context containing the necessary configuration and endpoint factory.
    /// @return The created or retrieved endpoint instance.
    /// /
    public IEndpoint GetOrCreateEndpoint(TestContext context)
    {
        if (Endpoint != null) return Endpoint;

        if (!string.IsNullOrWhiteSpace(EndpointUri)) return context.EndpointFactory.Create(EndpointUri, context);

        throw new AgenixSystemException("Neither endpoint nor endpoint uri is set properly!");
    }

    /// The Builder class assists in the construction of ReceiveTimeoutAction instances using a fluent interface.
    /// /
    public sealed class Builder : AbstractTestActionBuilder<ITestAction, dynamic>
    {
        internal long _timeout = 1000L;
        public IEndpoint MessageEndpoint { get; private set; }
        public string EndpointUri { get; private set; }
        public Dictionary<string, object> MessageSelectorDictionary { get; private set; } = new();
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

        public override ReceiveTimeoutAction Build()
        {
            return new ReceiveTimeoutAction(this);
        }
    }
}