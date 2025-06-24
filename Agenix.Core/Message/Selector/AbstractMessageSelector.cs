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
using Agenix.Api.Validation.Matcher;

namespace Agenix.Core.Message.Selector;

/// <summary>
///     Abstract base class for message selectors.
/// </summary>
/// <remarks>
///     This abstract class provides the basic functionality to select messages based on key-value pairs
///     and supports validation matcher expressions.
/// </remarks>
public abstract class AbstractMessageSelector(string selectKey, string matchingValue, TestContext context)
    : IMessageSelector
{
    /// <summary>
    ///     Test Context
    /// </summary>
    protected readonly TestContext Context = context;

    protected readonly string MatchingValue = matchingValue;

    /// <summary>
    ///     Key and value to evaluate selection with
    /// </summary>
    protected readonly string SelectKey = selectKey;

    public abstract bool Accept(IMessage message);

    /// <summary>
    ///     Reads message payload as String either from a message object directly or from nested Agenix message representation.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public string GetPayloadAsString(IMessage message)
    {
        if (message.Payload is IMessage innerMessage)
        {
            return innerMessage.GetPayload<string>();
        }

        return message.Payload.ToString();
    }

    /// <summary>
    ///     Evaluates the given value to match this selector matching condition. Automatically supports validation matcher
    ///     expressions.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected bool Evaluate(string value)
    {
        if (ValidationMatcherUtils.IsValidationMatcherExpression(MatchingValue))
        {
            try
            {
                ValidationMatcherUtils.ResolveValidationMatcher(SelectKey, value, MatchingValue, Context);
                return true;
            }
            catch (ValidationException)
            {
                return false;
            }
        }

        return value.Equals(MatchingValue);
    }
}
