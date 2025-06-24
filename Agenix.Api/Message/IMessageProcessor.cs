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
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Message;

public delegate void MessageProcessor(IMessage message, TestContext context);

/// <summary>
///     Defines the contract for processing messages within the given context.
/// </summary>
public interface IMessageProcessor : IMessageTransformer
{
    /// <summary>
    ///     Represents the path used to locate resources for the message processor.
    /// </summary>
    private const string ResourcePath = "Extension/agenix/message/processor";

    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(IMessageProcessor));

    /// <summary>
    ///     Lazy-initialized type resolver used to locate and retrieve custom message processors by performing
    ///     resource path lookups, enabling dynamic discovery and management of processor types.
    /// </summary>
    private static readonly Lazy<ResourcePathTypeResolver> TypeResolver =
        new(() => new ResourcePathTypeResolver(ResourcePath));

    /// <summary>
    ///     Lazy-initialized cache for individual processor lookups to avoid repeated resolution attempts.
    /// </summary>
    private static readonly Lazy<ConcurrentDictionary<string, object>> ProcessorLookupCache =
        new(() => new ConcurrentDictionary<string, object>());

    /// <summary>
    ///     Processes the given message payload within the specified test context.
    /// </summary>
    /// <param name="message">The message to process.</param>
    /// <param name="context">The context in which the message is being processed.</param>
    void Process(IMessage message, TestContext context);

    /// <summary>
    ///     Transforms the given message within the specified test context.
    /// </summary>
    /// <param name="message">The message to transform.</param>
    /// <param name="context">The context in which the message is being transformed.</param>
    /// <returns>The transformed message.</returns>
    new IMessage Transform(IMessage message, TestContext context)
    {
        Process(message, context);
        return message;
    }

    /// <summary>
    ///     Retrieves an instance of the specified message processor builder based on the provided validator.
    /// </summary>
    /// <param name="processor">The identifier of the message processor builder to retrieve.</param>
    /// <typeparam name="T">The type of IMessageProcessor.</typeparam>
    /// <typeparam name="TB">The type of the builder for the IMessageProcessor.</typeparam>
    /// <returns>An instance of the message processor builder, or null if it fails to resolve.</returns>
    public static Optional<IBuilder<T, TB>> Lookup<T, TB>(string processor)
        where T : IMessageProcessor where TB : IBuilder<T, TB>
    {
        // Create a cache key that includes type information for generic safety
        var cacheKey = $"{processor}_{typeof(TB).FullName}";

        return (Optional<IBuilder<T, TB>>)ProcessorLookupCache.Value.GetOrAdd(cacheKey, key =>
        {
            try
            {
                var instance = TypeResolver.Value.Resolve<TB>(processor);
                return Optional<IBuilder<T, TB>>.Of(instance);
            }
            catch (AgenixSystemException)
            {
                Log.LogWarning(
                    "Failed to resolve message processor from resource '{ResourcePath}/{Processor}'",
                    ResourcePath, processor);
                return Optional<IBuilder<T, TB>>.Empty;
            }
        });
    }

    /// <summary>
    ///     Interface IBuilder defines the contract for building instances of
    ///     IMessageProcessor implementations.
    /// </summary>
    new interface IBuilder<out T, TB> : IMessageTransformer.IBuilder<T, TB>, IBuilder
        where T : IMessageProcessor
        where TB : IBuilder;
}
