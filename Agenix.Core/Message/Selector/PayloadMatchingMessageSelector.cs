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

using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;

namespace Agenix.Core.Message.Selector;

/// <summary>
///     Message selector matches one or more header elements with the message header. Only in case all matching header
///     elements are present in message header, and its value matches the expected value, the message is accepted.
/// </summary>
public class PayloadMatchingMessageSelector : AbstractMessageSelector
{
    /// <summary>
    ///     Special selector identifying key for this message selector implementation
    /// </summary>
    public const string SelectorId = "payload";

    /// <summary>
    ///     Default constructor using fields.
    /// </summary>
    public PayloadMatchingMessageSelector(string selectKey, string matchingValue, TestContext context) : base(selectKey,
        matchingValue, context)
    {
        if (!selectKey.Equals(SelectorId))
        {
            throw new AgenixSystemException("Invalid usage of payload matching message selector - " +
                                            $"usage restricted to key '{SelectorId}' but was '{selectKey}'");
        }
    }

    /// <summary>
    ///     Determines whether the given message satisfies the selection criteria.
    /// </summary>
    /// <param name="message">The message to evaluate against the selection criteria.</param>
    /// <returns>A boolean value indicating whether the message matches the criteria.</returns>
    public override bool Accept(IMessage message)
    {
        return Evaluate(GetPayloadAsString(message));
    }

    /// <summary>
    ///     Message selector factory for this implementation.
    /// </summary>
    public class Factory : IMessageSelector.IMessageSelectorFactory
    {
        /// <summary>
        ///     Checks if the provided key is supported by this message selector factory.
        /// </summary>
        /// <param name="key">The key to be checked for support.</param>
        /// <returns>True if the key is supported, otherwise false.</returns>
        public bool Supports(string key)
        {
            return key.Equals(SelectorId);
        }

        /// <summary>
        ///     Creates an IMessageSelector based on the provided key and value.
        /// </summary>
        /// <param name="key">The selector key used to determine the appropriate message selector.</param>
        /// <param name="value">The value associated with the selector key.</param>
        /// <param name="context">The test context in which the message selector operates.</param>
        /// <returns>A new instance of IMessageSelector, configured based on the provided key and value.</returns>
        public IMessageSelector Create(string key, string value, TestContext context)
        {
            return new PayloadMatchingMessageSelector(key, value, context);
        }
    }
}
