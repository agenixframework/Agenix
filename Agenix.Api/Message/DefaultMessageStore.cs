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

using System.Collections.Concurrent;
using Agenix.Api.Endpoint;

namespace Agenix.Api.Message;

/// <summary>
///     Represents the default implementation of a message store that uses a concurrent dictionary
///     to store and retrieve messages identified by unique string keys.
/// </summary>
public class DefaultMessageStore : ConcurrentDictionary<string, IMessage>, IMessageStore
{
    /// <summary>
    ///     Retrieves a message from the store using a unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the message to be retrieved.</param>
    /// <returns>The message associated with the specified identifier, or null if no match is found.</returns>
    public IMessage GetMessage(string id)
    {
        TryGetValue(id, out var message);
        return message;
    }

    /// <summary>
    ///     Stores a message in the store using a unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the message to be stored.</param>
    /// <param name="message">The message instance to be stored in the store.</param>
    public void StoreMessage(string id, IMessage message)
    {
        TryAdd(id, message);
    }

    /// <summary>
    ///     Constructs a message name by combining the name of the test action and the name of the endpoint.
    /// </summary>
    /// <param name="action">The test action that provides the action name.</param>
    /// <param name="endpoint">The endpoint that provides the endpoint name.</param>
    /// <returns>A string representing the constructed message name in the format: "ActionName(EndpointName)".</returns>
    public string ConstructMessageName(ITestAction action, IEndpoint endpoint)
    {
        return $"{action.Name}({endpoint.Name})";
    }
}
