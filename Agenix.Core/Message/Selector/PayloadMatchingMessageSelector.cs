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
