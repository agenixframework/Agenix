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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Endpoint;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Api.Variable;
using Agenix.Api.Variable.Dictionary;
using Agenix.Core.Message;
using Agenix.Core.Message.Builder;
using Agenix.Core.Util;
using Agenix.Core.Validation;
using Agenix.Core.Validation.Json;
using Agenix.Core.Validation.Xml;
using Microsoft.Extensions.Logging;
using MsgType = Agenix.Api.Message.MessageType;

namespace Agenix.Core.Actions;

/// <summary>
///     This action receives messages from a service destination. Action uses a Endpoint to receive the message; this means
///     that this action is independent of any message transport. The received message is validated using a
///     MessageValidator supporting expected control message payload and header templates.
/// </summary>
public class ReceiveMessageAction : AbstractTestAction
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(ReceiveMessageAction));

    public ReceiveMessageAction(
        ReceiveMessageActionBuilder<ReceiveMessageAction, ReceiveMessageActionBuilderSupport, Builder> builder)
        : base(builder.GetName() ?? "receive", builder.GetDescription() ?? "")
    {
        Endpoint = builder.GetEndpoint();
        EndpointUri = builder.GetEndpointUri();
        ReceiveTimeout = builder._receiveTimeout;
        Selector = builder._messageSelector;
        MessageSelectors = builder._messageSelectors;
        Validators = builder._validators;
        Processor = builder._validationProcessor;
        ValidationContexts = builder.GetValidationContexts();
        VariableExtractors = builder.GetVariableExtractors();
        Processors = builder.GetMessageProcessors();
        MessageBuilder = builder.GetMessageBuilderSupport().GetMessageBuilder();
        ControlMessageProcessors = builder.GetMessageBuilderSupport().ControlMessageProcessors;
        MessageType = builder.GetMessageBuilderSupport().GetMessageType();
        DataDictionary = builder.GetMessageBuilderSupport().DataDictionary;
    }

    /// <summary>
    ///     Represents the communication endpoint associated with the action.
    /// </summary>
    public IEndpoint Endpoint { get; }

    /// <summary>
    ///     Represents a collection of message selectors used to filter and retrieve messages based on specific criteria.
    /// </summary>
    public Dictionary<string, object> MessageSelectors { get; }

    public IDataDictionary DataDictionary { get; }

    /// <summary>
    ///     Gets the message selector used to filter messages based on specific criteria.
    /// </summary>
    public string Selector { get; }

    /// <summary>
    ///     Specifies the URI of the endpoint associated with the action.
    /// </summary>
    public string EndpointUri { get; }

    /// <summary>
    ///     Specifies the maximum duration, in milliseconds, to wait for a message to be received.
    /// </summary>
    public long ReceiveTimeout { get; }

    /// <summary>
    ///     Provides functionality for constructing and managing message-related operations.
    /// </summary>
    public IMessageBuilder MessageBuilder { get; }

    /// <summary>
    ///     A collection of validators for validating messages within a given validation context.
    /// </summary>
    public List<IMessageValidator<IValidationContext>> Validators { get; }

    /// <summary>
    ///     Validation processor used for processing and validating messages in the action.
    /// </summary>
    public IValidationProcessor Processor { get; }

    /// <summary>
    ///     A collection of validation contexts used during the validation process.
    /// </summary>
    public List<IValidationContext> ValidationContexts { get; }

    /// <summary>
    ///     A collection of variable extractors used to extract and store variables from a received message into the test
    ///     context.
    /// </summary>
    public List<IVariableExtractor> VariableExtractors { get; }

    /// <summary>
    ///     A collection of message processors used to perform custom processing operations
    ///     on received messages during the action's execution.
    /// </summary>
    public List<IMessageProcessor> Processors { get; }

    /// <summary>
    ///     A collection of message processors specifically intended for handling control messages.
    /// </summary>
    public List<IMessageProcessor> ControlMessageProcessors { get; }

    /// <summary>
    ///     Represents the type of the message received in the action.
    /// </summary>
    public string MessageType { get; set; }

    /// <summary>
    ///     Sets the type of the message to the specified message type.
    /// </summary>
    /// <param name="messageType">
    ///     The message type to be set, represented by an instance of the <see cref="MessageType" /> enumeration.
    /// </param>
    private void SetMessageType(MessageType messageType)
    {
        MessageType = messageType.ToString();
    }

    /// <summary>
    ///     Executes the main logic for this test action, including receiving a message and validating it.
    /// </summary>
    /// <param name="context">
    ///     The current test execution context which provides the environment and necessary data for the operation.
    /// </param>
    public override void DoExecute(TestContext context)
    {
        var selector = MessageSelectorBuilder.Build(Selector, MessageSelectors, context);

        // Receive a message either selected or plain with the message receiver
        var receivedMessage = !string.IsNullOrEmpty(selector) ? ReceiveSelected(context, selector) : Receive(context);

        if (receivedMessage == null)
        {
            throw new AgenixSystemException("Failed to receive message - message is not available");
        }

        // Validate the message
        ValidateMessage(receivedMessage, context);
    }

    /// <summary>
    ///     Receives the message with the respective message receiver implementation.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private IMessage Receive(TestContext context)
    {
        var messageEndpoint = GetOrCreateEndpoint(context);
        var consumer = messageEndpoint.CreateConsumer();

        return consumer.Receive(context,
            ReceiveTimeout > 0 ? ReceiveTimeout : messageEndpoint.EndpointConfiguration.Timeout);
    }

    /// <summary>
    ///     Receives the message with the respective message receiver implementation also using a message selector.
    /// </summary>
    /// <param name="context">the test context</param>
    /// <param name="selectorString">the message selector string</param>
    /// <returns></returns>
    private IMessage ReceiveSelected(TestContext context, string selectorString)
    {
        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug("Setting message selector: '{SelectorString}'", selectorString);
        }

        var messageEndpoint = GetOrCreateEndpoint(context);
        var consumer = messageEndpoint.CreateConsumer();

        if (consumer is ISelectiveConsumer selectiveConsumer)
        {
            return selectiveConsumer.Receive(context.ReplaceDynamicContentInString(selectorString), context,
                ReceiveTimeout > 0 ? ReceiveTimeout : messageEndpoint.EndpointConfiguration.Timeout);
        }

        Log.LogWarning("Unable to receive selectively with consumer implementation: '{Type}'", consumer.GetType());
        return Receive(context);
    }

    /// <summary>
    ///     Override this message if you want to add additional message validation
    /// </summary>
    /// <param name="message"></param>
    /// <param name="context"></param>
    protected void ValidateMessage(IMessage message, TestContext context)
    {
        foreach (var processor in Processors)
        {
            processor.Process(message, context);
        }

        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug("Received message:\n{Print}", message.Print(context));
        }

        // Extract variables from received message content
        foreach (var variableExtractor in VariableExtractors)
        {
            variableExtractor.ExtractVariables(message, context);
        }

        var controlMessage = CreateControlMessage(context, MessageType);
        context.MessageStore.StoreMessage(
            !string.IsNullOrEmpty(controlMessage.Name)
                ? controlMessage.Name
                : context.MessageStore.ConstructMessageName(this, GetOrCreateEndpoint(context)), message);

        if (Processor != null)
        {
            Processor.Validate(message, context);
        }
        else
        {
            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug("Control message:\n{Print}", controlMessage.Print(context));
            }

            AssumeMessageType(StringUtils.HasText(controlMessage.GetPayload<string>()) ? controlMessage : message);

            if (Validators is { Count: > 0 })
            {
                foreach (var messageValidator in Validators)
                {
                    messageValidator.ValidateMessage(message, controlMessage, context, ValidationContexts);
                }

                if (Validators.AsParallel()
                    .Select(v => v.GetType())
                    .Any(type => typeof(DefaultMessageHeaderValidator).IsAssignableFrom(type)))
                {
                    return;
                }

                var defaultMessageHeaderValidator = context.MessageValidatorRegistry.GetDefaultMessageHeaderValidator();
                if (defaultMessageHeaderValidator != null)
                {
                    defaultMessageHeaderValidator.ValidateMessage(message, controlMessage, context, ValidationContexts);
                }
            }
            else
            {
                var mustFindValidator = ValidationContexts.Any(ctx => ctx.RequiresValidator);

                var activeValidators =
                    context.MessageValidatorRegistry.FindMessageValidators(MessageType, message, mustFindValidator);

                foreach (var messageValidator in activeValidators)
                {
                    messageValidator.ValidateMessage(message, controlMessage, context, ValidationContexts);
                }

                if (AgenixSettings.IsPerformDefaultValidation() &&
                    ValidationContexts.Any(validationContext => validationContext.Status == ValidationStatus.UNKNOWN))
                {
                    var defaultValidator = context.MessageValidatorRegistry.GetDefaultMessageValidator();
                    if (!activeValidators.Any(validator => defaultValidator.GetType().IsInstanceOfType(validator)))
                    {
                        defaultValidator.ValidateMessage(message, controlMessage, context, ValidationContexts);
                    }
                }
            }

            var unknown = ValidationContexts
                .Where(validationContext => validationContext.Status == ValidationStatus.UNKNOWN)
                .ToList();
            if (unknown.Count <= 0)
            {
                return;
            }

            {
                foreach (var validationContext in unknown)
                {
                    Log.LogWarning(
                        "Found validation context that has not been processed: {S}", validationContext.GetType().Name);
                }

                throw new ValidationException(
                    $"Incomplete message validation - total of {unknown.Count} validation context has not been processed");
            }
        }
    }

    /// <summary>
    ///     Analyzes the message payload and infers the message type based on its content if the message type is not provided
    ///     or mismatched.
    ///     Adjusts the message type to match the detected payload type such as XML or JSON.
    /// </summary>
    /// <param name="message">
    ///     The message instance that contains the payload to be examined, implementing the <see cref="IMessage" /> interface.
    /// </param>
    private void AssumeMessageType(IMessage message)
    {
        if (MessageTypeExtensions.IsBinary(MessageType) ||
            MessageTypeExtensions.FormUrlEncoded.Equals(MessageType, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var payload = message.GetPayload<string>();
        if (string.IsNullOrEmpty(payload))
        {
            return;
        }

        payload = payload.Trim();

        if (MessagePayloadUtils.IsXml(payload)
            && (string.IsNullOrEmpty(MessageType) || !MessageTypeExtensions.IsXml(MessageType)))
        {
            Log.LogWarning(
                "Detected XML message payload type, but non-XML message type '{S}' configured! Assuming message type {Xml}",
                MessageType, MsgType.XML);

            SetMessageType(MsgType.XML);
        }
        else if (MessagePayloadUtils.IsJson(payload)
                 && (string.IsNullOrEmpty(MessageType) || !MessageType.Equals(nameof(MsgType.JSON),
                     StringComparison.OrdinalIgnoreCase)))
        {
            Log.LogWarning(
                "Detected JSON message payload type, but non-JSON message type '{S}' configured! Assuming message type {Json}",
                MessageType, MsgType.JSON);

            SetMessageType(MsgType.JSON);
        }
    }

    /// Creates a control message based on the provided test context and message type.
    /// <param name="context">The test context containing necessary configurations and message processors.</param>
    /// <param name="messageType">The type of the message to be created.</param>
    /// <return>The created control message after processing.</return>
    protected IMessage CreateControlMessage(TestContext context, string messageType)
    {
        var message = MessageBuilder.Build(context, messageType);

        if (message.Payload == null)
        {
            return message;
        }

        foreach (var processor in context.GetMessageProcessors(MessageDirection.INBOUND))
        {
            processor.Process(message, context);
        }

        DataDictionary?.Process(message, context);

        foreach (var processor in ControlMessageProcessors)
        {
            processor.Process(message, context);
        }

        return message;
    }

    /// Determines whether the action is disabled based on the given test context.
    /// <param name="context">The test context containing necessary configurations and factories.</param>
    /// <return>True if the action is disabled; otherwise, false.</return>
    public override bool IsDisabled(TestContext context)
    {
        var messageEndpoint = GetOrCreateEndpoint(context);

        return base.IsDisabled(context);
    }

    /// Retrieves the existing endpoint or creates a new one based on the endpoint URI.
    /// <param name="context">The test context containing necessary configurations and factories.</param>
    /// <return>The existing or newly created endpoint.</return>
    /// <exception cref="AgenixSystemException">Thrown when neither the endpoint nor the endpoint URI is set properly.</exception>
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

    public class Builder : ReceiveMessageActionBuilder<ReceiveMessageAction, ReceiveMessageActionBuilderSupport,
        Builder>
    {
        /// Fluent API action building entry method used C# DSL.
        /// @return
        /// /
        public static Builder Receive()
        {
            return new Builder();
        }

        /// Fluent API action building entry method for creating a builder with a specified message endpoint.
        /// <param name="messageEndpoint">The message endpoint to be used.</param>
        /// <return>A new instance of the Builder class configured with the specified message endpoint.</return>
        public static Builder Receive(IEndpoint messageEndpoint)
        {
            var builder = new Builder();
            builder.Endpoint(messageEndpoint);
            return builder;
        }

        /// Fluent API action building entry method used C# DSL.
        /// <return>A new instance of the Builder class configured for receiving a message.</return>
        public static Builder Receive(string messageEndpointUri)
        {
            var builder = new Builder();
            builder.Endpoint(messageEndpointUri);
            return builder;
        }

        /// Retrieves the message builder support instance, initializing it if it doesn't already exist.
        /// <return>Returns the instance of ReceiveMessageActionBuilderSupport</return>
        public override ReceiveMessageActionBuilderSupport GetMessageBuilderSupport()
        {
            if (messageBuilderSupport == null)
            {
                messageBuilderSupport = new ReceiveMessageActionBuilderSupport(self);
            }

            return base.GetMessageBuilderSupport();
        }

        /// Builds an instance of the ReceiveMessageAction using the provided builder configurations.
        /// <return>A newly created ReceiveMessageAction instance.</return>
        protected override ReceiveMessageAction DoBuild()
        {
            return new ReceiveMessageAction(this);
        }
    }

    /// <summary>
    ///     Provides builder support functionalities for constructing <see cref="ReceiveMessageAction" /> instances.
    /// </summary>
    /// <remarks>
    ///     This class facilitates the setup and customization of receive message actions
    ///     including configuring message processors, header case sensitivity, and validation contexts.
    /// </remarks>
    public class ReceiveMessageActionBuilderSupport(Builder dlg)
        : ReceiveMessageBuilderSupport<ReceiveMessageAction, Builder, ReceiveMessageActionBuilderSupport>(dlg)
    {
    }

    /// <summary>
    ///     Builder for constructing <see cref="ReceiveMessageAction" /> instances. This builder provides methods to set
    ///     various configurations such as validators, selectors, and timeouts for receiving messages. It is designed to
    ///     support
    ///     a fluent interface, enabling method chaining to configure and build the action.
    /// </summary>
    public abstract class ReceiveMessageActionBuilder<T, TM, TB> : MessageActionBuilder<T, TM, TB>
        where T : ReceiveMessageAction
        where TM : ReceiveMessageBuilderSupport<T, TB, TM>
        where TB : ReceiveMessageActionBuilder<T, TM, TB>
    {
        protected internal readonly Dictionary<string, object> _messageSelectors = new();

        protected readonly List<string> _validatorNames = [];

        protected internal readonly List<IMessageValidator<IValidationContext>> _validators = [];

        protected HeaderValidationContext _headerValidationContext;

        protected internal string _messageSelector;
        protected internal long _receiveTimeout;

        protected internal IValidationProcessor _validationProcessor;

        public List<IValidationContext.IBuilder<IValidationContext, IBuilder>> ValidationContexts { get; } = [];

        public override T Build()
        {
            messageBuilderSupport ??= GetMessageBuilderSupport();

            ReconcileValidationContexts();

            if (referenceResolver == null)
            {
                return DoBuild();
            }

            if (_validationProcessor is IReferenceResolverAware referenceResolverAwareProcessor)
            {
                referenceResolverAwareProcessor.SetReferenceResolver(referenceResolver);
            }

            while (_validatorNames.Count > 0)
            {
                var validatorName = _validatorNames[0];
                _validatorNames.RemoveAt(0);

                var validator = referenceResolver.Resolve(validatorName);
                if (validator is IHeaderValidator headerValidator)
                {
                    GetHeaderValidationContext().AddHeaderValidator(headerValidator);
                }
                else
                {
                    _validators.Add((IMessageValidator<IValidationContext>)validator);
                }
            }

            if (messageBuilderSupport.DataDictionaryName != null)
            {
                messageBuilderSupport.Dictionary(
                    referenceResolver.Resolve<IDataDictionary>(messageBuilderSupport.DataDictionaryName));
            }

            return DoBuild();
        }

        /// <summary>
        ///     Adds a custom timeout to this message receiving action.
        /// </summary>
        /// <param name="receiveTimeout"></param>
        /// <returns></returns>
        public TB Timeout(long receiveTimeout)
        {
            _receiveTimeout = receiveTimeout;
            return self;
        }

        /// <summary>
        ///     Adds a validation context to this message-receiving action.
        /// </summary>
        /// <param name="validationContext">The validation context to add.</param>
        /// <typeparam name="B">The type of the validation context builder.</typeparam>
        /// <returns>The current builder instance.</returns>
        public TB Validate(IValidationContext.IBuilder<IValidationContext, IBuilder> validationContext)
        {
            ValidationContexts.Add(validationContext);
            return self;
        }

        /// <summary>
        ///     Adds a validation context to the message-receiving action using the provided validation context.
        /// </summary>
        /// <typeparam name="TFB">The type of the builder for the validation context.</typeparam>
        /// <param name="validationContext">An instance of the validation context.</param>
        /// <returns>The builder instance with the added validation context.</returns>
        public TB Validate(IValidationContext validationContext)
        {
            return Validate(new FuncValidationContextBuilder<IBuilder>(() => validationContext));
        }

        /// <summary>
        ///     Validates the message action using the provided validation context factory.
        /// </summary>
        /// <typeparam name="TFB">The type of the validation context builder.</typeparam>
        /// <param name="validationContextFactory">A factory function to create the validation context.</param>
        /// <returns>An instance of the action builder for chaining further configurations.</returns>
        public TB Validate(Func<IValidationContext> validationContextFactory)
        {
            var validationContextBuilder = new FuncValidationContextBuilder<IBuilder>(validationContextFactory);
            return Validate(validationContextBuilder);
        }

        /// <summary>
        ///     Adds a validation context to the message-receiving action using an adapter that implements
        ///     <see cref="IValidationContextAdapter" />.
        /// </summary>
        /// <param name="adapter">An instance of the adapter that can be adapted to a validation context.</param>
        /// <returns>The builder instance with the added validation context.</returns>
        public TB Validate(IValidationContextAdapter adapter)
        {
            return Validate(adapter.AsValidationContext());
        }

        /// <summary>
        ///     Adds a validation context to this message-receiving action.
        /// </summary>
        /// <typeparam name="TFB">The type of the validation context builder.</typeparam>
        /// <param name="validationContexts">The list of validation contexts to be added.</param>
        /// <returns>The builder instance.</returns>
        public TB Validate(List<IValidationContext.IBuilder<IValidationContext, IBuilder>> validationContexts)
        {
            ValidationContexts.AddRange(validationContexts);
            return self;
        }

        /// <summary>
        ///     Adds one or more validation contexts to this message-receiving action.
        /// </summary>
        /// <typeparam name="TFB">The type of the context builder.</typeparam>
        /// <param name="validationContexts">The validation contexts to be added.</param>
        /// <returns>Updated message action builder.</returns>
        public TB Validate(params IValidationContext.IBuilder<IValidationContext, IBuilder>[] validationContexts)
        {
            return Validate(validationContexts.ToList());
        }

        /// <summary>
        ///     Specifies a message selector to be used during the message-receiving action.
        /// </summary>
        /// <param name="messageSelector">The message selector string.</param>
        /// <returns>The builder instance.</returns>
        public TB Selector(string messageSelector)
        {
            _messageSelector = messageSelector;
            return self;
        }

        /// <summary>
        ///     Adds message selectors to the current message-receiving action.
        /// </summary>
        /// <param name="messageSelector">A dictionary containing message selectors.</param>
        /// <returns>The current instance of the builder.</returns>
        public TB Selector(IDictionary<string, object> messageSelector)
        {
            foreach (var kvp in messageSelector)
            {
                _messageSelectors[kvp.Key] = kvp.Value;
            }

            return self;
        }

        /// <summary>
        ///     Adds a custom validator to this message-receiving action.
        /// </summary>
        /// <param name="validator">An instance of IMessageValidator to be added.</param>
        /// <returns>Returns an instance of the action builder for method chaining.</returns>
        public TB Validator(IMessageValidator<IValidationContext> validator)
        {
            _validators.Add(validator);
            return self;
        }

        /// <summary>
        ///     Specifies one or more custom validators for the message-receiving action.
        /// </summary>
        /// <param name="validators">A list of validator names to be added.</param>
        /// <returns>The updated action builder with the specified validators.</returns>
        public TB Validators(params string[] validators)
        {
            foreach (var validator in validators)
            {
                Validator(validator);
            }

            return self;
        }

        /// <summary>
        ///     Adds a validator to the message receiving action.
        /// </summary>
        /// <param name="validatorName">The name of the validator to add.</param>
        /// <returns>The builder instance for chaining further configurations.</returns>
        public TB Validator(string validatorName)
        {
            _validatorNames.Add(validatorName);
            return self;
        }

        /// <summary>
        ///     Adds a collection of message validators to this message-receiving action.
        /// </summary>
        /// <param name="validators">An array of validators to validate the received messages.</param>
        /// <returns>Returns the builder with the added validators.</returns>
        public TB Validators(params IMessageValidator<IValidationContext>[] validators)
        {
            return Validators(validators.ToList());
        }

        /// <summary>
        ///     Adds a list of message validators to the ReceiveMessageActionBuilder.
        /// </summary>
        /// <param name="validators">A list of IMessageValidator instances to add.</param>
        /// <returns>The builder instance with the specified validators added.</returns>
        public TB Validators(List<IMessageValidator<IValidationContext>> validators)
        {
            _validators.AddRange(validators);
            return self;
        }

        /// <summary>
        ///     Adds header validators to the message receiving action.
        /// </summary>
        /// <param name="validators">An array of header validators to be added.</param>
        /// <returns>The builder instance with the added validators.</returns>
        public TB Validator(params IHeaderValidator[] validators)
        {
            foreach (var validator in validators)
            {
                GetHeaderValidationContext().AddHeaderValidator(validator);
            }

            return self;
        }


        /// <summary>
        ///     Retrieves or creates the header validation context and adds it to the list of validation contexts.
        /// </summary>
        /// <returns>The header validation context.</returns>
        public HeaderValidationContext GetHeaderValidationContext()
        {
            if (_headerValidationContext != null)
            {
                return _headerValidationContext;
            }

            _headerValidationContext = new HeaderValidationContext();

            Validate(() => _headerValidationContext);

            return _headerValidationContext;
        }

        /// <summary>
        ///     Retrieves a list of validation contexts by building them from their respective builders.
        /// </summary>
        /// <returns>A list of <see cref="IValidationContext" /> objects.</returns>
        public List<IValidationContext> GetValidationContexts()
        {
            return ValidationContexts
                .Select(builder => builder.Build())
                .ToList();
        }

        public TB Validate(IValidationProcessor processor)
        {
            _validationProcessor = processor;
            return self;
        }

        public TB Validate(ValidationProcessor processor)
        {
            _validationProcessor = new DelegatingValidationProcessor(processor);
            return self;
        }

        /// <summary>
        ///     Processes the provided message processor and adds it to the appropriate list based on its type.
        /// </summary>
        /// <param name="processor">The <see cref="IMessageProcessor" /> to be processed.</param>
        /// <returns>The current builder instance.</returns>
        public override TB Process(IMessageProcessor processor)
        {
            switch (processor)
            {
                case IVariableExtractor variableExtractor:
                    variableExtractors.Add(variableExtractor);
                    break;
                case IValidationProcessor validationProcessor:
                    Validate(validationProcessor);
                    break;
                default:
                    messageProcessors.Add(processor);
                    break;
            }

            return self;
        }

        /// <summary>
        ///     Revisit configured a validation context list and automatically added context based on message payload and path
        ///     expression contexts, if any. This method makes sure that validation contexts are configured. If no validation
        ///     context has been set yet, the method automatically adds proper validation contexts for JSON and XML message
        ///     payloads. In case a path expression (JsonPath, XPath) context is set but no proper message validation context
        ///     (Json, XML) the method automatically adds the proper message validation context. Only when validation contexts are
        ///     set properly according to the message type and content, the message validation steps will execute later on.
        /// </summary>
        protected void ReconcileValidationContexts()
        {
            var validationContexts = GetValidationContexts();
            if (!validationContexts.Any(context => context is HeaderValidationContext))
            {
                GetHeaderValidationContext();
            }

            if (!validationContexts.Any(context => context is IMessageValidationContext))
            {
                InjectMessageValidationContext();
            }
        }

        /// <summary>
        ///     Determines and injects the appropriate message validation context based on the current message payload
        ///     or other available attributes in the message. If no validation context is explicitly defined,
        ///     it evaluates the message format (e.g., XML, JSON, or plaintext) or other metadata to set a default context.
        /// </summary>
        private void InjectMessageValidationContext()
        {
            // if still no JSON or XML message validation context is set, check the message payload and set the proper context
            IValidationContext validationContext = null;
            var payload = GetMessagePayload();
            if (payload.IsPresent)
            {
                if (MessagePayloadUtils.IsXml(payload.Value))
                {
                    validationContext = new XmlMessageValidationContext();
                }
                else if (MessagePayloadUtils.IsJson(payload.Value))
                {
                    validationContext = new JsonMessageValidationContext();
                }
                else
                {
                    validationContext = new DefaultMessageValidationContext();
                }
            }

            if (validationContext == null && messageBuilderSupport != null)
            {
                if (messageBuilderSupport.GetMessageBuilder().GetType().IsDefined(typeof(MessagePayloadAttribute)))
                {
                    var type = messageBuilderSupport.GetMessageBuilder().GetType()
                        .GetCustomAttribute<MessagePayloadAttribute>()!.Value;
                    validationContext = type switch
                    {
                        MsgType.XML or MsgType.XHTML => new XmlMessageValidationContext(),
                        MsgType.JSON => new JsonMessageValidationContext(),
                        MsgType.PLAINTEXT => new DefaultMessageValidationContext(),
                        _ => validationContext
                    };
                }
            }

            if (validationContext == null)
            {
                var resource = GetMessageResource();
                if (resource.IsPresent)
                {
                    var fileExt = FileUtils.GetFileExtension(resource.Value);
                    validationContext = fileExt switch
                    {
                        "xml" => new XmlMessageValidationContext(),
                        "json" => new JsonMessageValidationContext(),
                        _ => new DefaultMessageValidationContext()
                    };
                }
            }

            if (validationContext == null && messageBuilderSupport != null)
            {
                if (messageBuilderSupport.GetMessageBuilder() is StaticMessageBuilder)
                {
                    validationContext = new DefaultMessageValidationContext();
                }
                else if (messageBuilderSupport.GetMessageBuilder() is IWithPayloadBuilder payloadBuilder)
                {
                    if (payloadBuilder.GetPayloadBuilder() != null)
                    {
                        validationContext = new DefaultMessageValidationContext();
                    }
                }
            }

            if (validationContext != null)
            {
                Validate(validationContext);
            }
        }

        /// <summary>
        ///     Gets a message resource file path from a configured message builder.
        /// </summary>
        /// <returns>Optional containing the resource path if available</returns>
        protected Optional<string> GetMessageResource()
        {
            if (messageBuilderSupport?.GetMessageBuilder() is not IWithPayloadBuilder withPayloadBuilder)
            {
                return Optional<string>.Empty;
            }

            if (withPayloadBuilder.GetPayloadBuilder() is FileResourcePayloadBuilder filePayloadBuilder)
            {
                return Optional<string>.OfNullable(filePayloadBuilder.GetResourcePath());
            }

            return Optional<string>.Empty;
        }

        /// <summary>
        ///     Gets message payload String representation from a configured message builder.
        /// </summary>
        /// <returns></returns>
        protected virtual Optional<string> GetMessagePayload()
        {
            if (messageBuilderSupport == null)
            {
                return Optional<string>.Empty;
            }

            var messageBuilder = messageBuilderSupport.GetMessageBuilder();

            switch (messageBuilder)
            {
                case StaticMessageBuilder staticMessageBuilder:
                    {
                        var message = staticMessageBuilder.GetMessage();
                        if (message.Payload is string)
                        {
                            return Optional<string>.OfNullable(message.GetPayload<string>());
                        }

                        break;
                    }
                case IWithPayloadBuilder withPayloadBuilder:
                    {
                        var payloadBuilder = withPayloadBuilder.GetPayloadBuilder();
                        if (payloadBuilder is DefaultPayloadBuilder defaultPayloadBuilder)
                        {
                            return Optional<string>.OfNullable(defaultPayloadBuilder.Payload?.ToString());
                        }

                        break;
                    }
            }

            return Optional<string>.Empty;
        }
    }
}
