using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FleetPay.Core.Exceptions;

namespace FleetPay.Core.Validation.Matcher.Core
{
    /// <summary>
    ///     ValidationMatcher trims leading and trailing whitespaces in value and control value.
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
}