using Agenix.Api.Validation.Context;

namespace Agenix.Api.Validation;

// Delegate declaration
public delegate IValidationContext ValidationContextAdapter();

/// <summary>
///     Adapter interface marks that a class is able to act as a validation context.
/// </summary>
public interface IValidationContextAdapter
{
    /// <summary>
    ///     Adapt as a validation context
    /// </summary>
    /// <returns></returns>
    IValidationContext AsValidationContext();
}