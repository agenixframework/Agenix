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

/// Send a new message to the queue.
/// @param message the message to be sent to the queue.
public interface IMessageQueue
{
    /// Send a new message to the queue.
    /// @param message the message to be sent to the queue.
    /// /
    void Send(IMessage message);

    /// Receive any message on the queue. If no message is present, return null.
    /// @return the first message on the queue or null if no message is available.
    /// /
    IMessage Receive()
    {
        return Receive(_ => true);
    }

    /// Receive any message on the queue. If no message is present, return null.
    /// @return the first message on the queue or null if no message is available.
    /// /
    IMessage Receive(long timeout)
    {
        return Receive(_ => true, timeout);
    }

    /// Receive any message on the queue. If no message is present, return null.
    /// @return the first message on the queue or null if no message is available.
    /// /
    IMessage Receive(MessageSelector selector);

    /// Receive any message on the queue. If no message is present, return null.
    /// @return the first message on the queue or null if no message is available.
    /// /
    IMessage Receive(MessageSelector selector, long timeout);

    /// Purge messages selected by a given selector.
    /// @param selector the criteria to select the messages to be purged.
    /// /
    void Purge(IMessageSelector selector);
}
