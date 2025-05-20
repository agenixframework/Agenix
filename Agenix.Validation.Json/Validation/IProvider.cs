using Agenix.Api.Context;
using Agenix.Api.Validation.Context;

namespace Agenix.Validation.Json.Validation;

/// <summary>
///     Defines a provider interface for creating instances of JsonElementValidator.
/// </summary>
public interface IProvider
{
    /// <summary>
    ///     Creates and returns a new instance of JsonElementValidator based on the provided parameters.
    /// </summary>
    /// <param name="isStrict">Determines if validation should be strict (true) or lenient (false).</param>
    /// <param name="context">The test context which provides necessary data and utilities for validation.</param>
    /// <param name="validationContext">The validation context specific to JSON message validation.</param>
    /// <returns>A JsonElementValidator configured with the specified parameters.</returns>
    JsonElementValidator GetValidator(bool isStrict, TestContext context,
        IMessageValidationContext validationContext);
}