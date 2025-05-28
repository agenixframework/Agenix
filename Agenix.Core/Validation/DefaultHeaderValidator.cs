#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Api.Validation.Matcher;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Validation;

/// DefaultHeaderValidator provides the mechanism to validate HTTP headers against
/// expected control values within a given test context.
/// /
public class DefaultHeaderValidator : IHeaderValidator
{
    /// <summary>
    ///     Logger instance for recording debug, informational, warning, and error messages
    ///     within the DefaultHeaderValidator class. Utilized primarily for debugging and
    ///     tracking header validation processes.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(IHeaderValidator));

    /// <summary>
    ///     Set of default header validators located via resource path lookup.
    /// </summary>
    public static readonly IDictionary<string, IHeaderValidator> DefaultValidators = IHeaderValidator.Lookup();

    /// <summary>
    ///     Validates that the received header value matches the expected control value.
    /// </summary>
    /// <param name="headerName">The name of the header to validate.</param>
    /// <param name="receivedValue">The actual value received for this header.</param>
    /// <param name="controlValue">The expected value to validate against.</param>
    /// <param name="context">Context providing test-related information and functionality.</param>
    /// <param name="validationContext">Additional context for the validation process.</param>
    /// <exception cref="ValidationException">Thrown when the validation fails.</exception>
    public void ValidateHeader(string headerName, object receivedValue, object controlValue, TestContext context,
        HeaderValidationContext validationContext)
    {
        var validator = GetHeaderValidator(headerName, controlValue, context);
        if (validator != null)
        {
            try
            {
                validator.ValidateHeader(headerName, receivedValue!, controlValue!, context, validationContext);
                validationContext.UpdateStatus(ValidationStatus.PASSED);
            }
            catch (ValidationException)
            {
                validationContext.UpdateStatus(ValidationStatus.FAILED);
                throw;
            }

            return;
        }

        try
        {
            var expectedValue = controlValue != null
                ? context.ReplaceDynamicContentInString(
                    context.TypeConverter.ConvertIfNecessary<string>(controlValue, typeof(string)))
                : string.Empty;

            if (receivedValue != null)
            {
                var receivedValueString =
                    context.TypeConverter.ConvertIfNecessary<string>(receivedValue, typeof(string));
                if (ValidationMatcherUtils.IsValidationMatcherExpression(expectedValue))
                {
                    ValidationMatcherUtils.ResolveValidationMatcher(headerName, receivedValueString,
                        expectedValue, context);
                    return;
                }

                if (!receivedValueString.Equals(expectedValue))
                    throw new ValidationException(
                        $"Values not equal for header element '{headerName}', expected '{expectedValue}' but was '{receivedValue}'");
            }
            else if (!string.IsNullOrWhiteSpace(expectedValue))
            {
                throw new ValidationException(
                    $"Values not equal for header element '{headerName}', expected '{expectedValue}' but was 'null'");
            }

            Log.LogDebug($"Validating header element: {headerName}='{expectedValue}' : OK");
            validationContext.UpdateStatus(ValidationStatus.PASSED);
        }
        catch (ValidationException)
        {
            validationContext.UpdateStatus(ValidationStatus.FAILED);
            throw;
        }
    }

    /// <summary>
    ///     Determines whether the specified header name and type are supported by this validator.
    /// </summary>
    /// <param name="headerName">The name of the header to validate.</param>
    /// <param name="type">The type of the value associated with the header.</param>
    /// <returns>True if the validator supports the header name and type; otherwise, false.</returns>
    public bool Supports(string headerName, Type type)
    {
        return type == null ||
               type == typeof(string) ||
               type.IsPrimitive;
    }

    /// <summary>
    ///     Validates that the header array's actual values match the expected control values using the provided context.
    /// </summary>
    /// <param name="headerName">The name of the header being validated.</param>
    /// <param name="receivedValue">The actual value or values received for the header.</param>
    /// <param name="controlValue">The expected control value or values to validate against.</param>
    /// <param name="context">Context providing test-related information and functionality.</param>
    /// <param name="validationContext">Additional context managing the validation process and its result.</param>
    /// <exception cref="ValidationException">
    ///     Thrown when the validation fails for any header element or configuration
    ///     mismatch.
    /// </exception>
    public void ValidateHeaderArray(string headerName, object receivedValue, object controlValue, TestContext context,
        HeaderValidationContext validationContext)
    {
        var validator = GetHeaderValidator(headerName, controlValue, context);
        if (validator != null)
        {
            try
            {
                validator.ValidateHeader(headerName, receivedValue, controlValue, context, validationContext);
                validationContext.UpdateStatus(ValidationStatus.PASSED);
            }
            catch (ValidationException)
            {
                validationContext.UpdateStatus(ValidationStatus.FAILED);
                throw;
            }

            return;
        }

        try
        {
            var receivedValues = ToList(receivedValue);
            var controlValues = ToList(controlValue);

            // Convert and replace dynamic content for controlValue
            var expectedValues = controlValues
                .Select(value => context.TypeConverter.ConvertIfNecessary<string>(value, typeof(string)))
                .Select(str => context.ReplaceDynamicContentInString(str))
                .ToList();

            // Process received values
            if (receivedValue != null)
            {
                var receivedValueStrings = receivedValues
                    .Select(value => context.TypeConverter.ConvertIfNecessary<string>(value, typeof(string)))
                    .ToList();

                var expectedValuesCopy = new List<string>(expectedValues);

                // Iterate over received values and try to match with expected values
                foreach (var receivedValueString in from receivedValueString in receivedValueStrings
                         let validated = ValidateExpected(headerName, context, receivedValueString, expectedValuesCopy)
                         where !validated
                         select receivedValueString)
                    throw new ValidationException(
                        $"Values not equal for header element '{headerName}', expected '{string.Join(", ", expectedValues)}' but was '{receivedValueString}'");

                if (expectedValuesCopy.Any())
                    throw new ValidationException(
                        $"Values not equal for header element '{headerName}', expected '{string.Join(", ", expectedValues)}' but was '{string.Join(", ", receivedValues)}'");
            }
            else if (expectedValues.Any())
            {
                throw new ValidationException(
                    $"Values not equal for header element '{headerName}', expected '{string.Join(", ", expectedValues)}' but was 'null'");
            }

            Log.LogDebug("Validating header element: {HeaderName}='{Join}' : OK", headerName,
                string.Join(", ", expectedValues));
            validationContext.UpdateStatus(ValidationStatus.PASSED);
        }
        catch (ValidationException)
        {
            validationContext.UpdateStatus(ValidationStatus.FAILED);
            throw;
        }
    }

