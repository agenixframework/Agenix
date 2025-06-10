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
