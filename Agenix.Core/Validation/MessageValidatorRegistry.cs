using System;
using System.Collections.Generic;
using System.Linq;
using Agenix.Core.Exceptions;
using Agenix.Core.Message;
using Agenix.Core.Util;
using log4net;
using IValidationContext = Agenix.Core.Validation.Context.IValidationContext;

namespace Agenix.Core.Validation;

public class MessageValidatorRegistry
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(MessageValidatorRegistry));

    /// <summary>
    ///     The default POCO id in Spring application context
    /// </summary>
    public static readonly string POCO_NAME = "agenixMessageValidatorRegistry";

    /// <summary>
    ///     Default message validators used as a fallback option
    /// </summary>
    private readonly DefaultEmptyMessageValidator _defaultEmptyMessageValidator = new();

    private readonly DefaultTextEqualsMessageValidator _defaultTextEqualsMessageValidator = new();

    /// <summary>
    ///     Default message header validator - gets looked up via resource path
    /// </summary>
    private IMessageValidator<IValidationContext> _defaultMessageHeaderValidator;

    /// <summary>
    ///     Registered message validators
    /// </summary>
    private IDictionary<string, IMessageValidator<IValidationContext>> _messageValidators =
        new Dictionary<string, IMessageValidator<IValidationContext>>();

    /// <summary>
    ///     Finds message validators for a given message type and message.
    /// </summary>
    /// <param name="messageType">The type of the message to find validators for.</param>
    /// <param name="message">The message instance to validate.</param>
    /// <param name="mustFindValidator">Indicates whether it is mandatory to find a validator.</param>
    /// <returns>
    ///     A list of message validators that support the specified message type and message. If no specific validator is
    ///     found, fallback validators may be returned.
    /// </returns>
    public List<IMessageValidator<IValidationContext>> FindMessageValidators(string messageType,
        IMessage message, bool mustFindValidator = false)
    {
        List<IMessageValidator<IValidationContext>> matchingValidators = [];
        matchingValidators.AddRange(
            _messageValidators.Values.Where(validator => validator.SupportsMessageType(messageType, message)));

        if (IsEmptyOrDefault(matchingValidators))
            // Try to find fallback message validator for given message payload
            if (message.Payload is string payload && !string.IsNullOrWhiteSpace(payload))
            {
                payload = payload.Trim();

                if (payload.StartsWith('<') &&
                    !messageType.Equals(MessageType.XML.ToString(), StringComparison.OrdinalIgnoreCase))
                    matchingValidators = FindFallbackMessageValidators(MessageType.XML.ToString(), message);
                else if ((payload.StartsWith('{') || payload.StartsWith('[')) &&
                         !messageType.Equals(MessageType.JSON.ToString(), StringComparison.OrdinalIgnoreCase))
                    matchingValidators = FindFallbackMessageValidators(MessageType.JSON.ToString(), message);
                else if (!messageType.Equals(MessageType.PLAINTEXT.ToString(), StringComparison.OrdinalIgnoreCase))
                    matchingValidators = FindFallbackMessageValidators(MessageType.PLAINTEXT.ToString(), message);
            }

        if (IsEmptyOrDefault(matchingValidators) && string.IsNullOrWhiteSpace(message.GetPayload<string>()))
            matchingValidators.Add(_defaultEmptyMessageValidator);

        if (IsEmptyOrDefault(matchingValidators))
        {
            if (mustFindValidator)
            {
                Log.Warn(
                    $"Unable to find proper message validator. Message type is '{messageType}' and message payload is '{message.GetPayload<string>()}'");
                throw new CoreSystemException("Failed to find proper message validator for message");
            }

            Log.Warn("Unable to find proper message validator - fallback to default text equals validation.");
            matchingValidators.Add(_defaultTextEqualsMessageValidator);
        }

        if (Log.IsDebugEnabled) Log.Debug($"Found {matchingValidators.Count} message validators for message");

        return matchingValidators;
    }

    /// <summary>
    ///     Finds fallback message validators for a given message type and message.
    /// </summary>
    /// <param name="messageType">The type of the message to find validators for.</param>
    /// <param name="message">The message instance to validate.</param>
    /// <returns>A list of message validators that support the specified message type and message.</returns>
    private List<IMessageValidator<IValidationContext>> FindFallbackMessageValidators(string messageType,
        IMessage message)
    {
        return _messageValidators.Values.Where(validator => validator.SupportsMessageType(messageType, message))
            .ToList();
    }

    /// <summary>
    ///     Finds a message validator registered under a specified name.
    /// </summary>
    /// <param name="name">The name of the message validator to retrieve.</param>
    /// <returns>An Optional containing the found message validator if present, otherwise an empty Optional.</returns>
    public Optional<IMessageValidator<IValidationContext>> FindMessageValidator(string name)
    {
        return _messageValidators.TryGetValue(name, out var validator)
            ? Optional<IMessageValidator<IValidationContext>>.Of(validator)
            : Optional<IMessageValidator<IValidationContext>>.Empty;
    }

    /// <summary>
    ///     Retrieves the message validator associated with the specified name.
    /// </summary>
    /// <param name="name">The name of the message validator to retrieve.</param>
    /// <returns>The message validator associated with the specified name.</returns>
    /// <exception cref="NoSuchMessageValidatorException">Thrown if no message validator exists with the specified name.</exception>
    public IMessageValidator<IValidationContext> GetMessageValidator(string name)
    {
        return _messageValidators.TryGetValue(name, out var validator)
            ? validator
            : throw new NoSuchMessageValidatorException($"Unable to find message validator with name '{name}'");
    }

    /// <summary>
    ///     Adds a message validator to the registry using the specified name.
    /// </summary>
    /// <param name="name">The name of the message validator to register.</param>
    /// <param name="messageValidator">The message validator instance to be added to the registry.</param>
    public void AddMessageValidator(string name, IMessageValidator<IValidationContext> messageValidator)
    {
        if (_messageValidators.ContainsKey(name) && Log.IsDebugEnabled)
            Log.Debug($"Overwriting message validator '{name}' in registry");

        _messageValidators[name] = messageValidator;
    }

    /// <summary>
    ///     Sets the message validators in the registry.
    /// </summary>
    /// <param name="messageValidators">
    ///     A dictionary containing message validators to be set in the registry, where the key is
    ///     the validator name and the value is the message validator instance.
    /// </param>
    public void SetMessageValidators(
        IDictionary<string, IMessageValidator<IValidationContext>> messageValidators)
    {
        _messageValidators = messageValidators;
    }

    /// <summary>
    ///     Retrieves all registered message validators.
    /// </summary>
    /// <returns>A dictionary containing all registered message validators.</returns>
    public IDictionary<string, IMessageValidator<IValidationContext>> GetMessageValidators()
    {
        return _messageValidators;
    }

    /// <summary>
    ///     Retrieves the default message header validator.
    /// </summary>
    /// <returns>The default message header validator if available; otherwise, returns a predefined default validator.</returns>
    public IMessageValidator<IValidationContext> GetDefaultMessageHeaderValidator()
    {
        // LINQ to find and filter default message header validator
        return _messageValidators.Values
            .Where(IsDefaultMessageHeaderValidator)
            .FirstOrDefault() ?? _defaultMessageHeaderValidator;
    }

    /// <summary>
    ///     Determines whether the provided list of message validators is empty or contains only the default message header
    ///     validator.
    /// </summary>
    /// <param name="matchingValidators">The list of message validators to check.</param>
    /// <returns>True if the list is empty or contains only the default message header validator; otherwise, false.</returns>
    private bool IsEmptyOrDefault(List<IMessageValidator<IValidationContext>> matchingValidators)
    {
        return matchingValidators.Count == 0 || matchingValidators.All(IsDefaultMessageHeaderValidator);
    }

    /// <summary>
    ///     Verify if given message validator is a subclass of default message header validator.
    /// </summary>
    /// <param name="messageValidator"></param>
    /// <returns></returns>
    /// <exception cref="CoreSystemException"></exception>
    private bool IsDefaultMessageHeaderValidator(IMessageValidator<IValidationContext> messageValidator)
    {
        if (_defaultMessageHeaderValidator == null)
        {
            var defaultHeaderValidator = IMessageValidator<IValidationContext>.Lookup("header");
            _defaultMessageHeaderValidator = defaultHeaderValidator.IsPresent
                ? defaultHeaderValidator.Value
                : throw new CoreSystemException("Unable to locate default message header validator");
        }

        return _defaultMessageHeaderValidator.GetType().IsAssignableFrom(messageValidator.GetType());
    }
}