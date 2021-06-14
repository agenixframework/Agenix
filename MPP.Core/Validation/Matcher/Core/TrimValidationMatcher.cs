using System;
using System.Collections.Generic;
using System.ComponentModel;
using FleetPay.Core.Exceptions;

namespace FleetPay.Core.Validation.Matcher.Core
{
    /// <summary>
    ///     ValidationMatcher trims leading and trailing whitespaces in value and control value.
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
}