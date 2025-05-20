using Agenix.Api.Message;
using Agenix.Api.Validation.Context;

namespace Agenix.Api.Validation;

/// <summary>
///     The DefaultMessageValidator class provides a default implementation for validating messages.
///     It extends the AbstractMessageValidator class and operates on instances of IValidationContext.
/// </summary>
public class DefaultMessageValidator : AbstractMessageValidator<IValidationContext>
{
    /// <summary>
    ///     Determines whether the validator supports the specified message type.
    /// </summary>
    /// <param name="messageType">The type of the message to be validated.</param>
    /// <param name="message">The message instance to be validated.</param>
    /// <returns>A boolean value indicating whether the message type is supported.</returns>
    public override bool SupportsMessageType(string messageType, IMessage message)
    {
        return true;
    }

    /// <summary>
    /// Finds and returns the appropriate validation context from a list of validation contexts,
    /// excluding contexts of the type HeaderValidationContext.
    /// </summary>
    /// <param name="validationContexts">The list of available validation contexts to search through.</param>
    /// <returns>The first validation context that matches the criteria, with HeaderValidationContext types excluded.</returns>
    public override IValidationContext FindValidationContext(List<IValidationContext> validationContexts)
    {
        return base.FindValidationContext(validationContexts
            .Where(it => !(it is HeaderValidationContext))
            .ToList());
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