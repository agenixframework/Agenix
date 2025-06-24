#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
// 
// Copyright (c) 2025 Agenix
// 
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

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
