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

using System.Collections.Concurrent;
using Agenix.Api.Context;
using Agenix.Api.Log;
using Agenix.Api.Spi;
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
        if (_factories.Count > 0)
        {
            return _factories;
        }

        _factories = new Dictionary<string, IMessageSelectorFactory>
        (
            TypeResolver.ResolveAll<IMessageSelectorFactory>()
        );

        if (!Log.IsEnabled(LogLevel.Debug))
        {
            return _factories;
        }

        foreach (var kvp in _factories)
        {
            Log.LogDebug("Found message selector '{KvpKey}' as {Type}", kvp.Key, kvp.Value.GetType());
        }

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
