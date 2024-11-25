using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Agenix.Core.Exceptions;
using Agenix.Core.Validation.Matcher;

namespace Agenix.Core.Validation;

/// Provides utility methods for validating values in various contexts.
public abstract class ValidationUtils
{
    private static readonly IDictionary<string, IValueMatcher> DefaultValueMatchers = IValueMatcher.Lookup();

    /// Validates whether the actual value matches the expected value based on specific criteria.
    /// <param name="actualValue">The actual value obtained from the operation.</param>
    /// <param name="expectedValue">The value that is expected to match the actual value.</param>
    /// <param name="pathExpression">The path expression indicating the data path being validated.</param>
    /// <param name="context">The context within which the validation is being performed, providing necessary utilities.</param>
    /// <exception cref="ValidationException">Thrown when the actual value does not match the expected value.</exception>
    public static void ValidateValues(object actualValue, object expectedValue, string pathExpression,
        TestContext context)
    {
        if (actualValue != null)
        {
            if (expectedValue == null)
                throw new ValidationException(BuildValueMismatchErrorMessage(
                    "Values not equal for element '" + pathExpression + "'", null, actualValue));

            var matcher = GetValueMatcher(expectedValue, context);

            if (matcher != null)
            {
                if (!matcher.Validate(actualValue, expectedValue, context))
                    throw new ValidationException(BuildValueMismatchErrorMessage(
                        "Values not matching for element '" + pathExpression + "'", expectedValue, actualValue));
                return;
            }

            if (expectedValue is not string)
            {
                var converted = context.TypeConverter.ConvertIfNecessary<object>(actualValue, expectedValue.GetType());

                if (converted == null)
                    throw new CoreSystemException(
                        $"Failed to convert value '{actualValue}' to required type '{expectedValue.GetType()}'");

                if (IsList(converted))
                {
                    if (!converted.ToString()!.Equals(expectedValue.ToString()))
                        throw new ValidationException(BuildValueMismatchErrorMessage(
                            "Values not equal for element '" + pathExpression + "'", expectedValue.ToString(),
                            converted.ToString()));
                }
                else if (converted is string[] convertedArray && expectedValue is string[] expectedArray)
                {
                    var convertedDelimitedString = string.Join(",", convertedArray);
                    var expectedDelimitedString = string.Join(",", expectedArray);

                    if (convertedDelimitedString != expectedDelimitedString)
                        throw new ValidationException(BuildValueMismatchErrorMessage(
                            $"Values not equal for element '{pathExpression}'", expectedDelimitedString,
                            convertedDelimitedString));
                }
                else if (converted is byte[] convertedBytes && expectedValue is byte[] expectedBytes)
                {
                    var convertedBase64 = Convert.ToBase64String(convertedBytes);
                    var expectedBase64 = Convert.ToBase64String(expectedBytes);

                    if (convertedBase64 != expectedBase64)
                        throw new ValidationException(BuildValueMismatchErrorMessage(
                            $"Values not equal for element '{pathExpression}'", expectedBase64, convertedBase64));
                }
                else if (!converted.Equals(expectedValue))
                {
                    throw new ValidationException(BuildValueMismatchErrorMessage(
                        "Values not equal for element '" + pathExpression + "'", expectedValue, converted));
                }
            }
            else
            {
                var expectedValueString = expectedValue.ToString();
                string actualValueString;

                if (typeof(IEnumerable<object>).IsAssignableFrom(actualValue.GetType()))
                {
                    actualValueString =
                        Regex.Replace(string.Join(",", ((IEnumerable<object>)actualValue).Select(x => x.ToString())),
                            @"^\[|\]$", "").Replace("\r\n", string.Empty).Replace(" ", "").Trim();
                    expectedValueString = Regex.Replace(expectedValueString, @"^\[|\]$", "").Replace(" ", "").Trim();
                }
                else
                {
                    actualValueString = actualValue.ToString();
                }

                if (ValidationMatcherUtils.IsValidationMatcherExpression(expectedValueString))
                    ValidationMatcherUtils.ResolveValidationMatcher(pathExpression, actualValueString,
                        expectedValueString, context);
                else if (actualValueString != expectedValueString)
                    throw new ValidationException(BuildValueMismatchErrorMessage(
                        $"Values not equal for element '{pathExpression}'", expectedValueString, actualValueString));
            }
        }
        else if (expectedValue != null)
        {
            var matcher = GetValueMatcher(expectedValue, context);

            if (matcher != null)
            {
                if (!matcher.Validate(actualValue, expectedValue, context))
                    throw new ValidationException(BuildValueMismatchErrorMessage(
                        "Values not matching for element '" + pathExpression + "'", expectedValue, actualValue));
            }
            else if (expectedValue is string)
            {
                var expectedValueString = expectedValue.ToString();

                if (ValidationMatcherUtils.IsValidationMatcherExpression(expectedValueString))
                    ValidationMatcherUtils.ResolveValidationMatcher(pathExpression,
                        null,
                        expectedValueString,
                        context);
                else if (!string.IsNullOrWhiteSpace(expectedValueString))
                    throw new ValidationException(BuildValueMismatchErrorMessage(
                        "Values not equal for element '" + pathExpression + "'", expectedValueString, null));
            }
            else
            {
                throw new ValidationException("Validation failed: " + BuildValueMismatchErrorMessage(
                    "Values not equal for element '" + pathExpression + "'", expectedValue, null));
            }
        }
    }

