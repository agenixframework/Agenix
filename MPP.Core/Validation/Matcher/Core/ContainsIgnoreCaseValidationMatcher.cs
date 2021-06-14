using System.Collections.Generic;
using System.ComponentModel;
using FleetPay.Core.Exceptions;

namespace FleetPay.Core.Validation.Matcher.Core
{
    /// <summary>
    ///     ValidationMatcher based on String.ToLower().Contains()
    /// </summary>
    public class ContainsIgnoreCaseValidationMatcher : IValidationMatcher
    {
        public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
        {
            var control = controlParameters[0];
            if (!value.ToLower().Contains(control.ToLower()))
                throw new ValidationException(TypeDescriptor.GetClassName(typeof(ContainsIgnoreCaseValidationMatcher))
                                              + " failed for field '" + fieldName
                                              + "'. Received value is '" + value
                                              + "', control value is '" + control + "'.");
        }
    }
}