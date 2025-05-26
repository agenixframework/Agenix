using System;
using System.Collections.Generic;
using System.ComponentModel;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Validation.Matcher;

namespace Agenix.Core.Validation.Matcher.Core;

/// <summary>
///     Performs validation by trimming leading and trailing whitespaces from both the provided value and the control value
///     for comparison.
///     Throws a ValidationException if the trimmed values do not match.
/// </summary>
public class TrimValidationMatcher : IValidationMatcher
{
    public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
    {
        var control = controlParameters[0];

        if (!string.Equals(value.Trim(), control.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new ValidationException(TypeDescriptor.GetClassName(typeof(TrimValidationMatcher))
                                          + " failed for field '" + fieldName
                                          + "'. Received value is '" + value.Trim()
                                          + "', control value is '" + control.Trim() + "'.");
    }
}