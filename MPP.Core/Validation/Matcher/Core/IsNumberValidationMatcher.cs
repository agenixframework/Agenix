using System;
using System.Collections.Generic;
using System.ComponentModel;
using MPP.Core.Exceptions;
using static System.Double;

namespace MPP.Core.Validation.Matcher.Core
{
    public class IsNumberValidationMatcher : IValidationMatcher
    {
        public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
        {
            try
            {
                Parse(value);
            }
            catch (Exception e)
            {
                throw new ValidationException(TypeDescriptor.GetClassName(typeof(GreaterThanValidationMatcher))
                                              + " failed for field '" + fieldName
                                              + "'. Received value is '" + value
                                              + "', and is not a number", e);
            }
        }
    }
}