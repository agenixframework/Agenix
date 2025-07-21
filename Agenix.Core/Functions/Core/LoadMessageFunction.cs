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

using System;
using System.Collections.Generic;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;

namespace Agenix.Core.Functions.Core;

/// <summary>
///     Function loads a message from the test context message store. Incoming and sent messages get automatically
///     stored to the message store. Messages are identified by their name.
/// </summary>
public class LoadMessageFunction : IFunction
{
    /// <summary>
    ///     Executes the load message function.
    /// </summary>
    /// <param name="parameterList">List containing the message name and optional selector</param>
    /// <param name="testContext">Test context</param>
    /// <returns>The message body or header value it as a string</returns>
    /// <exception cref="InvalidFunctionUsageException">Thrown when parameters are null or empty</exception>
    /// <exception cref="AgenixSystemException">Thrown when a message or header is not found</exception>
    public string Execute(List<string> parameterList, TestContext testContext)
    {
        if (parameterList == null || parameterList.Count == 0)
        {
            throw new InvalidFunctionUsageException("Function parameters must not be empty");
        }

        var messageName = parameterList[0];
        string messageHeader = null;

        if (messageName.EndsWith(".Body()"))
        {
            messageName = messageName[..messageName.IndexOf(".Body()", StringComparison.Ordinal)];
        }
        else if (messageName.Contains(".Header(") && messageName.EndsWith(')'))
        {
            var headerStart = messageName.IndexOf(".Header(", StringComparison.Ordinal) + 8;
            messageHeader = messageName.Substring(headerStart, messageName.Length - 1 - headerStart);

            if (messageHeader.StartsWith('\'') && messageHeader.EndsWith('\''))
            {
                messageHeader = messageHeader.Substring(1, messageHeader.Length - 2);
            }

            if (string.IsNullOrWhiteSpace(messageHeader))
            {
                throw new AgenixSystemException("Missing header name in function parameter");
            }

            messageName = messageName[..messageName.IndexOf(".Header(", StringComparison.Ordinal)];
        }

        var stored = testContext.MessageStore.GetMessage(messageName);
        if (stored == null)
        {
            throw new AgenixSystemException($"Failed to find stored message of name: '{messageName}'");
        }

        if (!string.IsNullOrWhiteSpace(messageHeader))
        {
            var headerValue = stored.GetHeader(messageHeader);
            if (headerValue == null)
            {
                throw new AgenixSystemException($"Failed to find header '{messageHeader}' in stored message");
            }

            return headerValue.ToString()!;
        }

        return stored.GetPayload<string>();
    }
}
