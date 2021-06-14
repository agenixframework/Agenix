using System;
using System.Collections.Generic;
using System.ComponentModel;
using FleetPay.Core.Exceptions;

namespace FleetPay.Core.Validation.Matcher.Core
{
    /// <summary>
    ///     ValidationMatcher based on string.Equals(value, control, StringComparison.OrdinalIgnoreCase)
    /// </summary>
    public class EqualsIgnoreCaseValidationMatcher : IValidationMatcher
    {
        public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
        {
            var control = controlParameters[0];

            if (!string.Equals(value, control, StringComparison.OrdinalIgnoreCase))
                throw new ValidationException(TypeDescriptor.GetClassName(typeof(EqualsIgnoreCaseValidationMatcher))
                                              + " failed for field '" + fieldName
                                              + "'. Received value is '" + value
                                              + "', control value is '" + control + "'.");
        }
    }
}