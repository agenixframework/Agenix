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

namespace Agenix.Core.Message;

/// <summary>
///     Default message correlator implementation using the Agenix message id as correlation key.
/// </summary>
public class DefaultMessageCorrelator : IMessageCorrelator
{
    /// <summary>
    ///     Generates a correlation key for the given request message.
    /// </summary>
    /// <param name="request">The message for which to generate the correlation key.</param>
    /// <returns>The correlation key constructed from the message id.</returns>
    public string GetCorrelationKey(IMessage request)
    {
        return MessageHeaders.Id + " = '" + request.Id + "'";
    }

    /// <summary>
    ///     Constructs the correlation key from the given identifier.
    /// </summary>
    /// <param name="id">The identifier from which to generate the correlation key.</param>
    /// <returns>The correlation key constructed from the identifier.</returns>
    public string GetCorrelationKey(string id)
    {
        return MessageHeaders.Id + " = '" + id + "'";
    }

    /// <summary>
    ///     Generates a correlation key name for the given consumer name.
    /// </summary>
    /// <param name="consumerName">The name of the consumer for which to generate the correlation key name.</param>
    /// <returns>The correlation key name constructed from the consumer name.</returns>
    public string GetCorrelationKeyName(string consumerName)
    {
        return MessageHeaders.MessageCorrelationKey + "_" + consumerName;
    }
}
