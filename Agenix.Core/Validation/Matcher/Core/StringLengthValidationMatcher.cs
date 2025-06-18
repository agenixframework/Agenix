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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Validation.Matcher;

namespace Agenix.Core.Validation.Matcher.Core;

/// <summary>
///     Represents a validation matcher that verifies whether the length of a given string field
///     matches the specified length defined in the control parameters.
/// </summary>
public class StringLengthValidationMatcher : IValidationMatcher
{
    public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
    {
        try
        {
            var control = int.Parse(controlParameters[0].Trim());
            if (value.Length != control)
            {
                throw new ValidationException(TypeDescriptor.GetClassName(typeof(StringLengthValidationMatcher))
                                              + " failed for field '" + fieldName
                                              + "'. Received value '" + value
                                              + "' should match string length '" + control + "'.");
            }
        }
        catch (Exception)
        {
            throw new ValidationException(TypeDescriptor.GetClassName(typeof(StringLengthValidationMatcher))
                                          + " failed for field '" + fieldName
                                          + "'. Invalid matcher argument '" + controlParameters[0] +
                                          "'. Must be a number");
        }
    }
}
