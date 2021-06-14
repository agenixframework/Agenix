using System;
using System.Collections.Generic;
using System.ComponentModel;
using FleetPay.Core.Exceptions;

namespace FleetPay.Core.Validation.Matcher.Core
{
    /// <summary>
    ///     ValidationMatcher checks string length of given field.
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
}