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
