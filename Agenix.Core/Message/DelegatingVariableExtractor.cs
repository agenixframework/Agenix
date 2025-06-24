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
using Agenix.Api.Variable;

namespace Agenix.Core.Message;

/// <summary>
///     Extracts variables from a message by delegating the extraction work to another specified variable extractor.
/// </summary>
/// <param name="variableExtractor">The variable extractor to which the extraction work is delegated.</param>
public class DelegatingVariableExtractor(VariableExtractor variableExtractor) : IVariableExtractor
{
    /// <summary>
    ///     Extracts variables from a message and context using the provided variable extractor delegate.
    /// </summary>
    /// <param name="message">The message from which variables are to be extracted.</param>
    /// <param name="context">The context used during the variable extraction process.</param>
    public void ExtractVariables(IMessage message, TestContext context)
    {
        variableExtractor(message, context);
    }

    /// <summary>
    ///     Processes the given message within the provided test context by extracting variables.
    /// </summary>
    /// <param name="message">The message to be processed.</param>
    /// <param name="context">The test context in which the message processing occurs.</param>
    public void Process(IMessage message, TestContext context)
    {
        ExtractVariables(message, context);
    }
}