    /// <summary>
    ///     Validates whether the received header value matches any of the expected values and removes matched values
    ///     from the list of expected values.
    /// </summary>
    /// <param name="headerName">The name of the header being validated.</param>
    /// <param name="context">The context providing test-related information and functionality.</param>
    /// <param name="receivedValueString">The actual value received as a string.</param>
    /// <param name="expectedValues">A list of expected values for validation.</param>
    /// <returns>
    ///     A boolean value indicating whether the received value matches any of the expected values.
    /// </returns>
    private static bool ValidateExpected(string headerName, TestContext context,
        string receivedValueString, List<string> expectedValues)
    {
        for (var i = 0; i < expectedValues.Count; i++)
        {
            var expectedValue = expectedValues[i];

            if (IsMatcherValidation(headerName, receivedValueString, expectedValue, context) ||
                IsExactMatch(receivedValueString, expectedValue))
            {
                expectedValues.RemoveAt(i); // Remove the matched value
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Determines whether the received header value meets the conditions of a validation matcher expression.
    /// </summary>
    /// <param name="headerName">The name of the header being validated.</param>
    /// <param name="receivedValueString">The actual received value of the header as a string.</param>
    /// <param name="expectedValue">The expected value to validate against, which may contain a validation matcher expression.</param>
    /// <param name="context">The test context containing relevant information and utilities for the validation process.</param>
    /// <returns>True if the received value satisfies the conditions of the validation matcher; otherwise, false.</returns>
    /// <exception cref="ValidationException">Thrown when the validation matcher resolution fails for the given inputs.</exception>
    private static bool IsMatcherValidation(string headerName, string receivedValueString,
        string expectedValue, TestContext context)
    {
        if (!ValidationMatcherUtils.IsValidationMatcherExpression(expectedValue)) return false;

        try
        {
            ValidationMatcherUtils.ResolveValidationMatcher(headerName, receivedValueString, expectedValue, context);
            return true;
        }
        catch (ValidationException)
        {
            // Ignore this exception and try other expected values
            return false;
        }
    }

    /// <summary>
    ///     Determines if the received value matches the expected value exactly.
    /// </summary>
    /// <param name="receivedValueString">The actual string value received to be validated.</param>
    /// <param name="expectedValue">The expected string value to compare against.</param>
    /// <returns>
    ///     True if the received value matches the expected value exactly; otherwise, false.
    /// </returns>
    private static bool IsExactMatch(string receivedValueString, string expectedValue)
    {
        return receivedValueString.Equals(expectedValue);
    }

    /// <summary>
    ///     Converts the provided value to a list of strings.
    /// </summary>
    /// <param name="value">The object value to convert into a list of strings. Can be null.</param>
    /// <returns>
    ///     A list of strings derived from the input value. If the value is null, an empty list is returned. If the value
    ///     is already a list of strings, it is returned as-is. Otherwise, the value is converted to a string and wrapped in a
    ///     list.
    /// </returns>
    private static List<string> ToList(object value)
    {
        if (value == null) return [];

        return value as List<string> ?? [value.ToString()];
    }

    /// <summary>
    ///     Retrieves the appropriate header validator for the given header name and control value.
    /// </summary>
    /// <param name="headerName">The name of the header to be validated.</param>
    /// <param name="controlValue">The expected control value for validation.</param>
    /// <param name="context">Context providing test-related information and functionality.</param>
    /// <returns>
    ///     The header validator that supports the specified header name and control value type or null if no matching
    ///     validator is found.
    /// </returns>
    private static IHeaderValidator GetHeaderValidator(string headerName, object controlValue, TestContext context)
    {
        var allValidators = new Dictionary<string, IHeaderValidator>(DefaultValidators);
        var controlValueType = controlValue?.GetType();

        var validators = context.ReferenceResolver.ResolveAll<IHeaderValidator>();

        if (validators != null && validators.Count > 0)
            foreach (var validator in validators)
                allValidators.TryAdd(validator.Key, validator.Value);

        return allValidators.Values
            .Where(validator => validator is not DefaultHeaderValidator)
            .FirstOrDefault(validator => validator.Supports(headerName, controlValueType));
    }
}
