#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
// 
// Copyright (c) 2025 Agenix
// 
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

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
