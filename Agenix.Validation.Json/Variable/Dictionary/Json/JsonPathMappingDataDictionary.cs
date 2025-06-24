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
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Variable.Dictionary;
using Agenix.Validation.Json.Validation;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Json.Variable.Dictionary.Json;

/// <summary>
/// JSON data dictionary implementation maps elements via JsonPath expressions. When an element is identified by some expression
/// in the dictionary, the value is overwritten accordingly.
/// </summary>
public class JsonPathMappingDataDictionary : AbstractJsonDataDictionary
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(JsonPathMappingDataDictionary));

    public override void ProcessMessage(IMessage message, TestContext context)
    {
        if (message.Payload == null || string.IsNullOrWhiteSpace(message.GetPayload<string>()))
        {
            return;
        }

        var delegateProcessor = new JsonPathMessageProcessor.Builder()
            .IgnoreNotFound(true)
            .Expressions(Mappings.ToDictionary(
                entry => entry.Key,
                entry => (object)entry.Value))
            .Build();

        delegateProcessor.ProcessMessage(message, context);
    }

    public override T Translate<T>(string jsonPath, T value, TestContext context)
    {
        return value;
    }

    public override void Initialize()
    {
        if (PathMappingStrategy != null && !PathMappingStrategy.Equals(PathMappingStrategy.EXACT))
        {
            Log.LogWarning("{ClassName} ignores path mapping strategy other than {Strategy}",
                GetType().Name, PathMappingStrategy.EXACT);
        }

        base.Initialize();
    }
}
