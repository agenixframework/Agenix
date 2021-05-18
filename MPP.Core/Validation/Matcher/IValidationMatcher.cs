using System.Collections.Generic;

namespace MPP.Core.Validation.Matcher
{
    public interface IValidationMatcher
    {
        /// <summary>
        ///     Method called on validation.
        /// </summary>
        /// <param name="fieldName">the fieldName for logging purpose</param>
        /// <param name="value">the value to be validated.</param>
        /// <param name="controlParameters">the control parameters.</param>
        /// <param name="context"></param>
        void Validate(string fieldName, string value, List<string> controlParameters, TestContext context);
    }
}