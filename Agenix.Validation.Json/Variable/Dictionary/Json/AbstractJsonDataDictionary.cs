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
using Agenix.Core.Variable.Dictionary;

namespace Agenix.Validation.Json.Variable.Dictionary.Json;

/// <summary>
/// Abstract json data dictionary works on json message data. Each value is translated with a dictionary.
/// </summary>
public abstract class AbstractJsonDataDictionary : AbstractDataDictionary<string>
{
    /// <summary>
    /// Checks if this message interceptor is capable of this message type. XML message interceptors may only apply to this message
    /// type while JSON message interceptor implementations do not and vice versa.
    /// </summary>
    /// <param name="messageType">The message type representation as string (e.g. XML, JSON, CSV, plaintext).</param>
    /// <returns>True if this message interceptor supports the message type.</returns>
    public override bool SupportsMessageType(string messageType)
    {
        return nameof(MessageType.JSON).Equals(messageType, StringComparison.OrdinalIgnoreCase);
    }
}
