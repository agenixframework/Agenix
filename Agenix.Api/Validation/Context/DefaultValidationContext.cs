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
///     Represents the default validation context.
///     This class implements the IValidationContext interface,
///     providing a basic structure for validation operations in the system.
/// </summary>
public class DefaultValidationContext : IValidationContext
{
    /// <summary>
    ///     Updates the validation status if an update is allowed according to the current status.
    /// </summary>
    /// <param name="status">the new status</param>
    public virtual void UpdateStatus(ValidationStatus status)
    {
        if (UpdateAllowed())
        {
            Status = status;
        }
    }

    /// <summary>
    ///     Gets the current validation status of the context.
    ///     The status indicates whether the validation context is in an optional, passed, failed, or unknown state.
    ///     This property reflects the state of the validation process for the associated context.
    /// </summary>
    public ValidationStatus Status { get; private set; } = ValidationStatus.UNKNOWN;

    /// <summary>
    ///     Determine whether the status update is allowed.
    ///     In case the current state is FAILED, the update is not allowed to not lose the failure state.
    /// </summary>
    /// <returns></returns>
    private bool UpdateAllowed()
    {
        return Status != ValidationStatus.FAILED;
    }
}
