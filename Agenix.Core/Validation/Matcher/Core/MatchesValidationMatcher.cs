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
using System.Text.RegularExpressions;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Validation.Matcher;

namespace Agenix.Core.Validation.Matcher.Core;

/// Represents a validation matcher that uses a regular expression pattern
/// to validate string values against a control regex pattern.
/// </summary>
/// <remarks>
///     This class implements the <see cref="IValidationMatcher" /> interface and
///     uses the <c>Regex.IsMatch</c> method to determine if a given input matches
///     a provided regular expression pattern. If the pattern is invalid or the
///     input does not conform to the pattern, validation exceptions are thrown.
/// </remarks>
/// <exception cref="ValidationException">
///     Thrown when the input does not match the control regex pattern or if the pattern syntax is invalid.
/// </exception>
public class MatchesValidationMatcher : IValidationMatcher
{
    public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
    {
        var control = controlParameters[0];
        bool success;

        try
        {
            success = Regex.IsMatch(value, control);
        }
        catch (Exception e)
        {
            throw new ValidationException(TypeDescriptor.GetClassName(typeof(MatchesValidationMatcher))
                                          + " failed for field '" + fieldName
                                          + "'. Found invalid pattern syntax", e);
        }

        if (!success)
        {
            throw new ValidationException(TypeDescriptor.GetClassName(typeof(MatchesValidationMatcher))
                                          + " failed for field '" + fieldName
                                          + "'. Received value is '" + value
                                          + "', control value is '" + control + "'");
        }
    }
}
