using System;
using Agenix.Core.Exceptions;
using Agenix.Core.Json;
using Agenix.Core.Message;
using Agenix.Core.Validation.Context;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Agenix.Core.Validation.Json;

/// <summary>
///     Message validator evaluates set of JSONPath expressions on message payload and checks that values are as expected.
/// </summary>
public class JsonPathMessageValidator : AbstractMessageValidator<JsonPathMessageValidationContext>,
    IMessageValidator<IValidationContext>
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILog _log = LogManager.GetLogger(typeof(JsonPathMessageValidator));

    /// <summary>
    ///     Determines if the given message type is supported by evaluating
    ///     whether the message type is JSON and has a valid JSON payload.
    /// </summary>
    /// <param name="messageType">The type of the message to check.</param>
    /// <param name="message">The message instance to validate.</param>
    /// <returns>
    ///     A boolean value indicating whether the message type is supported.
    /// </returns>
    public override bool SupportsMessageType(string messageType, IMessage message)
    {
        return new JsonTextMessageValidator().SupportsMessageType(messageType, message);
    }

    /// <summary>
    ///     Validates the received message against the control message using JSONPath expressions
    ///     specified in the validation context.
    /// </summary>
    /// <param name="receivedMessage">The message that was received and needs validation.</param>
    /// <param name="controlMessage">The control message to validate against.</param>
    /// <param name="context">The execution context for the current test.</param>
    /// <param name="validationContext">The context containing JSONPath expressions and their expected values.</param>
    /// <exception cref="ValidationException">Thrown when the received message payload is empty or validation fails.</exception>
    /// <exception cref="CoreSystemException">Thrown when the received message payload cannot be parsed as JSON.</exception>
    public override void ValidateMessage(IMessage receivedMessage, IMessage controlMessage, TestContext context,
        JsonPathMessageValidationContext validationContext)
    {
        if (validationContext.GetJsonPathExpressions() == null ||
            validationContext.GetJsonPathExpressions().Count == 0) return;

        if (string.IsNullOrWhiteSpace(receivedMessage.GetPayload<string>()))
            throw new ValidationException("Unable to validate message elements - receive message payload was empty");

        _log.Debug("Start JSONPath element validation ...");

        try
        {
            var readerContext = JToken.Parse(receivedMessage.GetPayload<string>());

            foreach (var (key, value) in validationContext.GetJsonPathExpressions())
            {
                var expectedValue = value;

                if (expectedValue is string)
                    //check if expected value is variable or function (and resolve it, if yes)
                    expectedValue = context.ReplaceDynamicContentInString(expectedValue.ToString());
                var jsonPathExpression = context.ReplaceDynamicContentInString(key);
                var jsonPathResult = JsonPathUtils.EvaluateAsString(readerContext, jsonPathExpression);
                //do the validation of actual and expected value for element
                ValidationUtils.ValidateValues(jsonPathResult, expectedValue, jsonPathExpression, context);

                _log.DebugFormat("Validating element: {0}='{1}': OK", jsonPathExpression, expectedValue);
            }

            _log.Debug("JSONPath element validation successful: All values OK");
        }
        catch (JsonReaderException e)
        {
            throw new CoreSystemException("Failed to parse JSON text", e);
        }
    }

    /// <summary>
    ///     Retrieves the required type of validation context for the JsonPathMessageValidator.
    /// </summary>
    /// <returns>
    ///     A <see cref="Type" /> representing the required validation context,
    ///     which is <see cref="JsonPathMessageValidationContext" />.
    /// </returns>
    protected override Type GetRequiredValidationContextType()
    {
        return typeof(JsonPathMessageValidationContext);
    }
}