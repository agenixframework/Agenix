using System;
using System.Collections.Generic;
using System.Linq;
using Agenix.Core.Endpoint;
using Agenix.Core.Exceptions;
using Agenix.Core.Message;
using Agenix.Core.Message.Builder;
using Agenix.Core.Messaging;
using Agenix.Core.Spi;
using Agenix.Core.Util;
using Agenix.Core.Validation;
using Agenix.Core.Validation.Context;
using Agenix.Core.Validation.Json;
using Agenix.Core.Variable;
using log4net;

namespace Agenix.Core.Actions;

/// <summary>
///     This action receives messages from a service destination. Action uses a Endpoint to receive the message, this means
///     that this action is independent of any message transport. The received message is validated using a
///     MessageValidator supporting expected control message payload and header templates.
/// </summary>
public class ReceiveMessageAction : AbstractTestAction
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(ReceiveMessageAction));

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
    }

    /**
     * Message endpoint
     */
    public IEndpoint Endpoint { get; }

    /**
     * Build message selector with name value pairs
     */
    public Dictionary<string, object> MessageSelectors { get; }

    /**
     * Select messages via message selector string
     */
    public string Selector { get; }

    /**
     * Message endpoint uri - either bean name or dynamic endpoint uri
     */
    public string EndpointUri { get; }

    /**
     * Receive timeout
     */
    public long ReceiveTimeout { get; }

    /**
     * Builder constructing a control message
     */
    public IMessageBuilder MessageBuilder { get; }

    /**
     * MessageValidator responsible for message validation
     */
    public List<IMessageValidator<IValidationContext>> Validators { get; }

    /**
     * Callback able to additionally validate received message
     */
    public IValidationProcessor Processor { get; }

    /**
     * List of validation contexts for this receive action
     */
    public List<IValidationContext> ValidationContexts { get; }

    /**
     * List of variable extractors responsible for creating variables from received message content
     */
    public List<IVariableExtractor> VariableExtractors { get; }

    /**
     * List of processors that handle the received message
     */
    public List<IMessageProcessor> Processors { get; }

    /**
     * List of processors that handle the control message builder
     */
    public List<IMessageProcessor> ControlMessageProcessors { get; }

    /**
     * The expected message type to arrive in this receive action - this information is needed to find a proper
     * message validator for this message
     */
    public string MessageType { get; }

    public override void DoExecute(TestContext context)
    {
        IMessage receivedMessage;
        var selector = MessageSelectorBuilder.Build(Selector, MessageSelectors, context);

        // Receive message either selected or plain with message receiver
        if (!string.IsNullOrEmpty(selector))
            receivedMessage = ReceiveSelected(context, selector);
        else
            receivedMessage = Receive(context);

        if (receivedMessage == null)
            throw new CoreSystemException("Failed to receive message - message is not available");

        // Validate the message
        ValidateMessage(receivedMessage, context);
    }

    /// <summary>
    ///     Receives the message with respective message receiver implementation.
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
        if (Log.IsDebugEnabled) Log.Debug($"Setting message selector: '{selectorString}'");

        var messageEndpoint = GetOrCreateEndpoint(context);
        var consumer = messageEndpoint.CreateConsumer();

        if (consumer is ISelectiveConsumer selectiveConsumer)
            return selectiveConsumer.Receive(context.ReplaceDynamicContentInString(selectorString), context,
                ReceiveTimeout > 0 ? ReceiveTimeout : messageEndpoint.EndpointConfiguration.Timeout);

        Log.Warn($"Unable to receive selectively with consumer implementation: '{consumer.GetType()}'");
        return Receive(context);
    }

    /// <summary>
    ///     Override this message if you want to add additional message validation
    /// </summary>
    /// <param name="message"></param>
    /// <param name="context"></param>
    protected void ValidateMessage(IMessage message, TestContext context)
    {
        foreach (var processor in Processors) processor.Process(message, context);

        if (Log.IsDebugEnabled) Log.Debug($"Received message:\n{message.Print(context)}");

        // Extract variables from received message content
        foreach (var variableExtractor in VariableExtractors) variableExtractor.ExtractVariables(message, context);

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
            if (Log.IsDebugEnabled) Log.Debug($"Control message:\n{controlMessage.Print(context)}");

            if (Validators.Count != 0)
            {
                foreach (var messageValidator in Validators)
                    messageValidator.ValidateMessage(message, controlMessage, context, ValidationContexts);

                if (Validators.All(v => v.GetType() != typeof(DefaultMessageHeaderValidator)))
                {
                    var defaultMessageHeaderValidator =
                        context.MessageValidatorRegistry.GetDefaultMessageHeaderValidator();
                    if (defaultMessageHeaderValidator != null)
                        defaultMessageHeaderValidator.ValidateMessage(message, controlMessage, context,
                            ValidationContexts);
                }
            }
            else
            {
                var mustFindValidator = ValidationContexts.Any(ctx =>
                        ctx is JsonPathMessageValidationContext
                    // TODO: XpathMessageValidationContext
                    //ctx is XpathMessageValidationContext ||
                    //ctx is ScriptValidationContext
                );

                var validators =
                    context.MessageValidatorRegistry.FindMessageValidators(MessageType, message, mustFindValidator);

                foreach (var messageValidator in validators)
                    messageValidator.ValidateMessage(message, controlMessage, context, ValidationContexts);
            }
        }
    }

    /// Creates a control message based on the provided test context and message type.
    /// <param name="context">The test context containing necessary configurations and message processors.</param>
    /// <param name="messageType">The type of the message to be created.</param>
    /// <return>The created control message after processing.</return>
    protected IMessage CreateControlMessage(TestContext context, string messageType)
    {
        var message = MessageBuilder.Build(context, messageType);

        if (message.Payload == null) return message;
        foreach (var processor in context.GetMessageProcessors(MessageDirection.INBOUND))
            processor.Process(message, context);


        foreach (var processor in ControlMessageProcessors) processor.Process(message, context);

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
    /// <exception cref="CoreSystemException">Thrown when neither the endpoint nor the endpoint URI is set properly.</exception>
    public IEndpoint GetOrCreateEndpoint(TestContext context)
    {
        if (Endpoint != null) return Endpoint;

        if (!string.IsNullOrWhiteSpace(EndpointUri)) return context.EndpointFactory.Create(EndpointUri, context);

        throw new CoreSystemException("Neither endpoint nor endpoint uri is set properly!");
    }

    public sealed class Builder : ReceiveMessageActionBuilder<ReceiveMessageAction, ReceiveMessageActionBuilderSupport,
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
            if (messageBuilderSupport == null) messageBuilderSupport = new ReceiveMessageActionBuilderSupport(self);
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
        internal readonly Dictionary<string, object> _messageSelectors = new();

        internal readonly List<string> _validatorNames = [];

        internal readonly List<IMessageValidator<IValidationContext>> _validators = [];

        internal HeaderValidationContext _headerValidationContext;

        internal string _messageSelector;

        internal long _receiveTimeout;

        internal IValidationProcessor _validationProcessor;

        public List<IValidationContext.IBuilder<IValidationContext, dynamic>> ValidationContexts { get; } = [];

        public override T Build()
        {
            messageBuilderSupport ??= GetMessageBuilderSupport();

            ReconcileValidationContexts();

            if (referenceResolver != null)
            {
                if (_validationProcessor is IReferenceResolverAware referenceResolverAwareProcessor)
                    referenceResolverAwareProcessor.SetReferenceResolver(referenceResolver);

                while (_validatorNames.Count > 0)
                {
                    var validatorName = _validatorNames[0];
                    _validatorNames.RemoveAt(0);

                    var validator = referenceResolver.Resolve(validatorName);
                    if (validator is IHeaderValidator headerValidator)
                        GetHeaderValidationContext().AddHeaderValidator(headerValidator);
                    else
                        _validators.Add((IMessageValidator<IValidationContext>)validator);
                }
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
        ///     Adds a validation context to this message receiving action.
        /// </summary>
        /// <param name="validationContext">The validation context to add.</param>
        /// <typeparam name="B">The type of the validation context builder.</typeparam>
        /// <returns>The current builder instance.</returns>
        public TB Validate(IValidationContext.IBuilder<IValidationContext, dynamic> validationContext)
        {
            ValidationContexts.Add(validationContext);
            return self;
        }

        /// <summary>
        ///     Adds a validation context to the message receiving action using the provided validation context.
        /// </summary>
        /// <typeparam name="TFB">The type of the builder for the validation context.</typeparam>
        /// <param name="validationContext">An instance of the validation context.</param>
        /// <returns>The builder instance with the added validation context.</returns>
        public TB Validate(IValidationContext validationContext)
        {
            return Validate(new FuncValidationContextBuilder<dynamic>(() => validationContext));
        }

        /// <summary>
        ///     Validates the message action using the provided validation context factory.
        /// </summary>
        /// <typeparam name="TFB">The type of the validation context builder.</typeparam>
        /// <param name="validationContextFactory">A factory function to create the validation context.</param>
        /// <returns>An instance of the action builder for chaining further configurations.</returns>
        public TB Validate(Func<IValidationContext> validationContextFactory)
        {
            var validationContextBuilder = new FuncValidationContextBuilder<dynamic>(validationContextFactory);
            return Validate(validationContextBuilder);
        }

        /// <summary>
        ///     Adds a validation context to the message receiving action using an adapter that implements
        ///     <see cref="IValidationContextAdapter" />.
        /// </summary>
        /// <param name="adapter">An instance of the adapter that can be adapted to a validation context.</param>
        /// <returns>The builder instance with the added validation context.</returns>
        public TB Validate(IValidationContextAdapter adapter)
        {
            return Validate(adapter.AsValidationContext());
        }

        /// <summary>
        ///     Adds a validation context to this message receiving action.
        /// </summary>
        /// <typeparam name="TFB">The type of the validation context builder.</typeparam>
        /// <param name="validationContexts">The list of validation contexts to be added.</param>
        /// <returns>The builder instance.</returns>
        public TB Validate(List<IValidationContext.IBuilder<IValidationContext, dynamic>> validationContexts)
        {
            ValidationContexts.AddRange(validationContexts);
            return self;
        }

        /// <summary>
        ///     Adds one or more validation contexts to this message receiving action.
        /// </summary>
        /// <typeparam name="TFB">The type of the context builder.</typeparam>
        /// <param name="validationContexts">The validation contexts to be added.</param>
        /// <returns>Updated message action builder.</returns>
        public TB Validate(params IValidationContext.IBuilder<IValidationContext, dynamic>[] validationContexts)
        {
            return Validate(validationContexts.ToList());
        }

        /// <summary>
        ///     Specifies a message selector to be used during the message receiving action.
        /// </summary>
        /// <param name="messageSelector">The message selector string.</param>
        /// <returns>The builder instance.</returns>
        public TB Selector(string messageSelector)
        {
            _messageSelector = messageSelector;
            return self;
        }

        /// <summary>
        ///     Adds message selectors to the current message receiving action.
        /// </summary>
        /// <param name="messageSelector">A dictionary containing message selectors.</param>
        /// <returns>The current instance of the builder.</returns>
        public TB Selector(IDictionary<string, string> messageSelector)
        {
            foreach (var kvp in messageSelector) _messageSelectors[kvp.Key] = kvp.Value;
            return self;
        }

        /// <summary>
        ///     Adds a custom validator to this message receiving action.
        /// </summary>
        /// <param name="validator">An instance of IMessageValidator to be added.</param>
        /// <returns>Returns an instance of the action builder for method chaining.</returns>
        public TB Validator(IMessageValidator<IValidationContext> validator)
        {
            _validators.Add(validator);
            return self;
        }

        /// <summary>
        ///     Specifies one or more custom validators for the message receiving action.
        /// </summary>
        /// <param name="validators">A list of validator names to be added.</param>
        /// <returns>The updated action builder with the specified validators.</returns>
        public TB Validators(params string[] validators)
        {
            foreach (var validator in validators) Validator(validator);
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
        ///     Adds a collection of message validators to this message receiving action.
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
            foreach (var validator in validators) GetHeaderValidationContext().AddHeaderValidator(validator);
            return self;
        }


        /// <summary>
        ///     Retrieves or creates the header validation context and adds it to the list of validation contexts.
        /// </summary>
        /// <returns>The header validation context.</returns>
        public HeaderValidationContext GetHeaderValidationContext()
        {
            if (_headerValidationContext != null) return _headerValidationContext;
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
        ///     Revisit configured validation context list and automatically add context based on message payload and path
        ///     expression contexts if any. This method makes sure that validation contexts are configured. If no validation
        ///     context has been set yet the method automatically adds proper validation contexts for Json and XML message
        ///     payloads. In case a path expression (JsonPath, XPath) context is set but no proper message validation context
        ///     (Json, Xml) the method automatically adds the proper message validation context. Only when validation contexts are
        ///     set properly according to the message type and content the message validation steps will execute later on.
        /// </summary>
        protected void ReconcileValidationContexts()
        {
            var validationContexts = GetValidationContexts();

            if (!validationContexts.Any(c => c is HeaderValidationContext)) GetHeaderValidationContext();

            if (validationContexts.All(c => c is HeaderValidationContext))
            {
                // TODO: XmlMessageValidationContext
                //Validate(new XmlMessageValidationContext());
                Validate(new JsonMessageValidationContext());
            }
            else if (validationContexts.Any(c => c is JsonPathMessageValidationContext) &&
                     !validationContexts.Any(c => c is JsonMessageValidationContext))
            {
                Validate(new JsonMessageValidationContext());
            }
            else if ( /*!validationContexts.Any(c => c is XmlMessageValidationContext) &&*/
                     !validationContexts.Any(c => c is JsonMessageValidationContext))
            {
                // if still no Json or Xml message validation context is set, check the message payload and set the proper context
                var payload = GetMessagePayload();
                if (payload.IsPresent)
                {
                    if (MessagePayloadUtils.IsXml(payload.Value))
                    {
                        // TODO: XmlMessageValidationContext
                        //Validate(new XmlMessageValidationContext());
                    }
                    else if (MessagePayloadUtils.IsJson(payload.Value))
                    {
                        Validate(new JsonMessageValidationContext());
                    }
                }
            }

            validationContexts
                .OfType<HeaderValidationContext>()
                .ToList()
                .ForEach(c => c.HeaderNameIgnoreCase = GetMessageBuilderSupport().HeaderIgnoreCase);
        }

        /// <summary>
        ///     Gets message payload String representation from configured message builder.
        /// </summary>
        /// <returns></returns>
        protected Optional<string> GetMessagePayload()
        {
            if (messageBuilderSupport == null) return Optional<string>.Empty;

            var messageBuilder = messageBuilderSupport.GetMessageBuilder();

            switch (messageBuilder)
            {
                case StaticMessageBuilder staticMessageBuilder:
                {
                    var message = staticMessageBuilder.GetMessage();
                    if (message.Payload is string) return Optional<string>.OfNullable(message.GetPayload<string>());

                    break;
                }
                case IWithPayloadBuilder withPayloadBuilder:
                {
                    var payloadBuilder = withPayloadBuilder.GetPayloadBuilder();
                    if (payloadBuilder is DefaultPayloadBuilder defaultPayloadBuilder)
                        return Optional<string>.OfNullable(defaultPayloadBuilder.GetPayload()?.ToString());

                    break;
                }
            }

            return Optional<string>.Empty;
        }
    }
}