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
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Api.Util;
using Agenix.Core.Message.Selector;
using Agenix.Validation.Json.Json;

namespace Agenix.Validation.Json.Message.Selector;

/// <summary>
///     Message selector accepts JSON messages in case the JsonPath expression evaluation result matches
///     the expected value. With this selector someone can select messages according to a message payload JSON
///     element value, for instance.
///     Syntax is jsonPath:$.root.element
/// </summary>
public class JsonPathPayloadMessageSelector : AbstractMessageSelector
{
    /// <summary>
    ///     Special selector key prefix identifying this message selector implementation
    /// </summary>
    public static readonly string SelectorPrefix = "jsonPath:";

    /// <summary>
    ///     Default constructor using fields.
    /// </summary>
    public JsonPathPayloadMessageSelector(string expression, string control, TestContext context)
        : base(expression[SelectorPrefix.Length..], control, context)
    {
    }

    /// <summary>
    ///     Determines whether the provided IMessage satisfies the selection criteria
    ///     based on the payload content and JSON evaluation.
    /// </summary>
    /// <param name="message">The message to evaluate for selection.</param>
    /// <returns>
    ///     A boolean value indicating whether the specified message meets the criteria.
    ///     Returns true if the message is accepted, otherwise false.
    /// </returns>
    public override bool Accept(IMessage message)
    {
        var payload = GetPayloadAsString(message);
        if (StringUtils.HasText(payload) &&
            !payload.Trim().StartsWith('{') &&
            !payload.Trim().StartsWith('['))
        {
            return false;
        }

        try
        {
            return Evaluate(JsonPathUtils.EvaluateAsString(payload, SelectKey));
        }
        catch (AgenixSystemException)
        {
            return false;
        }
    }

    /// <summary>
    ///     Message selector factory for this implementation.
    /// </summary>
    public class Factory : IMessageSelector.IMessageSelectorFactory
    {
        public bool Supports(string key)
        {
            return key.StartsWith(SelectorPrefix);
        }

        public IMessageSelector Create(string key, string value, TestContext context)
        {
            return new JsonPathPayloadMessageSelector(key, value, context);
        }
    }
}
