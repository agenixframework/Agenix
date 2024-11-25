using System.Collections.Generic;
using Agenix.Core.Message;
using Agenix.Core.Spi;

namespace Agenix.Core.Validation;

public abstract class AbstractValidationProcessor<T> : IValidationProcessor, IGenericValidationProcessor<T>,
    IReferenceResolverAware
{
    /**
     * POCO reference resolver injected before validation callback is called
     */
    protected IReferenceResolver ReferenceResolver;

    public abstract void Validate(T payload, IDictionary<string, object> headers, TestContext context);

    /// <summary>
    ///     Sets the reference resolver for the validation processor.
    /// </summary>
    /// <param name="referenceResolver">The reference resolver to be set.</param>
    public void SetReferenceResolver(IReferenceResolver referenceResolver)
    {
        ReferenceResolver = referenceResolver;
    }

    /// <summary>
    ///     Validates the given message within the provided context.
    /// </summary>
    /// <param name="message">The message to be validated.</param>
    /// <param name="context">The test context in which the validation occurs.</param>
    public virtual void Validate(IMessage message, TestContext context)
    {
        Validate((T)message.Payload, message.GetHeaders(), context);
    }

    /// <summary>
    ///     Processes the given message by invoking the validation process.
    /// </summary>
    /// <param name="message">The message to be processed.</param>
    /// <param name="context">The context in which the message is processed.</param>
    public void Process(IMessage message, TestContext context)
    {
        Validate(message, context);
    }
}