using Agenix.Api.Context;
using Agenix.Api.Message;
using Agenix.Api.Validation;
using Agenix.Core.Message;

namespace Agenix.Core.Validation;

/// <summary>
///     Represents a validation processor that delegates the validation process to another
///     specified validation processor.
/// </summary>
/// <param name="processor">The validation processor to which the validation is delegated.</param>
public class DelegatingValidationProcessor(ValidationProcessor processor) : IValidationProcessor
{
    /// <summary>
    ///     Validates the specified message within the given context.
    /// </summary>
    /// <param name="message">The message to be validated.</param>
    /// <param name="context">The context in which the validation occurs.</param>
    public void Validate(IMessage message, TestContext context)
    {
        processor(message, context);
    }
}