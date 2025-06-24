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

using System.Xml;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Core.Message.Selector;
using Agenix.Validation.Xml.Namespace;
using Agenix.Validation.Xml.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Xml.Message.Selector;

/// <summary>
///     Message selector that accepts XML messages according to specified root element QName.
/// </summary>
public class RootQNameMessageSelector : AbstractMessageSelector
{
    /// <summary>
    ///     Special selector element name identifying this message selector implementation
    /// </summary>
    public const string SelectorId = "root-qname";

    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(RootQNameMessageSelector));

    /// <summary>
    ///     Target message XML root QName to look for
    /// </summary>
    private readonly QName _rootQName;

    /// <summary>
    ///     Default constructor using fields.
    /// </summary>
    /// <param name="name">Selector name</param>
    /// <param name="value">Selector value</param>
    /// <param name="context">Test context</param>
    /// <exception cref="AgenixSystemException">When selector usage is invalid or QName is invalid</exception>
    public RootQNameMessageSelector(string name, string value, TestContext context)
        : base(name, value, context)
    {
        if (!SelectKey.Equals(SelectorId))
        {
            throw new AgenixSystemException(
                $"Invalid usage of root QName message selector - " +
                $"usage restricted to key '{SelectorId}' but was '{SelectKey}'");
        }

        if (QNameUtils.ValidateQName(value))
        {
            _rootQName = QNameUtils.ParseQNameString(value);
        }
        else
        {
            throw new AgenixSystemException($"Invalid root QName string '{value}'");
        }
    }

    /// <summary>
    ///     Accepts or rejects message based on root element QName comparison.
    /// </summary>
    /// <param name="message">The message to check</param>
    /// <returns>True if the message will be accepted, false otherwise</returns>
    public override bool Accept(IMessage message)
    {
        XmlDocument doc;

        try
        {
            doc = XmlUtils.ParseMessagePayload(GetPayloadAsString(message));
        }
        catch (XmlException ex)
        {
            Log.LogWarning(ex, "Root QName message selector ignoring not well-formed XML message payload");
            return false; // non XML message - not accepted
        }

        var firstChild = doc.DocumentElement ?? doc.FirstChild;
        if (firstChild == null)
        {
            return false;
        }

        return !string.IsNullOrEmpty(_rootQName.NamespaceURI)
            ? _rootQName.Equals(QNameUtils.GetQNameForNode(firstChild))
            : _rootQName.LocalPart.Equals(firstChild.LocalName);
    }

    /// <summary>
    ///     Message selector factory for this implementation.
    /// </summary>
    public class Factory : IMessageSelector.IMessageSelectorFactory
    {
        /// <summary>
        ///     Checks if this factory supports the given key.
        /// </summary>
        /// <param name="key">Selector key</param>
        /// <returns>True if the factory accepts the key, false otherwise</returns>
        public bool Supports(string key)
        {
            return key.Equals(SelectorId);
        }

        /// <summary>
        ///     Creates a new RootQNameMessageSelector for given predicates.
        /// </summary>
        /// <param name="key">Selector key</param>
        /// <param name="value">Selector value</param>
        /// <param name="context">Test context</param>
        /// <returns>The created selector</returns>
        public IMessageSelector Create(string key, string value, TestContext context)
        {
            return new RootQNameMessageSelector(key, value, context);
        }
    }
}
