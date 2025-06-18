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

using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Util;
using Agenix.Api.Validation.Context;
using Microsoft.Extensions.Logging;
using IValidationContext = Agenix.Api.Validation.Context.IValidationContext;

namespace Agenix.Api.Validation;

/// <summary>
///     The MessageValidatorRegistry class is responsible for managing and retrieving message validators within a system.
///     It provides mechanisms to find, add, and manage validators based on message types and payload criteria.
/// </summary>
public class MessageValidatorRegistry
{
    private static readonly ILogger Log = LogManager.GetLogger(typeof(MessageValidatorRegistry));

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
    ///     A private collection that holds registered message validators mapped by their unique names.
    /// </summary>
    private IDictionary<string, IMessageValidator<IValidationContext>> _messageValidators =
        new Dictionary<string, IMessageValidator<IValidationContext>>();

    /// <summary>
    ///     A collection that maintains mappings of schema validator names to their
    ///     corresponding implementations of <see cref="ISchemaValidator{T}" />.
    ///     It is used for validating schemas within the context of message validation.
    /// </summary>
    private IDictionary<string, ISchemaValidator<ISchemaValidationContext>> _schemeValidators =
        new Dictionary<string, ISchemaValidator<ISchemaValidationContext>>();


    /// <summary>
    ///     A property that provides access to a collection of message validators, represented as a dictionary
    ///     where the key is a string and the value is an implementation of <see cref="IMessageValidator{T}" />.
    ///     This property allows for the retrieval or replacement of the current set of validators.
    /// </summary>
    public IDictionary<string, IMessageValidator<IValidationContext>> MessageValidators
    {
        get => _messageValidators;
        set => _messageValidators = value;
    }


    /// <summary>
    ///     Gets or sets all schema validators.
    /// </summary>
    public IDictionary<string, ISchemaValidator<ISchemaValidationContext>> SchemaValidators
    {
        get => _schemeValidators;
        set => _schemeValidators = value;
    }

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
        {
            if (message.Payload is string payload && !string.IsNullOrWhiteSpace(payload))
            {
                payload = payload.Trim();

                if (MessagePayloadUtils.IsXml(payload) &&
                    !messageType.Equals(nameof(MessageType.XML), StringComparison.OrdinalIgnoreCase))
                {
                    matchingValidators = FindFallbackMessageValidators(nameof(MessageType.XML), message);
                }
                else if (MessagePayloadUtils.IsJson(payload) &&
                         !messageType.Equals(nameof(MessageType.JSON), StringComparison.OrdinalIgnoreCase))
                {
                    matchingValidators = FindFallbackMessageValidators(nameof(MessageType.JSON), message);
                }
                else if (!messageType.Equals(nameof(MessageType.PLAINTEXT), StringComparison.OrdinalIgnoreCase))
                {
                    matchingValidators = FindFallbackMessageValidators(nameof(MessageType.PLAINTEXT), message);
                }
            }
        }

        if (IsEmptyOrDefault(matchingValidators) && string.IsNullOrWhiteSpace(message.GetPayload<string>()))
        {
            matchingValidators.Add(_defaultEmptyMessageValidator);
        }

