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

using System.Collections.Generic;
using System.Linq;
using Agenix.Api.Context;
using Agenix.Api.Message;

namespace Agenix.Core.Variable.Dictionary;

/// <summary>
///     Represents a simple key-value mapping dictionary derived from AbstractDataDictionary.
///     This class is used to process messages and update payloads based on key-value mappings.
/// </summary>
public class SimpleMappingDictionary(Dictionary<string, string> mappings)
    : AbstractDataDictionary<string>
{
    public SimpleMappingDictionary() : this(new Dictionary<string, string>())
    {
    }

    public override void ProcessMessage(IMessage message, TestContext context)
    {
        var payload = message.GetPayload<string>();

        payload = mappings.Aggregate(payload, (current, mapping) => current.Replace(mapping.Key, mapping.Value));

        message.Payload = payload;
    }

    public override R Translate<R>(string key, R value, TestContext context)
    {
        return value;
    }

    public override bool SupportsMessageType(string messageType)
    {
        return true;
    }
}
