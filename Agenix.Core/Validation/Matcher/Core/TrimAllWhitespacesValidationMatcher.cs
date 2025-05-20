using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Validation.Matcher;

namespace Agenix.Core.Validation.Matcher.Core;

/// <summary>
/// ValidationMatcher that removes all whitespaces from the input value and control value
/// before performing validation to ensure they match.
/// </summary>
public class TrimAllWhitespacesValidationMatcher : IValidationMatcher
{
    public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
    {
        var sControl = string.Concat(controlParameters[0].Where(c => !char.IsWhiteSpace(c)));
        var sValue = string.Concat(value.Where(c => !char.IsWhiteSpace(c)));

        if (!string.Equals(sValue, sControl, StringComparison.OrdinalIgnoreCase))
            throw new ValidationException(TypeDescriptor.GetClassName(typeof(TrimValidationMatcher))
                                          + " failed for field '" + fieldName
                                          + "'. Received value is '" + sValue
                                          + "', control value is '" + sControl + "'.");
    }
}