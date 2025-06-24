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
using Agenix.Api.Message;

namespace Agenix.Api.Validation;

public delegate void ValidationProcessor(IMessage message, TestContext context);

/// <summary>
///     Defines methods for validating and processing messages within a specified test context.
/// </summary>
public interface IValidationProcessor : IMessageProcessor
{
    /// <summary>
    ///     Processes the provided message within the given context.
    /// </summary>
    /// <param name="message">The message to be processed.</param>
    /// <param name="context">The context within which the message will be processed.</param>
    void IMessageProcessor.Process(IMessage message, TestContext context)
    {
        Validate(message, context);
    }

    /// <summary>
    ///     Validates the provided message within the specified test context.
    /// </summary>
    /// <param name="message">The message to be validated.</param>
    /// <param name="context">The test context in which validation will occur.</param>
    void Validate(IMessage message, TestContext context);
}
