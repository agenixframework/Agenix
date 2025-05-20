using System;
using Agenix.Api.Validation.Context;

namespace Agenix.Core.Message.Builder;

/// <summary>
///     A builder class for creating instances of <see cref="IValidationContext" /> using a provided factory function.
/// </summary>
/// <typeparam name="TBuilder">
///     The type of the builder implementing the <see cref="IValidationContext.IBuilder{T, TB}" />
///     interface.
/// </typeparam>
public class FuncValidationContextBuilder<TBuilder>(Func<IValidationContext> validationContextFactory)
    : IValidationContext.IBuilder<IValidationContext, TBuilder>
    where TBuilder : class
{
    /// <summary>
    ///     Builds and returns an instance of <see cref="IValidationContext" />.
    /// </summary>
    /// <returns>An instance of <see cref="IValidationContext" /> created by the factory.</returns>
    public IValidationContext Build()
    {
        return validationContextFactory();
    }
}