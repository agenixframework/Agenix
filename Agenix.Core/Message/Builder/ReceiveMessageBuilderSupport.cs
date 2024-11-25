using System.Collections.Generic;
using System.Linq;
using Agenix.Core.Actions;
using Agenix.Core.Validation;
using Agenix.Core.Validation.Context;
using Agenix.Core.Variable;

namespace Agenix.Core.Message.Builder;

/// <summary>
///     Provides support for building "ReceiveMessage" actions with various configuration options.
/// </summary>
/// <typeparam name="T">The type of the receive message action.</typeparam>
/// <typeparam name="TB">The type of the receive message action builder.</typeparam>
/// <typeparam name="TS">The type of the receive message builder support.</typeparam>
/// <typeparam name="TV">The type of the validation context builder.</typeparam>
public class ReceiveMessageBuilderSupport<T, TB, TS>(TB dlg) : MessageBuilderSupport<T, TB, TS>(dlg)
    where T : ReceiveMessageAction
    where TB : ReceiveMessageAction.ReceiveMessageActionBuilder<T, TS, TB>
    where TS : ReceiveMessageBuilderSupport<T, TB, TS>
{
    /// <summary>
    ///     Collection of message processors that handle control messages.
    ///     These processors are applied during the construction and processing
    ///     of incoming messages, providing custom logic for handling specific
    ///     aspects or modifications to the message before it reaches its final
    ///     destination.
    /// </summary>
    public List<IMessageProcessor> ControlMessageProcessors { get; } = [];

    /// <summary>
    ///     Indicates whether the header name comparison should ignore case sensitivity.
    ///     When set to true, header names are treated in a case-insensitive manner,
    ///     allowing greater flexibility in handling headers from various sources.
    /// </summary>
    public bool HeaderIgnoreCase { get; set; }

    /// <summary>
    ///     Sets a custom timeout for receiving messages.
    /// </summary>
    /// <param name="receiveTimeout">The timeout duration in milliseconds.</param>
    /// <returns>The current instance of the message builder support.</returns>
    public TS Timeout(long receiveTimeout)
    {
        _delegate.Timeout(receiveTimeout);
        return _self;
    }

    /// <summary>
    ///     Sets whether the message headers should be treated as case-insensitive.
    /// </summary>
    /// <param name="value">If true, header names will be treated case-insensitively.</param>
    /// <returns>The current instance of the message builder support.</returns>
    public TS HeaderNameIgnoreCase(bool value)
    {
        HeaderIgnoreCase = value;
        return _self;
    }

    /// <summary>
    ///     Validates the message action using the provided validation context.
    /// </summary>
    /// <param name="validationContext">The validation context to use for message action validation.</param>
    /// <returns>The current instance of the message builder support.</returns>
    public TS Validate(IValidationContext.IBuilder<IValidationContext, dynamic> validationContext)
    {
        _delegate.Validate(validationContext);
        return _self;
    }

    /// <summary>
    ///     Validates the message with the specified validation context.
    /// </summary>
    /// <param name="validationContext">The validation context to use for validation.</param>
    /// <returns>The current instance of the message builder support.</returns>
    public TS Validate(IValidationContext validationContext)
    {
        return Validate(new FuncValidationContextBuilder<dynamic>(() => validationContext));
    }

    /// <summary>
    ///     Validates the message using the specified validation context adapter.
    /// </summary>
    /// <param name="adapter">The adapter providing the validation context.</param>
    /// <returns>The current instance of the message builder support.</returns>
    public TS Validate(IValidationContextAdapter adapter)
    {
        return Validate(adapter.AsValidationContext());
    }

    /// <summary>
    ///     Validates the message using a list of validation context builders.
    /// </summary>
    /// <param name="validationContexts">List of validation context builders.</param>
    /// <returns>The current instance of the message builder support.</returns>
    public TS Validate(List<IValidationContext.IBuilder<IValidationContext, dynamic>> validationContexts)
    {
        _delegate.Validate(validationContexts);
        return _self;
    }

    /// <summary>
    ///     Validates the specified validation contexts.
    /// </summary>
    /// <param name="validationContexts">An array of validation context builders to be validated.</param>
    /// <returns>The current instance of the message builder support.</returns>
    public TS Validate(params IValidationContext.IBuilder<IValidationContext, dynamic>[] validationContexts)
    {
        return Validate(validationContexts.ToList());
    }

    /// <summary>
    ///     Specifies a message selector to be used for receiving messages.
    /// </summary>
    /// <param name="messageSelector">The message selector expression.</param>
    /// <returns>The current instance of the message builder support.</returns>
    public TS Selector(string messageSelector)
    {
        _delegate.Selector(messageSelector);
        return _self;
    }

    /// <summary>
    ///     Adds message selectors to the current message receiving action.
    /// </summary>
    /// <param name="messageSelector">A dictionary containing message selectors.</param>
    /// <returns>The current instance of the message builder support.</returns>
    public TS Selector(IDictionary<string, string> messageSelector)
    {
        _delegate.Selector(messageSelector);
        return _self;
    }

    /// <summary>
    ///     Sets a validator for the received message.
    /// </summary>
    /// <param name="validator">The message validator to be used for validating received messages.</param>
    /// <returns>The current instance of the message builder support.</returns>
    public TS Validator(IMessageValidator<IValidationContext> validator)
    {
        _delegate.Validator(validator);
        return _self;
    }

    /// <summary>
    ///     Adds multiple message validators to the builder.
    /// </summary>
    /// <param name="validators">An array of message validators to be added.</param>
    /// <returns>The current instance of the message builder support.</returns>
    public TS Validators(params IMessageValidator<IValidationContext>[] validators)
    {
        return Validators(validators.ToList());
    }

    /// <summary>
    ///     Adds a list of message validators to the message builder support.
    /// </summary>
    /// <param name="validators">A list of message validators to be added.</param>
    /// <returns>The current instance of the message builder support.</returns>
    public TS Validators(List<IMessageValidator<IValidationContext>> validators)
    {
        _delegate.Validators(validators);
        return _self;
    }

    /// <summary>
    ///     Specifies the validator to be used for the message receiving action.
    /// </summary>
    /// <param name="validatorName">The name of the validator to set.</param>
    /// <returns>The current instance of the message builder support for further configuration chaining.</returns>
    public TS Validator(string validatorName)
    {
        _delegate.Validator(validatorName);
        return _self;
    }

    /// <summary>
    ///     Applies a custom header validator to the message validation process.
    /// </summary>
    /// <param name="validator">The header validator to use during validation.</param>
    /// <returns>The current instance of the message builder support.</returns>
    public TS Validator(IHeaderValidator validator)
    {
        _delegate.Validator(validator);
        return _self;
    }

    /// <summary>
    ///     Validates the current message using the specified validation processor.
    /// </summary>
    /// <param name="processor">The validation processor to use for message validation.</param>
    /// <returns>The current instance of the message builder support.</returns>
    public TB Validate(IValidationProcessor processor)
    {
        return _delegate.Validate(processor);
    }

    /// <summary>
    ///     Adds validation logic based on the provided validation context builder.
    /// </summary>
    /// <param name="processor">the delegating processing</param>
    /// <returns>The current instance of ReceiveMessageBuilderSupport.</returns>
    public TB Validate(ValidationProcessor processor)
    {
        return _delegate.Validate(processor);
    }

    /// <summary>
    ///     Adds an IMessageProcessor to the ControlMessageProcessors list if it is not an instance of IVariableExtractor.
    ///     Otherwise, processes it directly.
    /// </summary>
    /// <param name="processor">The IMessageProcessor to be processed or added to the list.</param>
    /// <returns>The current instance of the ReceiveMessageBuilderSupport.</returns>
    public override TS Process(IMessageProcessor processor)
    {
        if (processor is IVariableExtractor)
            base.Process(processor);
        else
            ControlMessageProcessors.Add(processor);
        return _self;
    }

    /// <summary>
    ///     Assigns a message processor to handle the received message.
    /// </summary>
    /// <param name="processor">The message processor to handle the received message.</param>
    /// <returns>The current instance of the receive message builder support.</returns>
    public TS Process(MessageProcessor processor)
    {
        return Process(new DelegatingMessageProcessor(processor));
    }

    /// <summary>
    ///     Processes the received message using the specified variable extractor.
    /// </summary>
    /// <param name="extractor">The variable extractor to use for processing the received message.</param>
    /// <returns>The current instance of the receive message builder support.</returns>
    public TS Process(VariableExtractor extractor)
    {
        return Process(new DelegatingVariableExtractor(extractor));
    }
}