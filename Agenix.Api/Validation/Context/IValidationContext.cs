namespace Agenix.Api.Validation.Context;

/// <summary>
///     Represents a validation context interface.
/// </summary>
public interface IValidationContext
{
    /// <summary>
    /// Indicates whether this validation context requires a validator.
    /// </summary>
    /// <returns>true if a validator is required; false otherwise.</returns>
    public bool RequiresValidator => false;

    /// <summary>
    /// Updates the validation status for the current validation context.
    /// </summary>
    /// <param name="status">The new validation status to be applied to the context.</param>
    void UpdateStatus(ValidationStatus status);

    /// <summary>
    /// Retrieves the current validation status for this validation context.
    /// </summary>
    /// <returns>The current validation status of the context.</returns>
    ValidationStatus Status => ValidationStatus.UNKNOWN;

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