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
///     Represents a validation context interface.
/// </summary>
public interface IValidationContext
{
    /// <summary>
    ///     Indicates whether this validation context requires a validator.
    /// </summary>
    /// <returns>true if a validator is required; false otherwise.</returns>
    public bool RequiresValidator => false;

    /// <summary>
    ///     Retrieves the current validation status for this validation context.
    /// </summary>
    /// <returns>The current validation status of the context.</returns>
    ValidationStatus Status => ValidationStatus.UNKNOWN;

    /// <summary>
    ///     Updates the validation status for the current validation context.
    /// </summary>
    /// <param name="status">The new validation status to be applied to the context.</param>
    void UpdateStatus(ValidationStatus status);

    /// <summary>
    ///     Fluent builder interface.
    /// </summary>
    /// <typeparam name="T">The type of the context.</typeparam>
    /// <typeparam name="TB">The type of the builder.</typeparam>
    public interface IBuilder<out T, TB> : IBuilder
        where T : IValidationContext
        where TB : IBuilder
    {
        /// <summary>
        ///     Builds a new validation context instance.
        /// </summary>
        /// <returns>The built context.</returns>
        T Build();
    }
}
