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
using Agenix.Api.Message;
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Agenix.Api.Validation.Context;
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
    private static readonly Lazy<ResourcePathTypeResolver> TypeResolver =
        new(() => new ResourcePathTypeResolver(ResourcePath));


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
    private static readonly Lazy<ConcurrentDictionary<string, IMessageValidator<IValidationContext>>> ValidatorsCache =
        new(LoadValidators);

    /// <summary>
    /// Loads all available message validators of type <see cref="IMessageValidator{IValidationContext}"/>
    /// from the type resolver and populates them in a concurrent dictionary for fast retrieval.
    /// </summary>
    /// <returns>
    /// A concurrent dictionary containing the loaded message validators, where the key is the validator name,
    /// and the value is the corresponding <see cref="IMessageValidator{IValidationContext}"/> instance.
    /// </returns>
    private static ConcurrentDictionary<string, IMessageValidator<IValidationContext>> LoadValidators()
    {
        var validators = new ConcurrentDictionary<string, IMessageValidator<IValidationContext>>(
            TypeResolver.Value.ResolveAll<IMessageValidator<IValidationContext>>("", ITypeResolver.DEFAULT_TYPE_PROPERTY, null)
        );

        if (Log.IsEnabled(LogLevel.Debug))
        {
            foreach (var kvp in validators)
            {
                Log.LogDebug("Found message validator '{KvpKey}' as {Name}", kvp.Key, kvp.Value.GetType().Name);
            }
        }

        return validators;
    }

    /// <summary>
    /// Provides a lookup to retrieve a concurrent dictionary that maps validator keys to their corresponding message validators.
    /// </summary>
    /// <returns>
    /// A concurrent dictionary where the keys are strings representing validator identifiers and the values are the corresponding
    /// message validator instances implementing <see cref="IMessageValidator{T}"/> for <see cref="IValidationContext"/>.
    /// </returns>
    public static ConcurrentDictionary<string, IMessageValidator<IValidationContext>> Lookup()
    {
        return ValidatorsCache.Value;
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
                TypeResolver.Value.Resolve<IMessageValidator<IValidationContext>>(validator));
        }
        catch (AgenixSystemException)
        {
            Log.LogWarning($"Failed to resolve validator from resource '{validator}'");
        }

        return Optional<IMessageValidator<IValidationContext>>.Empty;
    }
}
