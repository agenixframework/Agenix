#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using Newtonsoft.Json.Schema;

namespace Agenix.Validation.Json.Validation.Report;

/// <summary>
///     This class provides functionality for creating a processing report that evaluates
///     JSON schema validation results graciously. It is primarily used to handle
///     cases where multiple schemas are involved in the validation process.
/// </summary>
/// <remarks>
///     The main purpose of this report is to account for scenarios where the correct
///     matching JSON schema cannot be definitively determined. If at least one schema
///     validates the JSON message successfully without any exceptions, this report
///     will consider the validation as successful.
/// </remarks>
public class GraciousProcessingReport
{
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
    public List<ValidationError> ValidationErrors { get; } = [];

    /// Merges the current {@link GraciousProcessingReport} with the provided set of validation errors.
    /// Updates the success status and appends the validation errors to the current report.
    /// @param validationErrors the set of validation errors to merge into the existing report
    /// /
    public void MergeWith(ISet<ValidationError> validationErrors)
    {
        IsSuccess = IsSuccess || validationErrors.Count == 0;
        ValidationErrors.AddRange(validationErrors);
    }
}
