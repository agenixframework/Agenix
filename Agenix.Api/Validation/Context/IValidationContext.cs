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
