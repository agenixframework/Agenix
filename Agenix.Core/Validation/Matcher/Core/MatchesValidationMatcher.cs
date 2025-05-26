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
            throw new ValidationException(TypeDescriptor.GetClassName(typeof(MatchesValidationMatcher))
                                          + " failed for field '" + fieldName
                                          + "'. Received value is '" + value
                                          + "', control value is '" + control + "'");
    }
}