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

using Agenix.Api.Message;

namespace Agenix.Core.Message;

/// <summary>
///     Default message correlator implementation using the Agenix message id as correlation key.
/// </summary>
public class DefaultMessageCorrelator : IMessageCorrelator
{
    /// <summary>
    ///     Generates a correlation key for the given request message.
    /// </summary>
    /// <param name="request">The message for which to generate the correlation key.</param>
    /// <returns>The correlation key constructed from the message id.</returns>
    public string GetCorrelationKey(IMessage request)
    {
        return MessageHeaders.Id + " = '" + request.Id + "'";
    }

    /// <summary>
    ///     Constructs the correlation key from the given identifier.
    /// </summary>
    /// <param name="id">The identifier from which to generate the correlation key.</param>
    /// <returns>The correlation key constructed from the identifier.</returns>
    public string GetCorrelationKey(string id)
    {
        return MessageHeaders.Id + " = '" + id + "'";
    }

    /// <summary>
    ///     Generates a correlation key name for the given consumer name.
    /// </summary>
    /// <param name="consumerName">The name of the consumer for which to generate the correlation key name.</param>
    /// <returns>The correlation key name constructed from the consumer name.</returns>
    public string GetCorrelationKeyName(string consumerName)
    {
        return MessageHeaders.MessageCorrelationKey + "_" + consumerName;
    }
}
