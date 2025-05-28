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

namespace Agenix.Api.Validation.Context;

/// <summary>
///     Represents a context for validating headers within a given scope.
///     It maintains a list of header validators and provides options to add validators
///     and their references. This context allows for case-insensitive header name comparisons.
/// </summary>
public class HeaderValidationContext : IValidationContext
{
    /// <summary>
    ///     Should header name validation ignore case sensitivity?
    /// </summary>
    private bool _headerNameIgnoreCase;

    /// <summary>
    ///     Represents the current validation status of the header validation context.
    ///     Tracks if the validation context is in an optional, passed, failed, or unknown state.
    /// </summary>
    private ValidationStatus _status = ValidationStatus.UNKNOWN;

    /// <summary>
    ///     List of special header validator references.
    /// </summary>
    private List<string> _validatorNames = [];

    /// <summary>
    ///     List of special header validators.
    /// </summary>
    private List<IHeaderValidator> _validators = [];

    public HeaderValidationContext() : this(new Builder())
    {
    }

    public HeaderValidationContext(Builder builder)
    {
        _validators = builder.HeaderValidators;
        _validatorNames = builder.HeaderValidatorNames;
        _headerNameIgnoreCase = builder.IsHeaderNameCaseInsensitive;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether header name validation should ignore case sensitivity.
    /// </summary>
    public bool HeaderNameIgnoreCase
    {
        get => _headerNameIgnoreCase;
        set => _headerNameIgnoreCase = value;
    }

    /// <summary>
    ///     Gets the list of validators.
    /// </summary>
    public List<IHeaderValidator> Validators
    {
        get => _validators;
        set => _validators = value;
    }

    /// <summary>
    ///     Gets the list of validator names.
    /// </summary>
    public List<string> ValidatorNames
    {
        get => _validatorNames;
        set => _validatorNames = value;
    }

    /// <summary>
    ///     Updates the validation status if the new status is not <see cref="ValidationStatus.FAILED" />.
    /// </summary>
    /// <param name="status">The new validation status to set.</param>
    public void UpdateStatus(ValidationStatus status)
    {
        if (status != ValidationStatus.FAILED) _status = status;
    }

    /// <summary>
    ///     Represents the current validation status within the header validation context.
    ///     The status indicates whether the validation passed, failed, is optional, or remains unknown.
    /// </summary>
    public ValidationStatus Status => _status;

    /// <summary>
    ///     Adds a header validator.
    /// </summary>
    /// <param name="validator">The header validator to add.</param>
    public void AddHeaderValidator(IHeaderValidator validator)
    {
        _validators.Add(validator);
    }

    /// <summary>
    ///     Adds a header validator reference.
    /// </summary>
    /// <param name="validatorName">The name of the header validator references to add.</param>
    public void AddHeaderValidator(string validatorName)
    {
        _validatorNames.Add(validatorName);
    }

    public sealed class Builder : IValidationContext.IBuilder<HeaderValidationContext, Builder>
    {
        // List of special header validator references
        internal readonly List<string> HeaderValidatorNames = [];

        // List of special header validators
        internal readonly List<IHeaderValidator> HeaderValidators = [];

        // Should header name validation ignore case sensitivity?
        internal bool IsHeaderNameCaseInsensitive;

        /// <summary>
        ///     Builds and returns a new instance of the HeaderValidationContext.
        /// </summary>
        /// <returns>A new instance of the HeaderValidationContext.</returns>
        public HeaderValidationContext Build()
        {
            return new HeaderValidationContext(this);
        }

        /// <summary>
        ///     Sets the headerNameIgnoreCase.
        /// </summary>
        public Builder IgnoreCase(bool headerNameIgnoreCase)
        {
            IsHeaderNameCaseInsensitive = headerNameIgnoreCase;
            return this;
        }

        /// <summary>
        ///     Adds header validator.
        /// </summary>
        public Builder Validator(IHeaderValidator validator)
        {
            HeaderValidators.Add(validator);
            return this;
        }

        /// <summary>
        ///     Adds header validator reference.
        /// </summary>
        public Builder Validator(string validatorName)
        {
            HeaderValidatorNames.Add(validatorName);
            return this;
        }

        /// <summary>
        ///     Sets the validators.
        /// </summary>
        public Builder Validators(List<IHeaderValidator> newValidators)
        {
            HeaderValidators.AddRange(newValidators);
            return this;
        }

        /// <summary>
        ///     Sets the validatorNames.
        /// </summary>
        public Builder ValidatorNames(List<string> newValidatorNames)
        {
            HeaderValidatorNames.AddRange(newValidatorNames);
            return this;
        }
    }
}
