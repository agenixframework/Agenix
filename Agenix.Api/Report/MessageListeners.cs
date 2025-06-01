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

/// Class responsible for managing and notifying message listeners.
public class MessageListeners : IMessageListenerAware
{
    private readonly List<IMessageListener> _messageListeners = [];

    /// Adds a new message listener.
    /// <param name="listener">The listener to be added.</param>
    public void AddMessageListener(IMessageListener listener)
    {
        if (!_messageListeners.Contains(listener))
        {
            _messageListeners.Add(listener);
        }
    }

    /// Notifies all registered message listeners about the inbound message.
    /// <param name="message">The inbound message that needs to be propagated to the listeners.</param>
    /// <param name="context">The context in which the message is being received.</param>
    public virtual void OnInboundMessage(IMessage message, TestContext context)
    {
        if (message == null)
        {
            return;
        }

        foreach (var listener in _messageListeners)
        {
            listener.OnInboundMessage(message, context);
        }
    }

    /// Notifies all registered message listeners about the outbound message.
    /// <param name="message">The outbound message that needs to be sent to the listeners.</param>
    /// <param name="context">The context in which the message is being sent.</param>
    public virtual void OnOutboundMessage(IMessage message, TestContext context)
    {
        if (message == null)
        {
            return;
        }

        foreach (var listener in _messageListeners)
        {
            listener.OnOutboundMessage(message, context);
        }
    }

    /// Checks if the message listeners list is empty.
    /// <return>Returns true if there are no message listeners; otherwise, false.</return>
    public bool IsEmpty()
    {
        return _messageListeners.Count == 0;
    }

    /// Retrieves the list of registered message listeners.
    /// <return>Returns an IReadOnlyList of IMessageListener representing the current message listeners.</return>
    public IList<IMessageListener> GetMessageListeners()
    {
        return _messageListeners.AsReadOnly();
    }
}
