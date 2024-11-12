using System.Collections.Generic;
using Agenix.Core.Message;
using Agenix.Core.Validation.Context;

namespace Agenix.Core.Validation;

public interface IMessageValidator<T> where T : IValidationContext
{
    /// <summary>
    ///     Validates the received message against the control message using the provided context and a list of validation
    ///     contexts.
    /// </summary>
    /// <param name="receivedMessage">The message that has been received and needs to be validated.</param>
    /// <param name="controlMessage">The control message to validate against.</param>
    /// <param name="context">The test context that holds specific settings and configurations for the validation.</param>
    /// <param name="validationContexts">A list of validation contexts to apply during the validation process.</param>
    void ValidateMessage(IMessage receivedMessage, IMessage controlMessage, TestContext context,
        List<IValidationContext> validationContexts);

    /// <summary>
    ///     Determines if the message validator supports a given message type.
    /// </summary>
    /// <param name="messageType">The type of the message to check.</param>
    /// <param name="message">The message instance to be validated.</param>
    /// <returns>Returns true if the validator supports the message type; otherwise, false.</returns>
    bool SupportsMessageType(string messageType, IMessage message);
}