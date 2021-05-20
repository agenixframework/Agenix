using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using MPP.Core.Exceptions;

namespace MPP.Core.Validation.Matcher.Core
{
    /// <summary>
    ///     ValidationMatcher ignores all new line characters in value and control value.
    /// </summary>
    public class IgnoreNewLineValidationMatcher : IValidationMatcher

    {
        public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
        {
            var control = controlParameters[0];

            var normalizedValue = Regex.Replace(value, "\\r(\\n)?", "\n");
            normalizedValue = Regex.Replace(normalizedValue, "\\n", "");
            var normalizedControl = Regex.Replace(control, "\\r(\\n)?", "\n");
            normalizedControl = Regex.Replace(normalizedControl, "\\n", "");

            if (!string.Equals(normalizedValue, normalizedControl, StringComparison.OrdinalIgnoreCase))
                throw new ValidationException(TypeDescriptor.GetClassName(typeof(IgnoreNewLineValidationMatcher))
                                              + " failed for field '" + fieldName
                                              + "'. Received value is '" + normalizedValue
                                              + "', control value is '" + normalizedControl + "'.");
        }
    }
}