using System;
using Agenix.Core.Exceptions;
using Agenix.Core.Validation.Matcher;

namespace Agenix.Core.Validation;

public class DefaultHeaderValidator : IHeaderValidator
{
    public void ValidateHeader(string headerName, object receivedValue, object controlValue, TestContext context)
    {
        var expectedValue =
            context.ReplaceDynamicContentInString(controlValue == null
                ? string.Empty
                : string.Join(",", controlValue));

        try
        {
            if (receivedValue != null)
            {
                var actualValue = string.Join(",", receivedValue);

                if (ValidationMatcherUtils.IsValidationMatcherExpression(expectedValue))
                {
                    ValidationMatcherUtils.ResolveValidationMatcher(headerName, actualValue, expectedValue,
                        context);
                    return;
                }

                if (!actualValue.Equals(expectedValue))
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

        Console.WriteLine("Validating header element: " + headerName + "='" + expectedValue + "': OK.");
    }
}