#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
// 
// Copyright (c) 2025 Agenix
// 
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

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
    public virtual bool IsEmpty()
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
