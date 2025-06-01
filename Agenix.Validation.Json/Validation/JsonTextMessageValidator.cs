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

using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Core.Util;
using Agenix.Core.Validation.Json;
using Agenix.Validation.Json.Validation.Schema;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Json.Validation;

/// <summary>
///     This message validator implementation is able to validate two JSON text objects. The order of JSON entries can
///     differ as specified in JSON protocol. Tester defines an expected control JSON text with optional ignored entries.
///     JSONArray as well as nested JSONObjects are supported, too.
///     Validator offers two different modes to operate. By default, strict mode is set and the validator will also check
///     the exact number of control object fields to match. No additional fields in the received JSON data structure will
///     be
///     accepted. In soft mode validator allows additional fields in received JSON data structure, so the control JSON
///     object can be a partial subset.
/// </summary>
public class JsonTextMessageValidator : AbstractMessageValidator<IMessageValidationContext>,
    IMessageValidator<IValidationContext>
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(JsonTextMessageValidator));

    /// <summary>
    ///     Default is a static readonly provider that offers a default instance
    ///     of the JsonElementValidator class. This provider is created and configured
    ///     using a DefaultProvider implementation of the IProvider interface.
    /// </summary>
    private IProvider _elementValidatorProvider = new JsonElementValidator.DefaultProvider();

    /// <summary>
    ///     Instance of the <see cref="JsonSchemaValidation" /> responsible for validating JSON structures against a defined
    ///     schema.
    /// </summary>
    private JsonSchemaValidation _jsonSchemaValidation = new();

    /// <summary>
    ///     Indicates whether the JSON message validation should be performed in strict mode.
    ///     This flag uses the CoreSettings.JsonMessageValidationStrict property to determine
    ///     the strictness of validation for JSON messages within the context of JsonTextMessageValidator.
    /// </summary>
    private bool _strict = AgenixSettings.JsonMessageValidationStrict();

    /// Determines if this validator supports the specified message type and message.
    /// <param name="messageType">The type of the message to validate.</param>
    /// <param name="message">The message instance to validate.</param>
    /// <return>True if the specified message type and message are supported; otherwise false.</return>
    /// /
    public override bool SupportsMessageType(string messageType, IMessage message)
    {
        return messageType.Equals(nameof(MessageType.JSON), StringComparison.OrdinalIgnoreCase) &&
               MessageUtils.HasJsonPayload(message);
    }

    public override void ValidateMessage(IMessage receivedMessage, IMessage controlMessage, TestContext context,
        IMessageValidationContext validationContext)
    {
        Log.LogDebug("Start JSON message validation ...");

        if (validationContext.IsSchemaValidationEnabled)
        {
            _jsonSchemaValidation.Validate(receivedMessage, context, validationContext);
        }

        var receivedJsonText = receivedMessage.GetPayload<string>();
        var controlJsonText = context.ReplaceDynamicContentInString(controlMessage.GetPayload<string>());

        if (string.IsNullOrWhiteSpace(controlJsonText))
        {
            Log.LogDebug("Skip message payload validation as no control message was defined");
            return;
        }


        if (string.IsNullOrWhiteSpace(receivedJsonText))
        {
            throw new ValidationException("Validation failed - expected message contents, but received empty message!");
        }

        _elementValidatorProvider.GetValidator(_strict, context, validationContext)
            .Validate(JsonElementValidatorItem<object>.ParseJson(receivedJsonText, controlJsonText));

        Log.LogDebug("JSON message validation successful: All values OK");
    }

    /// Finds and returns the appropriate validation context from the provided list of validation contexts.
    /// <param name="validationContexts">The list of validation contexts to search through.</param>
    /// <return>The matching validation context if found; otherwise, a default or base validation context.</return>
    public override IMessageValidationContext FindValidationContext(List<IValidationContext> validationContexts)
    {
        if (validationContexts
                .FirstOrDefault(x => x is JsonMessageValidationContext) is IMessageValidationContext
            jsonMessageValidationContext)
        {
            return jsonMessageValidationContext;
        }

        var defaultMessageValidationContext = validationContexts
                .FirstOrDefault(x => x.GetType() == typeof(DefaultMessageValidationContext))
            as IMessageValidationContext;

        return defaultMessageValidationContext ?? base.FindValidationContext(validationContexts);
    }

    /// Gets the type of the required validation context for this validator.
    protected override Type GetRequiredValidationContextType()
    {
        return typeof(JsonMessageValidationContext);
    }

    /// Sets whether the validator operates in strict mode.
    /// <param name="strict">
    ///     If true, the validator will require the exact number of control object fields to match and will
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

    /// <summary>
    ///     Sets the JSON schema validation instance for this validator.
    /// </summary>
    /// <param name="jsonSchemaValidation">The JSON schema validation instance to set.</param>
    public void SetJsonSchemaValidation(JsonSchemaValidation jsonSchemaValidation)
    {
        _jsonSchemaValidation = jsonSchemaValidation;
    }

    /// Represents the validation logic for JSON structures against a defined schema using a specified JSON schema filter.
    /// <param name="jsonSchemaValidation">The filter defining the JSON schema used for validation.</param>
    /// <return>A configured instance of the JsonSchemaValidation class for performing schema validation.</return>
    public JsonTextMessageValidator JsonSchemaValidation(JsonSchemaValidation jsonSchemaValidation)
    {
        SetJsonSchemaValidation(jsonSchemaValidation);
        return this;
    }
}
