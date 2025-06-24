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
using Agenix.Api.Validation;
using NHamcrest;
using NHamcrest.Core;

namespace Agenix.Validation.NHamcrest.Validation;

/// <summary>
///     Class HamcrestValueMatcher provides validation logic for matching received objects
///     against control objects using Hamcrest matchers.
/// </summary>
public class NHamcrestValueMatcher : IValueMatcher
{
    /// <summary>
    ///     Filter supported value types
    /// </summary>
    /// <param name="controlType">The control type to be checked for support.</param>
    /// <returns>True if the control type is supported, otherwise false.</returns>
    public bool Supports(Type controlType)
    {
        var openGenericType = typeof(IMatcher<>);
        var result = controlType.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericType);

        return result;
    }

    /// <summary>
    ///     Validates whether the received object matches the control object.
    /// </summary>
    /// <param name="received">The object to be validated.</param>
    /// <param name="control">The object used as the control for comparison.</param>
    /// <param name="context">The test context that provides additional validation context.</param>
    /// <returns>True if the received object matches the control, otherwise false.</returns>
    public bool Validate(object received, object control, TestContext context)
    {
        // workaround to be able to test a list of strings
        if (received is string receivedAsString)
        {
            if (receivedAsString.Contains(','))
            {
                received = receivedAsString.Split([','], StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()) // Remove extra spaces
                    .ToList();
            }
        }


        if (control != null)
        {
            var controlType = control.GetType();

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
                    return (bool)matchesMethod.Invoke(control, [received])!;
                }
            }
        }

        var equalMatcher = new IsEqualMatcher<object>(control);
        return equalMatcher.Matches(received);
    }
}
