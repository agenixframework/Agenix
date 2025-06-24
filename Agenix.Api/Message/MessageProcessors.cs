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

using System.Collections.ObjectModel;
using Agenix.Api.Exceptions;
using Agenix.Core;

namespace Agenix.Api.Message;

/// <summary>
///     Provides operations to manage and process a collection of message processors.
/// </summary>
public class MessageProcessors
{
    private List<IMessageProcessor> _messageProcessors = [];

    /// <summary>
    ///     Sets the messageProcessors property.
    /// </summary>
    /// <param name="messageProcessors">List of message processors.</param>
    public void SetMessageProcessors(List<IMessageProcessor> messageProcessors)
    {
        _messageProcessors = messageProcessors;
    }

    /// <summary>
    ///     Gets the message processors.
    /// </summary>
    /// <returns>Unmodifiable list of message processors.</returns>
    public IReadOnlyList<IMessageProcessor> GetMessageProcessors()
    {
        return new ReadOnlyCollection<IMessageProcessor>(_messageProcessors);
    }

    /// <summary>
    ///     Adds a new message processor.
    /// </summary>
    /// <param name="processor">Message processor to add.</param>
    public void AddMessageProcessor(IMessageProcessor processor)
    {
        if (processor is IScoped scopedProcessor && !scopedProcessor.IsGlobalScope())
        {
            throw new AgenixSystemException(
                "Unable to add non-global scoped processor to global message processors - " +
                "either declare processor as global scope or explicitly add it to test actions instead");
        }

        _messageProcessors.Add(processor);
    }
}
