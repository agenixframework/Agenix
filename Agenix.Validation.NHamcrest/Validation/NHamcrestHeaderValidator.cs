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

using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Core.Validation;
using Microsoft.Extensions.Logging;
using NHamcrest;
using NHamcrest.Core;

namespace Agenix.Validation.NHamcrest.Validation;

public class NHamcrestHeaderValidator : IHeaderValidator
{
    /// Logger for HamcrestHeaderValidator.
    /// /
    private static readonly ILogger Log = LogManager.GetLogger(typeof(NHamcrestHeaderValidator));

    public void ValidateHeader(string headerName, object receivedValue, object controlValue, TestContext context,
        HeaderValidationContext validationContext)
    {
        // workaround to be able to test list of strings
        if (receivedValue is string receivedAsString)
        {
            if (receivedAsString.Contains(','))
            {
                receivedValue = receivedAsString.Split([','], StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()) // Remove extra spaces
                    .ToList();
            }
        }


        try
        {
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
                                $"Header validation failed: Values not matching for header '{headerName}'",
                                controlValue,
                                receivedValue));
                        }
                    }
                }
                else
                {
                    var equalMatcher = new IsEqualMatcher<object>(controlValue); // Using NHamcrest's Is.EqualTo matcher
                    if (!equalMatcher.Matches(receivedValue))
                    {
                        throw new ValidationException(ValidationUtils.BuildValueMismatchErrorMessage(
                            $"Header validation failed: Values not equal for header '{headerName}'", controlValue,
                            receivedValue));
                    }
                }
            }


            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug($"Header validation: {headerName}='{controlValue}': OK");
            }

            validationContext.UpdateStatus(ValidationStatus.PASSED);
        }
        catch (ValidationException)
        {
            validationContext.UpdateStatus(ValidationStatus.FAILED);
            throw;
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
