using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using MPP.Core.Exceptions;

namespace MPP.Core.Validation.Matcher.Core
{
    /// <summary>
    ///     ValidationMatcher checking for valid date format.
    /// </summary>
    public class DatePatternValidationMatcher : IValidationMatcher
    {
        public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
        {
            var control = controlParameters[0];
            bool isValidDate;
            try
            {
                isValidDate = DateTime.TryParseExact(value, control, DateTimeFormatInfo.InvariantInfo,
                    DateTimeStyles.None, out _);
            }
            catch (ArgumentException e)
            {
                throw new ValidationException(TypeDescriptor.GetClassName(typeof(DatePatternValidationMatcher))
                                              + " failed for field '" + fieldName + "' " +
                                              ". Found invalid date format", e);
            }

            if (!isValidDate)
                throw new ValidationException(TypeDescriptor.GetClassName(typeof(DatePatternValidationMatcher))
                                              + " failed for field '" + fieldName + "'" +
                                              ". Received invalid date format for value '" + value
                                              + "', expected date format is '" + control + "'");
        }
    }
}