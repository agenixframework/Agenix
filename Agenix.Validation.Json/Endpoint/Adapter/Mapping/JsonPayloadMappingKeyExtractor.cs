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
using Agenix.Core.Endpoint.Adapter.Mapping;
using Agenix.Validation.Json.Json;

namespace Agenix.Validation.Json.Endpoint.Adapter.Mapping;

/// <summary>
///     Extracts a mapping key from a JSON payload using a JSON Path expression.
/// </summary>
public class JsonPayloadMappingKeyExtractor : AbstractMappingKeyExtractor
{
    /// <summary>
    ///     JSON Path expression evaluated on message payload
    /// </summary>
    private string _jsonPathExpression = "$.KeySet()";

    /// <summary>
    ///     Evaluates and returns the mapping key from the provided request message's payload using a JSON Path expression.
    /// </summary>
    /// <param name="request">The message from which to extract the mapping key.</param>
    /// <returns>The extracted mapping key as a string.</returns>
    protected override string GetMappingKey(IMessage request)
    {
        return JsonPathUtils.EvaluateAsString(request.GetPayload<string>(), _jsonPathExpression);
    }

    /// <summary>
    ///     Sets the JSON Path expression to be evaluated on the message payload.
    /// </summary>
    /// <param name="jsonPathExpression">The JSON Path expression to use for extraction.</param>
    public void SetJsonPathExpression(string jsonPathExpression)
    {
        _jsonPathExpression = jsonPathExpression;
    }
}