    /// Determines whether the provided object is a generic list.
    /// <param name="obj">The object to be checked.</param>
    /// <return>True if the object is a generic list, otherwise false.</return>
    private static bool IsList(object obj)
    {
        if (obj == null) return false;

        var type = obj.GetType();
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
    }


    /// Constructs proper error message for a value that should be in a collection.
    /// @param baseMessage the base error message.
    /// @param controlValue the expected value.
    /// @param actualValues where the controlValue should be present in.
    /// @return
    /// /
    public static string BuildValueMismatchErrorMessage(string baseMessage, object controlValue, object actualValue)
    {
        return baseMessage + ", expected '" + controlValue + "' but was '" + actualValue + "'";
    }

    /// Constructs proper error message for a value that should be in a collection.
    /// @param baseMessage the base error message.
    /// @param controlValue the expected value.
    /// @param actualValues where the controlValue should be present in.
    /// @return
    /// /
    public static string BuildValueToBeInCollectionErrorMessage(string baseMessage, object controlValue,
        ICollection<object> actualValues)
    {
        return baseMessage + ", expected '" + controlValue + "' to be in '" + actualValues + "'";
    }

    /// Builds an error message indicating that a specific control value was expected to be found in a collection of actual values.
    /// <param name="baseMessage">The base message to which the detailed error information will be appended.</param>
    /// <param name="controlValue">The control value that was expected to be present in the collection.</param>
    /// <param name="actualValues">The collection of actual values where the control value was expected to be found.</param>
    /// <return>A string representing the constructed error message.</return>
    public static string BuildValueToBeInCollectionErrorMessage(string baseMessage, object controlValue,
        ICollection<string> actualValues)
    {
        return baseMessage + ", expected '" + controlValue + "' to be in '" + actualValues + "'";
    }

    /// Returns an appropriate IValueMatcher for the given expected value within the provided context.
    /// <param name="expectedValue">The expected value for which a suitable value matcher is needed.</param>
    /// <param name="context">The context within which the value matching will be performed.</param>
    /// <returns>An instance of IValueMatcher that supports the type of the expected value, or null if no match is found.</returns>
    private static IValueMatcher GetValueMatcher(object expectedValue, TestContext context)
    {
        // Add validators from resource path lookup
        var allMatchers = new Dictionary<string, IValueMatcher>(DefaultValueMatchers);

        // Find the first matcher that supports the expected value's type
        return allMatchers.Values
            .FirstOrDefault(matcher => matcher.Supports(expectedValue.GetType()));
    }
}