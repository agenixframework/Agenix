using Agenix.Api.Context;
using Agenix.Api.Message;

namespace Agenix.Api.Validation;

public delegate void ValidationProcessor(IMessage message, TestContext context);

/// <summary>
///     Defines methods for validating and processing messages within a specified test context.
/// </summary>
public interface IValidationProcessor : IMessageProcessor
{
    /// <summary>
    ///     Processes the provided message within the given context.
    /// </summary>
    /// <param name="message">The message to be processed.</param>
    /// <param name="context">The context within which the message will be processed.</param>
    void IMessageProcessor.Process(IMessage message, TestContext context)
    {
        Validate(message, context);
    }

    /// <summary>
    ///     Validates the provided message within the specified test context.
    /// </summary>
    /// <param name="message">The message to be validated.</param>
    /// <param name="context">The test context in which validation will occur.</param>
    void Validate(IMessage message, TestContext context);
}