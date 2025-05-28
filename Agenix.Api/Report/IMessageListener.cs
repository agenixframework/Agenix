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

namespace Agenix.Api.Report;

/// Provides mechanisms to listen to inbound and outbound message events,
/// enabling actions to be taken on raw message data at different stages of
/// message processing.
/// /
public interface IMessageListener
{
    /// Invoked on inbound message event. Raw message data is passed to this listener
    /// in a very early state of message processing.
    /// @param message The inbound message being processed.
    /// @param context The context in which the message is processed, providing environment and additional metadata.
    /// /
    void OnInboundMessage(IMessage message, TestContext context);

    /// Invoked on outbound message event. Raw message data is passed to this listener
    /// in a very late state of message processing.
    /// @param message
    /// @param context
    /// /
    void OnOutboundMessage(IMessage message, TestContext context);
}
