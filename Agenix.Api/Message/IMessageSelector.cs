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
    ///     Lazy-initialized type resolver used for mapping resource paths to types and properties in the message selector
    ///     domain.
    /// </summary>
    private static readonly Lazy<ResourcePathTypeResolver> TypeResolver =
        new(() => new ResourcePathTypeResolver(ResourcePath));

    /// <summary>
    ///     Lazy-initialized cache of message selector factories for improved performance and thread safety.
    /// </summary>
    private static readonly Lazy<IDictionary<string, IMessageSelectorFactory>> FactoriesCache =
        new(LoadMessageSelectorFactories);

    /// <summary>
    ///     Checks weather this selector should accept a given message or not. When accepting the message, the selective
    ///     consumer is provided with the message; otherwise the message is skipped for this consumer.
    /// </summary>
    /// <param name="message">the message to check</param>
    /// <returns>true if the message will be accepted, false otherwise.</returns>
    bool Accept(IMessage message);

    /// <summary>
    ///     Loads all available message selector factories from the type resolver.
    /// </summary>
    /// <returns>A dictionary containing all loaded message selector factories.</returns>
    private static IDictionary<string, IMessageSelectorFactory> LoadMessageSelectorFactories()
    {
        var factories = new Dictionary<string, IMessageSelectorFactory>(
            TypeResolver.Value.ResolveAll<IMessageSelectorFactory>()
        );

        if (Log.IsEnabled(LogLevel.Debug))
        {
            foreach (var kvp in factories)
            {
                Log.LogDebug("Found message selector '{KvpKey}' as {Type}", kvp.Key, kvp.Value.GetType());
            }
        }

        return factories;
    }

    /// <summary>
    ///     Provides a dictionary of message selector factories.
    /// </summary>
    /// <returns>A dictionary containing message selector factories keyed by a selector type.</returns>
    public static IDictionary<string, IMessageSelectorFactory> Lookup()
    {
        return FactoriesCache.Value;
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

        /// <summary>
        ///     Create a new message selector for given predicates.
        /// </summary>
        /// <param name="key">selector key</param>
        /// <param name="value">selector value</param>
        /// <param name="context">test context</param>
        /// <returns>the created selector</returns>
        IMessageSelector Create(string key, string value, TestContext context);
    }
}
