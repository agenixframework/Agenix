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

namespace Agenix.Api.Message;

/// <summary>
///     Message correlator interface for synchronous reply messages. Correlator uses a specific header entry in messages in
///     order to construct a unique message correlation ke
/// </summary>
public interface IMessageCorrelator
{
    /// Constructs the correlation key from the message header.
    /// @param request The message from which to extract the correlation key.
    /// @return The constructed correlation key.
    /// /
    string GetCorrelationKey(IMessage request);

    /// Constructs the correlation key from the message header.
    /// <param name="request">The message from which to extract the correlation key.</param>
    /// <return>The constructed correlation key.</return>
    string GetCorrelationKey(string id);

    /// Constructs unique correlation key name for the given consumer name.
    /// Correlation key must be unique across all message consumers running inside a test case.
    /// Therefore, consumer name is passed as an argument and must be part of the constructed correlation key name.
    /// <param name="consumerName">The name of the consumer for which to construct the correlation key name.</param>
    /// <return>The constructed unique correlation key name for the given consumer.</return>
    string GetCorrelationKeyName(string consumerName);
}
