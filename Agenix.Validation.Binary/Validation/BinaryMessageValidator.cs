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

using System.Text;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Binary.Validation;

/// <summary>
///     A validator for binary messages that extends the functionality of the default message validator.
/// </summary>
public class BinaryMessageValidator : DefaultMessageValidator
{
    /// <summary>
    ///     Specifies the size of the buffer, in bytes, used for processing streams during binary message validation.
    /// </summary>
    private const int BufferSize = 1024;

    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(BinaryMessageValidator));

    /// <summary>
    ///     Validates a received message against a control message using the provided validation context and test context.
    /// </summary>
    /// <param name="receivedMessage">The message that was received for validation.</param>
    /// <param name="controlMessage">The reference control message to validate against.</param>
    /// <param name="context">The current test execution context.</param>
    /// <param name="validationContext">The context containing validation-specific details and configurations.</param>
    public override void ValidateMessage(IMessage receivedMessage, IMessage controlMessage,
        TestContext context, IValidationContext validationContext)
    {
        using var receivedInput = receivedMessage.GetPayload<Stream>();
        using var controlInput = controlMessage.GetPayload<Stream>();
        Log.LogDebug("Start binary message validation");

        var receivedBuffer = new byte[BufferSize];
        var controlBuffer = new byte[BufferSize];

        using var receivedResult = new MemoryStream();
        using var controlResult = new MemoryStream();

        while (true)
        {
            var n1 = receivedInput.Read(receivedBuffer, 0, BufferSize);
            var n2 = controlInput.Read(controlBuffer, 0, BufferSize);

            switch (n1)
            {
                case -1 when n2 == -1:
                    Log.LogDebug("Binary message validation successful: All values OK");
                    return;
                case -1:
                    throw new ValidationException("Received input stream reached end-of-stream - " +
                                                  "control input stream is not finished yet");
                default:
                    {
                        if (n2 == -1)
                        {
                            throw new ValidationException("Control input stream reached end-of-stream - " +
                                                          "received input stream is not finished yet");
                        }

                        break;
                    }
            }

            for (var i = 0; i < Math.Min(n1, n2); i++)
            {
                var received = receivedBuffer[i];
                var control = controlBuffer[i];

                receivedResult.WriteByte(received);
                controlResult.WriteByte(control);

                if (received == control)
                {
                    continue;
                }

                var receivedStr = Encoding.UTF8.GetString(receivedResult.ToArray());
                var controlStr = Encoding.UTF8.GetString(controlResult.ToArray());

                Log.LogInformation(
                    $"Received input stream is not equal - expected '{controlStr}', but was '{receivedStr}'");

                var expectedPart = controlStr[Math.Max(0, controlStr.Length - Math.Min(25, controlStr.Length))..];
                var actualPart = receivedStr[Math.Max(0, receivedStr.Length - Math.Min(25, receivedStr.Length))..];

                throw new ValidationException(
                    $"Received input stream is not equal to given control, expected '{expectedPart}', but was '{actualPart}'");
            }
        }
    }

    /// <summary>
    ///     Determines whether the specified message type is supported by the validator.
    /// </summary>
    /// <param name="messageType">The type of the message to check for support.</param>
    /// <param name="message">An instance of the message to validate.</param>
    /// <returns>True if the specified message type is supported; otherwise, false.</returns>
    public override bool SupportsMessageType(string messageType, IMessage message)
    {
        return messageType.Equals(nameof(MessageType.BINARY), StringComparison.OrdinalIgnoreCase);
    }
}
