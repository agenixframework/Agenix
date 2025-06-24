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

/// <summary>
///     Message correlator interface for synchronous reply messages. Correlator uses a specific header entry in messages in
///     order to construct a unique message correlation ke
/// </summary>
public interface IMessageCorrelator
{
    /// Constructs the correlation key from the message header.
    /// @param request The message from which to extract the correlation key.
    /// @return The constructed correlation key.
    /// /
    string GetCorrelationKey(IMessage request);

    /// Constructs the correlation key from the message header.
    /// <param name="request">The message from which to extract the correlation key.</param>
    /// <return>The constructed correlation key.</return>
    string GetCorrelationKey(string id);

    /// Constructs unique correlation key name for the given consumer name.
    /// Correlation key must be unique across all message consumers running inside a test case.
    /// Therefore, consumer name is passed as an argument and must be part of the constructed correlation key name.
    /// <param name="consumerName">The name of the consumer for which to construct the correlation key name.</param>
    /// <return>The constructed unique correlation key name for the given consumer.</return>
    string GetCorrelationKeyName(string consumerName);
}
