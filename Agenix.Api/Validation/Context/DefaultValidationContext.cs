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
        if (UpdateAllowed()) Status = status;
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
