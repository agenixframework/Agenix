using System.Collections.Generic;
using System.ComponentModel;
using Agenix.Core.Exceptions;

namespace Agenix.Core.Validation.Matcher.Core
{
    /// <summary>
    ///     ValidationMatcher based on String.Contains()
    /// </summary>
    public class ContainsValidationMatcher : IValidationMatcher
    {
        public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
        {
            var control = controlParameters[0];
            if (value == null || !value.Contains(control))
                throw new ValidationException(TypeDescriptor.GetClassName(typeof(ContainsValidationMatcher))
                                              + " failed for field '" + fieldName
                                              + "'. Received value is '" + value
                                              + "', must contain '" + control + "'.");
        }
    }
}