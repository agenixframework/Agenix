using System;
using Agenix.Core.Message;
using Agenix.Core.Validation.Context;

namespace Agenix.Core.Validation;

/// <summary>
///     The DefaultMessageValidator class provides a default implementation for validating messages.
///     It extends the AbstractMessageValidator class and operates on instances of IValidationContext.
/// </summary>
public class DefaultMessageValidator : AbstractMessageValidator<IValidationContext>
{
    /// <summary>
    ///     Determines whether the specified message type is supported by the validator.
    /// </summary>
    /// <param name="messageType">The type of the message to be validated.</param>
    /// <param name="message">The message instance to be validated.</param>
    /// <returns>A boolean value indicating whether the message type is supported.</returns>
    public override bool SupportsMessageType(string messageType, IMessage message)
    {
        return true;
    }

    /// <summary>
    ///     Provides the required type of validation context for the message validator.
    /// </summary>
    /// <returns>The type of the validation context required by the message validator.</returns>
    protected override Type GetRequiredValidationContextType()
    {
        return typeof(IValidationContext);
    }
}