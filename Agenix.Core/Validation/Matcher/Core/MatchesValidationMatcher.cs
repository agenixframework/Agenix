using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Agenix.Core.Validation.Matcher;
using Agenix.Core.Exceptions;

namespace Agenix.Core.Validation.Matcher.Core
{
    /// <summary>
    ///     ValidationMatcher based on Regex.IsMatch()
    /// </summary>
    public class MatchesValidationMatcher : IValidationMatcher
    {
        public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
        {
            var control = controlParameters[0];
            bool success;

            try
            {
                success = Regex.IsMatch(value, control);
            }
            catch (Exception e)
            {
                throw new ValidationException(TypeDescriptor.GetClassName(typeof(MatchesValidationMatcher))
                                              + " failed for field '" + fieldName
                                              + "'. Found invalid pattern syntax", e);
            }

            if (!success)
                throw new ValidationException(TypeDescriptor.GetClassName(typeof(MatchesValidationMatcher))
                                              + " failed for field '" + fieldName
                                              + "'. Received value is '" + value
                                              + "', control value is '" + control + "'");
        }
    }
}