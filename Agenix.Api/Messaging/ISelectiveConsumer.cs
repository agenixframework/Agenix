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
