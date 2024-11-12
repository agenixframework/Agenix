using System;
using System.Collections.Generic;
using System.Linq;
using Agenix.Core.Exceptions;
using Agenix.Core.Validation.Context;
using Agenix.Core.Validation.Matcher;
using log4net;

namespace Agenix.Core.Validation;

/// DefaultHeaderValidator provides the mechanism to validate HTTP headers against
/// expected control values within a given test context.
/// /
public class DefaultHeaderValidator : IHeaderValidator
{
    private static readonly ILog _log = LogManager.GetLogger(typeof(IHeaderValidator));

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
            validator.ValidateHeader(headerName, receivedValue, controlValue, context, validationContext);
            return;
        }

        var expectedValue =
            controlValue != null
                ? context.ReplaceDynamicContentInString(
                    context.TypeConverter.ConvertIfNecessary<string>(controlValue, typeof(string))
                )
                : string.Empty;

        try
        {
            if (receivedValue != null)
            {
                var receivedValueString =
                    context.TypeConverter.ConvertIfNecessary<string>(receivedValue, typeof(string));

                if (ValidationMatcherUtils.IsValidationMatcherExpression(expectedValue))
                {
                    ValidationMatcherUtils.ResolveValidationMatcher(headerName, receivedValueString, expectedValue,
                        context);
                    return;
                }

                if (!receivedValueString.Equals(expectedValue))
                    throw new ValidationException("Values not equal for header element '"
                                                  + headerName + "', expected '"
                                                  + expectedValue + "' but was '"
                                                  + receivedValue + "'");
            }
            else
            {
                if (!string.IsNullOrEmpty(expectedValue))
                    throw new ValidationException("Values not equal for header element '"
                                                  + headerName + "', expected '"
                                                  + expectedValue + "' but was '"
                                                  + null + "'");
            }
        }
        catch (Exception e)
        {
            throw new ValidationException("Validation failed:", e);
        }

        if (_log.IsDebugEnabled)
            _log.Debug("Validating header element: " + headerName + "='" + expectedValue + "': OK.");
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
    ///     Retrieves the appropriate header validator for the given header name and control value.
    /// </summary>
    /// <param name="headerName">The name of the header to be validated.</param>
    /// <param name="controlValue">The expected control value for validation.</param>
    /// <param name="context">Context providing test-related information and functionality.</param>
    /// <returns>
    ///     The header validator that supports the specified header name and control value type, or null if no matching
    ///     validator is found.
    /// </returns>
    private static IHeaderValidator GetHeaderValidator(string headerName, object controlValue, TestContext context)
    {
        var allValidators = new Dictionary<string, IHeaderValidator>(DefaultValidators);
        var controlValueType = controlValue?.GetType();

        return allValidators.Values
            .Where(validator => validator is not DefaultHeaderValidator)
            .FirstOrDefault(validator => validator.Supports(headerName, controlValueType));
    }
}