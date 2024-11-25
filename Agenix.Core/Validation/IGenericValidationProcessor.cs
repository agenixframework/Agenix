using System.Collections.Generic;

namespace Agenix.Core.Validation;

/// <summary>
///     Delegate for executing validation logic on a specified payload.
/// </summary>
/// <typeparam name="T">The type of the message payload.</typeparam>
/// <param name="payload">The message payload to be validated.</param>
/// <param name="headers">A dictionary containing header information related to the payload.</param>
/// <param name="context">The context in which the test is executed, providing various utilities and information.</param>
public delegate void GenericValidationProcessor<in T>(T payload, IDictionary<string, object> headers,
    TestContext context);

/// <summary>
///     Provides a contract for processing generic validation mechanisms.
///     Implementing classes should override the Validate method to specify the validation logic.
/// </summary>
/// <typeparam name="T">The type of the message payload.</typeparam>
public interface IGenericValidationProcessor<in T>
{
    /// <summary>
    ///     Subclasses do override this method for validation purposes.
    /// </summary>
    /// <param name="payload">The message payload object.</param>
    /// <param name="headers">The message headers.</param>
    /// <param name="context">The current test context.</param>
    void Validate(T payload, IDictionary<string, object> headers, TestContext context);
}