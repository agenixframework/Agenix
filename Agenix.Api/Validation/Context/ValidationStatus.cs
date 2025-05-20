namespace Agenix.Api.Validation.Context
{
    /// <summary>
    /// Status that marks that the validation results for a specific validation context.
    /// The validation context keeps track of its state to identify passed, failed or unknown state.
    /// Message validators update the status on the validation context after processing with the outcome of the validation.
    /// This way we can track if a validation context has been processed during validation.
    /// </summary>
    public enum ValidationStatus
    {
        OPTIONAL,
        PASSED,
        FAILED,
        UNKNOWN
    }
}