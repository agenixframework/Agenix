namespace Agenix.Core.Validation.Context;

/// <summary>
///     Represents a validation context interface.
/// </summary>
public interface IValidationContext
{
    /// <summary>
    ///     Fluent builder interface.
    /// </summary>
    /// <typeparam name="T">The type of the context.</typeparam>
    /// <typeparam name="TB">The type of the builder.</typeparam>
    public interface IBuilder<out T, TB>
        where T : IValidationContext
        where TB : class
    {
        /// <summary>
        ///     Builds a new validation context instance.
        /// </summary>
        /// <returns>The built context.</returns>
        T Build();
    }
}