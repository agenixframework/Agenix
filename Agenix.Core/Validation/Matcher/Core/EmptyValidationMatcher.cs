using System.Collections.Generic;
using Agenix.Core.Exceptions;

namespace Agenix.Core.Validation.Matcher.Core;

/// <summary>
///     Represents a validation matcher that ensures a given value is empty.
/// </summary>
/// <remarks>
///     The EmptyValidationMatcher class implements the IValidationMatcher interface and provides a method to validate
///     whether a specific field's value is empty.
/// </remarks>
public class EmptyValidationMatcher : IValidationMatcher
{
    /// <summary>
    ///     Validates whether the provided value for the specified field is empty.
    /// </summary>
    /// <param name="fieldName">The name of the field being validated.</param>
    /// <param name="value">The value of the field to be validated.</param>
    /// <param name="controlParameters">A list of control parameters for validation.</param>
    /// <param name="context">The test context in which validation is occurring.</param>
    /// <exception cref="ValidationException">Thrown when the value is not empty.</exception>
    public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
    {
        if (value == null || !string.IsNullOrEmpty(value))
            throw new ValidationException(GetType().Name
                                          + " failed for field '" + fieldName
                                          + "'. Received value '" + value
                                          + "' should be empty");
    }
}