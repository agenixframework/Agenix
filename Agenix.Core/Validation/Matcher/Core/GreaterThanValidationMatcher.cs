using System;
using System.Collections.Generic;
using System.ComponentModel;
using Agenix.Core.Exceptions;
using static System.Double;

namespace Agenix.Core.Validation.Matcher.Core;

/// <summary>
///     ValidationMatcher based on Double > Double.
/// </summary>
public class GreaterThanValidationMatcher : IValidationMatcher
{
    public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
    {
        var control = controlParameters[0];
        double dValue;
        double dControl;

        try
        {
            dValue = Parse(value);
            dControl = Parse(control);
        }
        catch (Exception e)
        {
            throw new ValidationException(TypeDescriptor.GetClassName(typeof(GreaterThanValidationMatcher))
                                          + " failed for field '" + fieldName
                                          + "'. Received value is '" + value
                                          + "', control value is '" + control + "'", e);
        }

        if (!(dValue > dControl))
            throw new ValidationException(TypeDescriptor.GetClassName(typeof(GreaterThanValidationMatcher))
                                          + " failed for field '" + fieldName
                                          + "'. Received value is '" + value
                                          + "', control value is '" + control + "'");
    }
}