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

using System.Collections.Generic;
using Agenix.Api.Context;
using Agenix.Api.Message;

namespace Agenix.Core.Message.Selector;

/// <summary>
///     Message selector matches one or more header elements with the message header. Only in case all matching header
///     elements are present in message header and its value matches the expected value the message is accepted.
/// </summary>
public class HeaderMatchingMessageSelector(string selectKey, string matchingValue, TestContext context)
    : AbstractMessageSelector(selectKey, matchingValue, context)
{
    /// <summary>
    ///     Special selector key prefix identifying this message selector implementation
    /// </summary>
    public const string SelectorPrefix = "header:";

    /// <summary>
    ///     Matches the header of a message against expected values.
    /// </summary>
    /// <param name="messageHeaders">The headers of the message to be matched.</param>
    /// <returns>True if the header matches the expected values, otherwise false.</returns>
    private bool MatchHeader(IDictionary<string, object> messageHeaders)
    {
        if (!messageHeaders.TryGetValue(SelectKey, out var value))
        {
            return false;
        }

        var valueAsString = value?.ToString();
        return Evaluate(valueAsString);
    }

    /// <summary>
    ///     Determines whether the message is accepted based on the header matching logic.
    /// </summary>
    /// <param name="message">The message to be assessed.</param>
    /// <returns>True if the message is accepted based on the header matching criteria; otherwise false.</returns>
    public override bool Accept(IMessage message)
    {
        var messageHeaders = message.GetHeaders();

        var nestedMessageHeaders = new Dictionary<string, object>();
        if (message.Payload is IMessage nestedMessage)
        {
            nestedMessageHeaders = nestedMessage.GetHeaders();
        }

        if (nestedMessageHeaders.ContainsKey(SelectKey))
        {
            return MatchHeader(nestedMessageHeaders);
        }

        return messageHeaders.ContainsKey(SelectKey) && MatchHeader(messageHeaders);
    }

    /// <summary>
    ///     Factory class responsible for creating instances of IMessageSelector.
    /// </summary>
    /// <remarks>
    ///     This factory checks whether a given key is supported and creates the appropriate IMessageSelector based on the
    ///     provided key and value.
    /// </remarks>
    public class Factory : IMessageSelector.IMessageSelectorFactory
    {
        /// <summary>
        ///     Checks if the provided key is supported by this message selector factory.
        /// </summary>
        /// <param name="key">The key to be checked for support.</param>
        /// <returns>True if the key is supported, otherwise false.</returns>
        public bool Supports(string key)
        {
            return key.StartsWith(SelectorPrefix);
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
            return key.StartsWith(SelectorPrefix)
                ? new HeaderMatchingMessageSelector(key[SelectorPrefix.Length..], value, context)
                : new HeaderMatchingMessageSelector(key, value, context);
        }
    }
}
