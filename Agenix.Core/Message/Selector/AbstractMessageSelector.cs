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
using Agenix.Api.Validation.Matcher;

namespace Agenix.Core.Message.Selector;

/// <summary>
///     Abstract base class for message selectors.
/// </summary>
/// <remarks>
///     This abstract class provides the basic functionality to select messages based on key-value pairs
///     and supports validation matcher expressions.
/// </remarks>
public abstract class AbstractMessageSelector(string selectKey, string matchingValue, TestContext context)
    : IMessageSelector
{
    /// <summary>
    ///     Test Context
    /// </summary>
    protected readonly TestContext Context = context;

    protected readonly string MatchingValue = matchingValue;

    /// <summary>
    ///     Key and value to evaluate selection with
    /// </summary>
    protected readonly string SelectKey = selectKey;

    public abstract bool Accept(IMessage message);

    /// <summary>
    ///     Reads message payload as String either from a message object directly or from nested Agenix message representation.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public string GetPayloadAsString(IMessage message)
    {
        if (message.Payload is IMessage innerMessage)
        {
            return innerMessage.GetPayload<string>();
        }

        return message.Payload.ToString();
    }

    /// <summary>
    ///     Evaluates the given value to match this selector matching condition. Automatically supports validation matcher
    ///     expressions.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected bool Evaluate(string value)
    {
        if (ValidationMatcherUtils.IsValidationMatcherExpression(MatchingValue))
        {
            try
            {
                ValidationMatcherUtils.ResolveValidationMatcher(SelectKey, value, MatchingValue, Context);
                return true;
            }
            catch (ValidationException)
            {
                return false;
            }
        }

        return value.Equals(MatchingValue);
    }
}
