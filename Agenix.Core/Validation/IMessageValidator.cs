using System.Collections.Generic;
using Agenix.Core.Exceptions;
using Agenix.Core.Message;
using Agenix.Core.Util;
using Agenix.Core.Validation.Context;
using Agenix.Core.Validation.Json;
using Agenix.Core.Validation.Text;
using log4net;

namespace Agenix.Core.Validation;

/// <summary>
///     Interface for validating messages against control messages using predefined validation contexts.
/// </summary>
/// <typeparam name="T">Type of the validation context that implements IValidationContext.</typeparam>
public interface IMessageValidator<T> where T : IValidationContext
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(IMessageValidator<T>).Name);

    /// <summary>
    ///     Validates the received message against the control message using the provided context and a list of validation
    ///     contexts.
    /// </summary>
    /// <param name="receivedMessage">The message that has been received and needs to be validated.</param>
    /// <param name="controlMessage">The control message to validate against.</param>
    /// <param name="context">The test context that holds specific settings and configurations for the validation.</param>
    /// <param name="validationContexts">A list of validation contexts to apply during the validation process.</param>
    void ValidateMessage(IMessage receivedMessage, IMessage controlMessage, TestContext context,
        List<IValidationContext> validationContexts);

    /// <summary>
    ///     Determines if the message validator supports a given message type.
    /// </summary>
    /// <param name="messageType">The type of the message to check.</param>
    /// <param name="message">The message instance to be validated.</param>
    /// <returns>Returns true if the validator supports the message type; otherwise, false.</returns>
    bool SupportsMessageType(string messageType, IMessage message);

    /// <summary>
    ///     Retrieves a dictionary of message validators mapped by string keys.
    /// </summary>
    /// <returns>
    ///     A dictionary mapping string identifiers to corresponding IMessageValidator instances of type IValidationContext.
    /// </returns>
    public static Dictionary<string, IMessageValidator<IValidationContext>> Lookup()
    {
        var validators = new Dictionary<string, IMessageValidator<IValidationContext>>
        {
            { "defaultValidator", new DefaultMessageHeaderValidator() },
            { "defaultPlaintextMessageValidator", new PlainTextMessageValidator() },
            { "defaultBinaryBase64MessageValidator", new BinaryBase64MessageValidator() },
            { "defaultGzipBinaryBase64MessageValidator", new GzipBinaryBase64MessageValidator() },
            { "defaultJsonMessageValidator", new JsonTextMessageValidator() },
            { "defaultJsonPathMessageValidator", new JsonPathMessageValidator() }
        };

        if (!Log.IsDebugEnabled) return validators;
        foreach (var kvp in validators) Log.Debug($"Found message validator '{kvp.Key}' as {kvp.Value.GetType().Name}");

        return validators;
    }

    /// <summary>
    ///     Attempts to look up and retrieve an instance of a message validator based on the given validator name.
    /// </summary>
    /// <param name="validator">The name of the validator to be retrieved.</param>
    /// <returns>
    ///     An Optional containing the message validator if found, or an empty Optional if the validator name is not
    ///     recognized.
    /// </returns>
    public static Optional<IMessageValidator<IValidationContext>> Lookup(string validator)
    {
        try
        {
            switch (validator)
            {
                case "header":
                {
                    var instance = new DefaultMessageHeaderValidator();
                    return Optional<IMessageValidator<IValidationContext>>.Of(instance);
                }
                case "plaintext":
                {
                    var instance = new PlainTextMessageValidator();
                    return Optional<IMessageValidator<IValidationContext>>.Of(instance);
                }
                case "binary_base64":
                {
                    var instance = new BinaryBase64MessageValidator();
                    return Optional<IMessageValidator<IValidationContext>>.Of(instance);
                }
                case "gzip_base64":
                {
                    var instance = new GzipBinaryBase64MessageValidator();
                    return Optional<IMessageValidator<IValidationContext>>.Of(instance);
                }
                case "json":
                {
                    var instance = new JsonTextMessageValidator();
                    return Optional<IMessageValidator<IValidationContext>>.Of(instance);
                }
                case "json-path":
                {
                    var instance = new JsonPathMessageValidator();
                    return Optional<IMessageValidator<IValidationContext>>.Of(instance);
                }
            }
        }
        catch (CoreSystemException)
        {
            Log.Warn($"Failed to resolve validator from resource '{validator}'");
        }

        return Optional<IMessageValidator<IValidationContext>>.Empty;
    }
}