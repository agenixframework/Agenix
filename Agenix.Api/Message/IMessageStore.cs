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

using Agenix.Api.Endpoint;

namespace Agenix.Api.Message;

/// <summary>
///     Defines a contract for managing messages, allowing retrieval, storage, and construction
///     of message names based on contextual components.
/// </summary>
public interface IMessageStore
{
    /// <summary>
    ///     Retrieves a message from the store using the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the message to be retrieved.</param>
    /// <returns>The message associated with the provided identifier, or null if no message is found.</returns>
    IMessage GetMessage(string id);

    /// <summary>
    ///     Stores a message in the store using the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier to associate with the message.</param>
    /// <param name="message">The message to be stored in the store.</param>
    void StoreMessage(string id, IMessage message);

    /// <summary>
    ///     Constructs a message name based on the provided action and endpoint.
    /// </summary>
    /// <param name="action">The test action that contributes to the message name.</param>
    /// <param name="endpoint">The endpoint that contributes to the message name.</param>
    /// <returns>A string representing the constructed message name composed of the action and endpoint details.</returns>
    string ConstructMessageName(ITestAction action, IEndpoint endpoint);
}
