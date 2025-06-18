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
using Agenix.Api.Util;
using Agenix.Core.Message.Selector;
using Agenix.Validation.Json.Json;

namespace Agenix.Validation.Json.Message.Selector;

/// <summary>
///     Message selector accepts JSON messages in case the JsonPath expression evaluation result matches
///     the expected value. With this selector someone can select messages according to a message payload JSON
///     element value, for instance.
///     Syntax is jsonPath:$.root.element
/// </summary>
public class JsonPathPayloadMessageSelector : AbstractMessageSelector
{
    /// <summary>
    ///     Special selector key prefix identifying this message selector implementation
    /// </summary>
    public static readonly string SelectorPrefix = "jsonPath:";

    /// <summary>
    ///     Default constructor using fields.
    /// </summary>
    public JsonPathPayloadMessageSelector(string expression, string control, TestContext context)
        : base(expression[SelectorPrefix.Length..], control, context)
    {
    }

    /// <summary>
    ///     Determines whether the provided IMessage satisfies the selection criteria
    ///     based on the payload content and JSON evaluation.
    /// </summary>
    /// <param name="message">The message to evaluate for selection.</param>
    /// <returns>
    ///     A boolean value indicating whether the specified message meets the criteria.
    ///     Returns true if the message is accepted, otherwise false.
    /// </returns>
    public override bool Accept(IMessage message)
    {
        var payload = GetPayloadAsString(message);
        if (StringUtils.HasText(payload) &&
            !payload.Trim().StartsWith('{') &&
            !payload.Trim().StartsWith('['))
        {
            return false;
        }

        try
        {
            return Evaluate(JsonPathUtils.EvaluateAsString(payload, SelectKey));
        }
        catch (AgenixSystemException)
        {
            return false;
        }
    }

    /// <summary>
    ///     Message selector factory for this implementation.
    /// </summary>
    public class Factory : IMessageSelector.IMessageSelectorFactory
    {
        public bool Supports(string key)
        {
            return key.StartsWith(SelectorPrefix);
        }

        public IMessageSelector Create(string key, string value, TestContext context)
        {
            return new JsonPathPayloadMessageSelector(key, value, context);
        }
    }
}
