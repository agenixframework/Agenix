using System;
using Agenix.Core.Exceptions;
using Agenix.Core.Message;
using Agenix.Core.Util;
using Agenix.Core.Validation.Context;
using log4net;

namespace Agenix.Core.Validation.Json;

/// <summary>
///     This message validator implementation is able to validate two JSON text objects. The order of JSON entries can
///     differ as specified in JSON protocol. Tester defines an expected control JSON text with optional ignored entries.
///     JSONArray as well as nested JSONObjects are supported, too.
///     Validator offers two different modes to operate. By default, strict mode is set and the validator will also check
///     the exact amount of control object fields to match. No additional fields in received JSON data structure will be
///     accepted. In soft mode validator allows additional fields in received JSON data structure so the control JSON
///     object can be a partial subset.
/// </summary>
public class JsonTextMessageValidator : AbstractMessageValidator<JsonMessageValidationContext>,
    IMessageValidator<IValidationContext>
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(JsonTextMessageValidator));

    /// <summary>
    ///     Default is a static readonly provider that offers a default instance
    ///     of the JsonElementValidator class. This provider is created and configured
    ///     using a DefaultProvider implementation of the IProvider interface.
    /// </summary>
    public IProvider _elementValidatorProvider = new JsonElementValidator.DefaultProvider();

    /// <summary>
    ///     Indicates whether the JSON message validation should be performed in strict mode.
    ///     This flag utilizes the CoreSettings.JsonMessageValidationStrict property to determine
    ///     the strictness of validation for JSON messages within the context of JsonTextMessageValidator.
    /// </summary>
    private bool _strict = CoreSettings.JsonMessageValidaitonStrict();

    /// Determines if the specified message type and message are supported by this validator.
    /// <param name="messageType">The type of the message to validate.</param>
    /// <param name="message">The message instance to validate.</param>
    /// <return>True if the specified message type and message are supported; otherwise false.</return>
    /// /
    public override bool SupportsMessageType(string messageType, IMessage message)
    {
        return messageType.Equals(MessageType.JSON.ToString(), StringComparison.OrdinalIgnoreCase) &&
               MessageUtils.HasJsonPayload(message);
    }

    public override void ValidateMessage(IMessage receivedMessage, IMessage controlMessage, TestContext context,
        JsonMessageValidationContext validationContext)
    {
        Log.Debug("Start JSON message validation ...");

        var receivedJsonText = receivedMessage.GetPayload<string>();
        var controlJsonText = context.ReplaceDynamicContentInString(controlMessage.GetPayload<string>());

        if (string.IsNullOrWhiteSpace(controlJsonText))
        {
            Log.Debug("Skip message payload validation as no control message was defined");
            return;
        }

        if (string.IsNullOrWhiteSpace(receivedJsonText))
            throw new ValidationException("Validation failed - expected message contents, but received empty message!");

        _elementValidatorProvider.GetValidator(_strict, context, validationContext)
            .Validate(JsonElementValidatorItem<object>.ParseJson(receivedJsonText, controlJsonText));

        Log.Debug("JSON message validation successful: All values OK");
    }

    /// Gets the type of the required validation context for this validator.
    protected override Type GetRequiredValidationContextType()
    {
        return typeof(JsonMessageValidationContext);
    }

    /// Sets whether the validator operates in strict mode.
    /// <param name="strict">
    ///     If true, the validator will require the exact amount of control object fields to match and will
    ///     not accept any additional fields in the received JSON data structure. If false, the validator will allow additional
    ///     fields in the received JSON data structure.
    /// </param>
    public void SetStrict(bool strict)
    {
        _strict = strict;
    }

    // Fluent method
    /// Enforces the validator to operate in strict mode or soft mode.
    /// <param name="strict">If true, the validator operates in strict mode; otherwise, it operates in soft mode.</param>
    /// <return>Returns the current instance of JsonTextMessageValidator, allowing method chaining.</return>
    public JsonTextMessageValidator Strict(bool strict)
    {
        SetStrict(strict);
        return this;
    }

    /// Sets the provider used to validate JSON elements.
    /// <param name="elementValidatorProvider">The provider to use for JSON element validation.</param>
    public void SetElementValidatorProvider(IProvider elementValidatorProvider)
    {
        _elementValidatorProvider = elementValidatorProvider;
    }

    // Fluent method
    /// Sets the element validator provider to be used by this validator. This provider
    /// is responsible for supplying validators for individual elements of the JSON message.
    /// <param name="elementValidatorProvider">The element validator provider to use.</param>
    /// <return>The current instance of <see cref="JsonTextMessageValidator" /> with the specified provider set.</return>
    public JsonTextMessageValidator ElementValidatorProvider(IProvider elementValidatorProvider)
    {
        SetElementValidatorProvider(elementValidatorProvider);
        return this;
    }
}