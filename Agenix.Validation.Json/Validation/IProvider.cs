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

using Agenix.Api.Context;
using Agenix.Api.Validation.Context;

namespace Agenix.Validation.Json.Validation;

/// <summary>
///     Defines a provider interface for creating instances of JsonElementValidator.
/// </summary>
public interface IProvider
{
    /// <summary>
    ///     Creates and returns a new instance of JsonElementValidator based on the provided parameters.
    /// </summary>
    /// <param name="isStrict">Determines if validation should be strict (true) or lenient (false).</param>
    /// <param name="context">The test context which provides necessary data and utilities for validation.</param>
    /// <param name="validationContext">The validation context specific to JSON message validation.</param>
    /// <returns>A JsonElementValidator configured with the specified parameters.</returns>
    JsonElementValidator GetValidator(bool isStrict, TestContext context,
        IMessageValidationContext validationContext);
}
