namespace Agenix.Api.Validation.Context;

/// <summary>
///     Represents the default validation context.
///     This class implements the IValidationContext interface,
///     providing a basic structure for validation operations in the system.
/// </summary>
public class DefaultValidationContext : IValidationContext
{
    /// <summary>
    /// Updates the validation status if an update is allowed according to the current status.
    /// </summary>
    /// <param name="status">the new status</param>
    public virtual void UpdateStatus(ValidationStatus status)
    {
        if (UpdateAllowed()) {
            Status = status;
        }
    }
    
    /// <summary>
    /// Determine whether the status update is allowed.
    /// In case the current state is FAILED, the update is not allowed to not lose the failure state.
    /// </summary>
    /// <returns></returns>
    private bool UpdateAllowed() {
        return Status != ValidationStatus.FAILED;
    }

    /// <summary>
    /// Gets the current validation status of the context.
    /// The status indicates whether the validation context is in an optional, passed, failed, or unknown state.
    /// This property reflects the state of the validation process for the associated context.
    /// </summary>
    public ValidationStatus Status { get; private set; } = ValidationStatus.UNKNOWN;
}