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
using Agenix.Api.Message;

namespace Agenix.Api.Messaging;

/// <summary>
///     Interface representing a selective message consumer capable of receiving messages based on specified selectors.
/// </summary>
public interface ISelectiveConsumer : IConsumer
{
    /// Receive a message with a message selector and default timeout.
    /// @param selector Specifies the criteria for selecting messages.
    /// @param context The context within which the message is to be received.
    /// @return The received message.
    /// /
    IMessage Receive(string selector, TestContext context);

    /// Receive a message with a message selector and default timeout.
    /// <param name="selector">Specifies the criteria for selecting messages.</param>
    /// <param name="context">The context within which the message is to be received.</param>
    /// <return>The received message.</return>
    IMessage Receive(string selector, TestContext context, long timeout);
}
