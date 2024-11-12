using Agenix.Core.Validation.Context;

namespace Agenix.Core.Validation;

// Delegate declaration
public delegate IValidationContext ValidationContextAdapter();

/// <summary>
///     Adapter interface marks that a class is able to act as a validation context.
/// </summary>
public interface IValidationContextAdapter
{
    /// <summary>
    ///     Adapt as validation context
    /// </summary>
    /// <returns></returns>
    IValidationContext AsValidationContext();
}