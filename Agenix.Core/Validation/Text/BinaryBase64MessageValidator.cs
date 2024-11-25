using System;
using Agenix.Core.Message;
using Agenix.Core.Validation.Context;

namespace Agenix.Core.Validation.Text;

/// <summary>
///     Message validator automatically converts received binary data message payload to base64 String. Assumes control
///     message payload is also base64 encoded String so we can compare the text data with normal plain text validation.
/// </summary>
public class BinaryBase64MessageValidator : PlainTextMessageValidator
{
    /// <summary>
    ///     Validates the received message by converting its payload to a Base64 string if it is a byte array,
    ///     and then delegates the validation to the base class.
    /// </summary>
    /// <param name="receivedMessage">The message received that needs to be validated.</param>
    /// <param name="controlMessage">The control message to validate against.</param>
    /// <param name="context">The context in which the validation is occurring.</param>
    /// <param name="validationContext">The context containing validation-specific data.</param>
    public new void ValidateMessage(IMessage receivedMessage, IMessage controlMessage, TestContext context,
        IValidationContext validationContext)
    {
        if (receivedMessage.Payload is byte[] bytes) receivedMessage.Payload = Convert.ToBase64String(bytes);

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
        return messageType.Equals(MessageType.BINARY_BASE64.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}