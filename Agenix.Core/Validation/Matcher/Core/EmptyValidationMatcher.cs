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

using System.Collections.Generic;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Validation.Matcher;

namespace Agenix.Core.Validation.Matcher.Core;

/// <summary>
///     Represents a validation matcher that ensures a given value is empty.
/// </summary>
/// <remarks>
///     The EmptyValidationMatcher class implements the IValidationMatcher interface and provides a method to validate
///     whether a specific field's value is empty.
/// </remarks>
public class EmptyValidationMatcher : IValidationMatcher
{
    /// <summary>
    ///     Validates whether the provided value for the specified field is empty.
    /// </summary>
    /// <param name="fieldName">The name of the field being validated.</param>
    /// <param name="value">The value of the field to be validated.</param>
    /// <param name="controlParameters">A list of control parameters for validation.</param>
    /// <param name="context">The test context in which validation is occurring.</param>
    /// <exception cref="ValidationException">Thrown when the value is not empty.</exception>
    public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
    {
        if (value == null || !string.IsNullOrEmpty(value))
        {
            throw new ValidationException(GetType().Name
                                          + " failed for field '" + fieldName
                                          + "'. Received value '" + value
                                          + "' should be empty");
        }
    }
}
