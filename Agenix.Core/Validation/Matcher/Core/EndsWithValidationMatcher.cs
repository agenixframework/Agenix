using System.Collections.Generic;
using System.ComponentModel;
using Agenix.Core.Exceptions;

namespace Agenix.Core.Validation.Matcher.Core;

/// <summary>
///     ValidationMatcher checking for valid date format.
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