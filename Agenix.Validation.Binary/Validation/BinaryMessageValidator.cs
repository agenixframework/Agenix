#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
