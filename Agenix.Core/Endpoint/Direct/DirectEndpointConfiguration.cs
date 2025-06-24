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

using Agenix.Api.Message;

namespace Agenix.Core.Endpoint.Direct;

/// <summary>
///     DirectEndpointConfiguration manages configuration settings specifically
///     for direct endpoints, such as setting and getting the message queue
///     and queue name.
/// </summary>
public class DirectEndpointConfiguration : AbstractEndpointConfiguration
{
    /// <summary>
    ///     Destination queue.
    /// </summary>
    private IMessageQueue _queue;

    /// <summary>
    ///     Destination queue name.
    /// </summary>
    private string _queueName;

    /// <summary>
    ///     Set the message queue.
    /// </summary>
    /// <param name="queue">The queue to set.</param>
    public void SetQueue(IMessageQueue queue)
    {
        _queue = queue;
    }

    /// <summary>
    ///     Sets the destination queue name.
    /// </summary>
    /// <param name="queueName">The queue name to set.</param>
    public void SetQueueName(string queueName)
    {
        _queueName = queueName;
    }

    /// <summary>
    ///     Gets the queue.
    /// </summary>
    /// <returns>The queue.</returns>
    public IMessageQueue GetQueue()
    {
        return _queue;
    }

    /// <summary>
    ///     Gets the queue name.
    /// </summary>
    /// <returns>The queue name.</returns>
    public string GetQueueName()
    {
        return _queueName;
    }
}
