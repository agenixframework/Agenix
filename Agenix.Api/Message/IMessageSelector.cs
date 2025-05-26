using System.Collections.Concurrent;
using Agenix.Api.Context;
using Agenix.Api.Log;
using Agenix.Core.Spi;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Message;

public delegate bool MessageSelector(IMessage message);

/// <summary>
///     Interface for selecting messages based on specific criteria.
/// </summary>
public interface IMessageSelector
{
    /// <summary>
    ///     Path used to locate the resource for message selector functionality.
    /// </summary>
    private const string ResourcePath = "Extension/agenix/message/selector";

    /// <summary>
    ///     Represents the logger instance used for logging messages within the message selector functionality.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(IMessageSelector));

    /// <summary>
    ///     Represents the type resolver used for mapping resource paths to types and properties in the message selector
    ///     domain.
    /// </summary>
    private static readonly ResourcePathTypeResolver TypeResolver = new(ResourcePath);

    private static IDictionary<string, IMessageSelectorFactory>
        _factories = new ConcurrentDictionary<string, IMessageSelectorFactory>();

    /// <summary>
    ///     Checks weather this selector should accept a given message or not. When accepting the message, the selective
    ///     consumer
    ///     is provided with the message; otherwise the message is skipped for this consumer.
    /// </summary>
    /// <param name="message">the message to check</param>
    /// <returns>true if the message will be accepted, false otherwise.</returns>
    bool Accept(IMessage message);

    /// <summary>
    ///     Provides a dictionary of message selector factories.
    /// </summary>
    /// <returns>A dictionary containing message selector factories keyed by a selector type.</returns>
    public static IDictionary<string, IMessageSelectorFactory> Lookup()
    {
        if (_factories.Count > 0) return _factories;

        _factories = new Dictionary<string, IMessageSelectorFactory>
        (
            TypeResolver.ResolveAll<IMessageSelectorFactory>()
        );

        if (!Log.IsEnabled(LogLevel.Debug)) return _factories;
        foreach (var kvp in _factories) Log.LogDebug("Found message selector '{KvpKey}' as {Type}", kvp.Key, kvp.Value.GetType());

        return _factories;
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