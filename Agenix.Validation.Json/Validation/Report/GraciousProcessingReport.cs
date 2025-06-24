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
