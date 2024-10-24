using System.Collections.Generic;
using System.ComponentModel;
using Agenix.Core.Validation.Matcher;
using Agenix.Core.Exceptions;

namespace Agenix.Core.Validation.Matcher.Core
{
    /// <summary>
    ///     ValidationMatcher based on String.StartsWith()
    /// </summary>
    public class StartsWithValidationMatcher : IValidationMatcher
    {
        public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
        {
            var control = controlParameters[0];
            if (!value.StartsWith(control))
                throw new ValidationException(TypeDescriptor.GetClassName(typeof(StartsWithValidationMatcher))
                                              + " failed for field '" + fieldName
                                              + "'. Received value is '" + value
                                              + "', control value is '" + control + "'");
        }
    }
}