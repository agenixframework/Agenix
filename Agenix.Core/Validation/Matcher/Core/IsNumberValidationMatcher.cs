using System;
using System.Collections.Generic;
using System.ComponentModel;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Validation.Matcher;
using static System.Double;

namespace Agenix.Core.Validation.Matcher.Core;

/// <summary>
/// Represents a validation matcher that checks whether a provided value is a valid number.
/// </summary>
/// <remarks>
/// This class implements the <see cref="IValidationMatcher"/> interface and is registered in
/// the default validation matcher library under the member name "IsNumber". The primary purpose
/// of this matcher is to ensure that a given value is a valid numeric format.
/// </remarks>
/// <example>
/// To use this matcher, ensure it has been registered in the validation matcher library
/// and provide it with the necessary field, value, and context during validation.
/// </example>
/// <exception cref="ValidationException">
/// Thrown when the provided value fails to parse as a number.
/// The exception message will include the specific field name and the invalid value.
/// </exception>
/// <seealso cref="IValidationMatcher"/>
public class IsNumberValidationMatcher : IValidationMatcher
{
    public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
    {
        try
        {
            var d = Parse(value);
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