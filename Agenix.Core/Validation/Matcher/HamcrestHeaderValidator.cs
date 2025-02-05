using System;
using System.Linq;
using Agenix.Core.Exceptions;
using Agenix.Core.Validation.Context;
using log4net;
using NHamcrest;
using NHamcrest.Core;

namespace Agenix.Core.Validation.Matcher;

public class HamcrestHeaderValidator : IHeaderValidator
{
    /// Logger for HamcrestHeaderValidator.
    /// /
    private static readonly ILog Log = LogManager.GetLogger(typeof(HamcrestHeaderValidator));
    
    public void ValidateHeader(string headerName, object receivedValue, object controlValue, TestContext context,
        HeaderValidationContext validationContext)
    {
        // workaround to be able to test list of strings
        if (receivedValue is string receivedAsString)
            if (receivedAsString.Contains(','))
                receivedValue = receivedAsString.Split([','], StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()) // Remove extra spaces
                    .ToList();


        if (controlValue != null)
        {
            var controlType = controlValue.GetType();

            // Find the IMatcher<T> interface implemented by control
            var matcherInterface = controlType.GetInterfaces()
                .FirstOrDefault(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IMatcher<>));

            if (matcherInterface != null)
            {
                var matchesMethod = matcherInterface.GetMethod("Matches");
                if (matchesMethod != null)
                {
                    if (!(bool)matchesMethod.Invoke(controlValue, [receivedValue])!)
                    {
                        throw new ValidationException(ValidationUtils.BuildValueMismatchErrorMessage(
                            $"Header validation failed: Values not matching for header '{headerName}'", controlValue, receivedValue));
                    }
                }
            }
            else
            {
                var equalMatcher = new IsEqualMatcher<object>(controlValue); // Using NHamcrest's Is.EqualTo matcher
                if (!equalMatcher.Matches(receivedValue))
                {
                    throw new ValidationException(ValidationUtils.BuildValueMismatchErrorMessage(
                        $"Header validation failed: Values not equal for header '{headerName}'", controlValue, receivedValue));
                }
            }
        }
        
         
        if (Log.IsDebugEnabled)
        {
            Log.Debug($"Header validation: {headerName}='{controlValue}': OK");
        }
    }

    public bool Supports(string headerName, Type type)
    {
        var openGenericType = typeof(IMatcher<>);
        var result = type != null && type.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericType);

        return result;
    }
}