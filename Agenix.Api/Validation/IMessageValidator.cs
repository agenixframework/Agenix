using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Util;
using Agenix.Api.Validation.Context;
using Agenix.Core.Spi;
using Microsoft.Extensions.Logging;
using ITypeResolver = Agenix.Api.Spi.ITypeResolver;

namespace Agenix.Api.Validation;

/// <summary>
///     Interface for validating messages against control messages using predefined validation contexts.
/// </summary>
/// <typeparam name="T">Type of the validation context that implements IValidationContext.</typeparam>
public interface IMessageValidator<T> where T : IValidationContext
{
    /// <summary>
    ///     Path to the message validator resource lookup.
    /// </summary>
    private const string ResourcePath = "Extension/agenix/message/validator";

    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(IMessageValidator<T>).Name);

    /// <summary>
    ///     Resolves types using a resource path lookup mechanism for identifying custom message validators.
    /// </summary>
    private static readonly ResourcePathTypeResolver TypeResolver = new(ResourcePath);

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
        (
            TypeResolver.ResolveAll<IMessageValidator<IValidationContext>>("", ITypeResolver.DEFAULT_TYPE_PROPERTY,
                null)
        );

        if (!Log.IsEnabled(LogLevel.Debug)) return validators;
        foreach (var kvp in validators)
            Log.LogDebug("Found message validator '{KvpKey}' as {Name}", kvp.Key, kvp.Value.GetType().Name);

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
            return Optional<IMessageValidator<IValidationContext>>.Of(
                TypeResolver.Resolve<IMessageValidator<IValidationContext>>(validator));
        }
        catch (AgenixSystemException)
        {
            Log.LogWarning($"Failed to resolve validator from resource '{validator}'");
        }

        return Optional<IMessageValidator<IValidationContext>>.Empty;
    }
}