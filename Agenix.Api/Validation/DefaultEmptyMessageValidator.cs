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
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Validation.Context;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Validation;

/// <summary>
///     Basic message validator is able to verify empty message payloads. Both received and control message must have empty
///     message payloads otherwise ths validator will raise some exception.
/// </summary>
public class DefaultEmptyMessageValidator : DefaultMessageValidator
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(DefaultEmptyMessageValidator));

    /// <summary>
    ///     Validates that the received message's payload is empty, based on the control message.
    /// </summary>
    /// <param name="receivedMessage">The message received that needs to be validated.</param>
    /// <param name="controlMessage">The control message which contains the expected payload.</param>
    /// <param name="context">The context of the test in which validation is performed.</param>
    /// <param name="validationContext">The context for validation specifics.</param>
    /// <exception cref="ValidationException">Thrown when the validation of the received message fails.</exception>
    public override void ValidateMessage(IMessage receivedMessage, IMessage controlMessage,
        TestContext context, IValidationContext validationContext)
    {
        if (controlMessage?.Payload == null)
        {
            Log.LogDebug("Skip message payload validation as no control message was defined");
            return;
        }

        if (!string.IsNullOrEmpty(controlMessage.GetPayload<string>()))
        {
            throw new ValidationException("Empty message validation failed - control message is not empty!");
        }

        Log.LogDebug("Start to verify empty message payload ...");

        if (!string.IsNullOrEmpty(receivedMessage.GetPayload<string>()))
        {
            throw new ValidationException("Validation failed - received message content is not empty!");
        }

        Log.LogInformation("Message payload is empty as expected: All values OK");
    }
}
