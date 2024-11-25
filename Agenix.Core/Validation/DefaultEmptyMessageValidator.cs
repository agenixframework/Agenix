using Agenix.Core.Exceptions;
using Agenix.Core.Message;
using Agenix.Core.Validation.Context;
using log4net;

namespace Agenix.Core.Validation;

/// <summary>
///     Basic message validator is able to verify empty message payloads. Both received and control message must have empty
///     message payloads otherwise ths validator will raise some exception.
/// </summary>
public class DefaultEmptyMessageValidator : DefaultMessageValidator
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILog _log = LogManager.GetLogger(typeof(DefaultEmptyMessageValidator));

    /// <summary>
    ///     Validates that the received message's payload is empty, based on the control message.
    /// </summary>
    /// <param name="receivedMessage">The message received that needs to be validated.</param>
    /// <param name="controlMessage">The control message which contains the expected payload.</param>
    /// <param name="context">The context of the test in which validation is performed.</param>
    /// <param name="validationContext">The context for validation specifics.</param>
    /// <exception cref="ValidationException">Thrown when the validation of the received message fails.</exception>
    public new void ValidateMessage(IMessage receivedMessage, IMessage controlMessage,
        TestContext context, IValidationContext validationContext)
    {
        if (controlMessage?.Payload == null)
        {
            _log.Debug("Skip message payload validation as no control message was defined");
            return;
        }

        if (!string.IsNullOrEmpty(controlMessage.GetPayload<string>()))
            throw new ValidationException("Empty message validation failed - control message is not empty!");

        _log.Debug("Start to verify empty message payload ...");

        if (!string.IsNullOrEmpty(receivedMessage.GetPayload<string>()))
            throw new ValidationException("Validation failed - received message content is not empty!");

        _log.Info("Message payload is empty as expected: All values OK");
    }
}