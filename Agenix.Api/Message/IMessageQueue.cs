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
