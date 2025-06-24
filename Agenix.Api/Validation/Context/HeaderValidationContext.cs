#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
// 
// Copyright (c) 2025 Agenix
// 
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

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
        if (status != ValidationStatus.FAILED)
        {
            _status = status;
        }
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
