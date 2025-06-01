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
