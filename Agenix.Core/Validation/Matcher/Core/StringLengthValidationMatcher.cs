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
                throw new ValidationException(TypeDescriptor.GetClassName(typeof(StringLengthValidationMatcher))
                                              + " failed for field '" + fieldName
                                              + "'. Received value '" + value
                                              + "' should match string length '" + control + "'.");
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