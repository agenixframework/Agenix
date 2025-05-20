using System.Collections.Generic;
using System.ComponentModel;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Validation.Matcher;

namespace Agenix.Core.Validation.Matcher.Core;

/// <summary>
///     ValidationMatcher checking for a valid date format.
/// </summary>
public class EndsWithValidationMatcher : IValidationMatcher
{
    public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
    {
        var controlValue = controlParameters[0];

        if (!value.EndsWith(controlValue))
            throw new ValidationException(TypeDescriptor.GetClassName(typeof(EndsWithValidationMatcher))
                                          + " failed for field '" + fieldName
                                          + "'. Received value is '" + value
                                          + "', control value is '" + controlValue + "'.");
    }
}