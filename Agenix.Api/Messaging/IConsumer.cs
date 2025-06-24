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
///     Represents a message consumer capable of receiving messages.
/// </summary>
public interface IConsumer
{
    /// <summary>
    ///     Gets the consumer name.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Receive message with default timeout.
    /// </summary>
    /// <param name="context">the internal context</param>
    /// <returns>the internal message</returns>
    IMessage Receive(TestContext context);

    /// <summary>
    ///     Receive message with a given timeout.
    /// </summary>
    /// <param name="context">the internal context</param>
    /// <param name="timeout">the timeout in milliseconds</param>
    /// <returns>the internal message</returns>
    IMessage Receive(TestContext context, long timeout);
}
