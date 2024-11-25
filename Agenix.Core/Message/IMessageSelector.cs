using System.Collections.Generic;
using Agenix.Core.Message.Selector;
using log4net;

namespace Agenix.Core.Message;

public delegate bool MessageSelector(IMessage message);

/// <summary>
///     Interface for selecting messages based on specific criteria.
/// </summary>
public interface IMessageSelector
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(IMessageSelector));


    /// <summary>
    ///     Checks weather this selector should accept given message or not. When accepting the message the selective consumer
    ///     is provided with the message otherwise the message is skipped for this consumer.
    /// </summary>
    /// <param name="message">the message to check</param>
    /// <returns>true if the message will be accepted, false otherwise.</returns>
    bool Accept(IMessage message);

    /// <summary>
    ///     Provides a dictionary of message selector factories.
    /// </summary>
    /// <returns>A dictionary containing message selector factories keyed by selector type.</returns>
    public static IDictionary<string, IMessageSelectorFactory> Lookup()
    {
        var validators = new Dictionary<string, IMessageSelectorFactory>
        {
            { "header", new HeaderMatchingMessageSelector.Factory() },
            { "payload", new PayloadMatchingMessageSelector.Factory() }
        };

        if (!Log.IsDebugEnabled) return validators;
        foreach (var kvp in validators) Log.Debug($"Found message selector '{kvp.Key}' as {kvp.Value.GetType()}");

        return validators;
    }

    /// <summary>
    ///     Factory capable of creating a message selector from key value pairs.
    /// </summary>
    interface IMessageSelectorFactory
    {
        /// <summary>
        ///     Check if this factory is able to create a message selector for the given key.
        /// </summary>
        /// <param name="key">The selector key.</param>
        /// <returns>true if the factory accepts the key, false otherwise.</returns>
        bool Supports(string key);

        /// Create a new message selector for given predicates.
        /// @param key selector key
        /// @param value selector value
        /// @param context test context
        /// @return the created selector
        /// /
        IMessageSelector Create(string key, string value, TestContext context);
    }
}