        if (IsEmptyOrDefault(matchingValidators))
        {
            if (mustFindValidator)
            {
                Log.LogWarning(
                    "Unable to find proper message validator. Message type is '{MessageType}' and message payload is '{GetPayload}'",
                    messageType, message.GetPayload<string>());
                throw new AgenixSystemException("Failed to find proper message validator for message");
            }

            Log.LogWarning("Unable to find proper message validator - fallback to default text equals validation.");
            matchingValidators.Add(_defaultTextEqualsMessageValidator);
        }

        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug("Found {MatchingValidatorsCount} message validators for message", matchingValidators.Count);
        }

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
    ///     Finds schema validators that support a specific message type and message payload.
    /// </summary>
    /// <param name="messageType">The type of the message for which schema validators are being searched.</param>
    /// <param name="message">The message instance used as part of schema validation criteria.</param>
    /// <returns>
    ///     A list of schema validators that support the specified message type and message. Returns an empty list if no
    ///     applicable validators are found.
    /// </returns>
    private List<ISchemaValidator<ISchemaValidationContext>> FindFallbackSchemaValidators(string messageType,
        IMessage message)
    {
        return _schemeValidators.Values.Where(validator => validator.SupportsMessageType(messageType, message))
            .ToList();
    }

    /// <summary>
    ///     Finds a message validator registered under a specified name.
    /// </summary>
    /// <param name="name">The name of the message validator to retrieve.</param>
    /// <returns>An Optional containing the found message validator if present, otherwise an empty Optional.</returns>
    public virtual Optional<IMessageValidator<IValidationContext>> FindMessageValidator(string name)
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
        if (_messageValidators.ContainsKey(name) && Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug("Overwriting message validator '{Name}' in registry", name);
        }

        _messageValidators[name] = messageValidator;
    }

    /// <summary>
    ///     Adds a schema validator to the registry or updates an existing one with the specified name.
    /// </summary>
    /// <param name="name">The unique name of the schema validator to add or update in the registry.</param>
    /// <param name="schemaValidator">The schema validator instance to be added or updated in the registry.</param>
    public void AddSchemeValidator(string name, ISchemaValidator<ISchemaValidationContext> schemaValidator)
    {
        if (_schemeValidators.ContainsKey(name) && Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug("Overwriting scheme validator '{Name}' in registry", name);
        }

        _schemeValidators[name] = schemaValidator;
    }

    /// <summary>
    ///     Retrieves the default message validator instance used for validating messages within the system.
    /// </summary>
    /// <returns>
    ///     The default implementation of the IMessageValidator interface for validating messages.
    /// </returns>
    public IMessageValidator<IValidationContext> GetDefaultMessageValidator()
    {
        return _defaultTextEqualsMessageValidator;
    }

    /// <summary>
    ///     Retrieves the default message header validator.
    /// </summary>
    /// <returns>The default message header validator if available; otherwise, returns a predefined default validator.</returns>
    public IMessageValidator<IValidationContext> GetDefaultMessageHeaderValidator()
    {
        // LINQ to find and filter the default message header validator
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
    ///     Verify if the given message validator is a subclass of the default message header validator.
    /// </summary>
    /// <param name="messageValidator"></param>
    /// <returns></returns>
    /// <exception cref="AgenixSystemException"></exception>
    private bool IsDefaultMessageHeaderValidator(IMessageValidator<IValidationContext> messageValidator)
    {
        if (_defaultMessageHeaderValidator == null)
        {
            var defaultHeaderValidator = IMessageValidator<IValidationContext>.Lookup("header");
            _defaultMessageHeaderValidator = defaultHeaderValidator.IsPresent
                ? defaultHeaderValidator.Value
                : throw new AgenixSystemException("Unable to locate default message header validator");
        }

        return _defaultMessageHeaderValidator.GetType().IsAssignableFrom(messageValidator.GetType());
    }

    /// <summary>
    ///     Finds schema validators for a given message type and message.
    /// </summary>
    /// <param name="messageType">The type of the message to find schema validators for.</param>
    /// <param name="message">The message instance to be validated.</param>
    /// <returns>
    ///     A list of schema validators that support the specified message type and message.
    ///     If no specific schema validators are found, fallback validators may be returned based on the message payload.
    /// </returns>
    public List<ISchemaValidator<ISchemaValidationContext>> FindSchemaValidators(string messageType, IMessage message)
    {
        var matchingSchemaValidators = _schemeValidators.Values
            .Where(validator => validator.SupportsMessageType(messageType, message)).ToList();

        if (matchingSchemaValidators.Count != 0)
        {
            return matchingSchemaValidators;
        }

        // try to find fallback message validator for given message payload
        if (message.Payload is not string payload || string.IsNullOrWhiteSpace(payload))
        {
            return matchingSchemaValidators;
        }

        payload = payload.Trim();

        if (IsXmlPredicate.Instance.Test(payload) && messageType != nameof(MessageType.XML))
        {
            matchingSchemaValidators = FindFallbackSchemaValidators(nameof(MessageType.XML), message);
        }
        else if (IsJsonPredicate.Instance.Test(payload) && messageType != nameof(MessageType.JSON))
        {
            matchingSchemaValidators = FindFallbackSchemaValidators(nameof(MessageType.JSON), message);
        }

        return matchingSchemaValidators;
    }

    /// <summary>
    ///     Finds a schema validator corresponding to the provided name.
    /// </summary>
    /// <param name="name">The name of the schema validator to retrieve.</param>
    /// <returns>
    ///     An optional containing the schema validator if found; otherwise, an empty optional.
    /// </returns>
    public Optional<ISchemaValidator<ISchemaValidationContext>> FindSchemaValidator(string name)
    {
        return _schemeValidators.TryGetValue(name, out var validator)
            ? Optional<ISchemaValidator<ISchemaValidationContext>>.Of(validator)
            : Optional<ISchemaValidator<ISchemaValidationContext>>.Empty;
    }
}
