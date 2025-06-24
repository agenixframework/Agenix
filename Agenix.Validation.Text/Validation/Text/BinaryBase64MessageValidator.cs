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
using Agenix.Api.Validation.Context;

namespace Agenix.Validation.Text.Validation.Text;

/// <summary>
///     Message validator automatically converts received binary data message payload to base64 String. Assumes control
///     message payload is also base64 encoded String, so we can compare the text data with normal plain text validation.
/// </summary>
public class BinaryBase64MessageValidator : PlainTextMessageValidator
{
    /// <summary>
    ///     Validates the received message by converting its payload to a Base64 string if it is a byte array
    ///     and then delegates the validation to the base class.
    /// </summary>
    /// <param name="receivedMessage">The message received that needs to be validated.</param>
    /// <param name="controlMessage">The control message to validate against.</param>
    /// <param name="context">The context in which the validation is occurring.</param>
    /// <param name="validationContext">The context containing validation-specific data.</param>
    public override void ValidateMessage(IMessage receivedMessage, IMessage controlMessage, TestContext context,
        IValidationContext validationContext)
    {
        if (receivedMessage.Payload is byte[] bytes)
        {
            receivedMessage.Payload = Convert.ToBase64String(bytes);
        }

        base.ValidateMessage(receivedMessage, controlMessage, context, validationContext);
    }

    /// <summary>
    ///     Determines if the given message type is supported by checking if it matches 'BINARY_BASE64'.
    /// </summary>
    /// <param name="messageType">The type of the message as a string.</param>
    /// <param name="message">The message object to validate.</param>
    /// <returns>True if the message type is 'BINARY_BASE64'; otherwise, false.</returns>
    public override bool SupportsMessageType(string messageType, IMessage message)
    {
        return messageType.Equals(nameof(MessageType.BINARY_BASE64), StringComparison.OrdinalIgnoreCase);
    }
}
