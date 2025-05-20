using Newtonsoft.Json.Schema;

namespace Agenix.Validation.Json.Validation.Report;

/// <summary>
/// This class provides functionality for creating a processing report that evaluates
/// JSON schema validation results graciously. It is primarily used to handle
/// cases where multiple schemas are involved in the validation process.
/// </summary>
/// <remarks>
/// The main purpose of this report is to account for scenarios where the correct
/// matching JSON schema cannot be definitively determined. If at least one schema
/// validates the JSON message successfully without any exceptions, this report
/// will consider the validation as successful.
/// </remarks>
public class GraciousProcessingReport
{
    private readonly List<ValidationError> _validationErrors = [];

    /// Represents a processing report designed for interpreting JSON schema validation results in a gracious manner.
    /// It considers the validation successful if at least one schema validates without throwing exceptions.
    /// /
    public GraciousProcessingReport() : this(false)
    {
    }

    /// Represents a processing report designed for interpreting JSON schema validation results in a gracious manner.
    /// It considers the validation successful if at least one schema validates without throwing exceptions.
    /// /
    public GraciousProcessingReport(bool success)
    {
        IsSuccess = success;
    }

    /// Represents a processing report designed for graciously interpreting JSON schema validation results.
    /// If at least one schema validates successfully without exceptions, the validation is considered successful.
    /// /
    public GraciousProcessingReport(ISet<ValidationError> validationErrors) : this(false)
    {
        MergeWith(validationErrors);
    }

    /// Determines whether the current state of {@link GraciousProcessingReport} represents a successful validation.
    /// <returns>A boolean value indicating whether the report is in a successful state.</returns>
    public bool IsSuccess { get; set; }
    
    /// Gets the list of validation errors associated with the report.
    /// This property retrieves the collection of validation errors
    /// encountered while processing JSON schema validations.
    public List<ValidationError> ValidationErrors => _validationErrors;

    /// Merges the current {@link GraciousProcessingReport} with the provided set of validation errors.
    /// Updates the success status and appends the validation errors to the current report.
    /// @param validationErrors the set of validation errors to merge into the existing report
    /// /
    public void MergeWith(ISet<ValidationError> validationErrors)
    {
        IsSuccess = IsSuccess || validationErrors.Count == 0;
        _validationErrors.AddRange(validationErrors);
    }
}