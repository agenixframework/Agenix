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

using Agenix.Api.Exceptions;
using NHamcrest;
using NHamcrest.Core;

namespace Agenix.Validation.NHamcrest.Validation.Matcher;

/// <summary>
///     The MatcherAssert class provides static methods to perform assertions
///     using matchers in a fluent and expressive way. It validates conditions
///     by comparing actual values against matchers, throwing an exception if
///     the condition is not met.
///     Key Features:
///     - Supports assertions for values using matchers ('IMatcher')
///     - Allows specifying a reason/message to provide context to assertion failures.
///     - Throws `AssertionError` with detailed messages when conditions are not met.
///     Typical Usage:
///     - Assert boolean conditions or validate that a value satisfies a specific matcher.
///     - Used most commonly in unit testing frameworks such as NHamcrest.
///     Example Usage:
///     MatcherAssert.AssertThat(10, Is.GreaterThan(5));                // Asserts a condition.
///     MatcherAssert.AssertThat("Value must be 10", value, Is.EqualTo(10)); // Asserts with a reason.
///     MatcherAssert.AssertThat("Expression should evaluate true", true);  // Boolean assertion.
///     Exception Thrown:
///     - AssertionError: Thrown when an assertion fails, providing a clear and descriptive error message.
/// </summary>
public static class MatcherAssert
{
    // Overload 1: Assert with no reason, only actual and matcher
    /// Asserts that an actual value satisfies the specified matcher. Throws an assertion exception if the condition is not met.
    /// <typeparam name="T">The type of the actual value to be matched.</typeparam>
    /// <param name="actual">The actual value to be evaluated.</param>
    /// <param name="matcher">The matcher used to evaluate the actual value.</param>
    public static void AssertThat<T>(T actual, IMatcher<T> matcher)
    {
        AssertThat("", actual, matcher);
    }

    /// Asserts that an actual value satisfies the specified matcher. Throws an assertion exception if the condition is not met.
    /// <typeparam name="T">The type of the actual value to be evaluated.</typeparam>
    /// <param name="reason">The reason or custom message to be included in the assertion failure message.</param>
    /// <param name="actual">The actual value to be evaluated.</param>
    /// <param name="matcher">The matcher used to evaluate the actual value.</param>
    public static void AssertThat<T>(string reason, T actual, IMatcher<T> matcher)
    {
        if (!matcher.Matches(actual))
        {
            var description = new StringDescription();
            description.AppendText(reason)
                .AppendText(Environment.NewLine)
                .AppendText("Expected: ")
                .AppendDescriptionOf(matcher)
                .AppendText(Environment.NewLine)
                .AppendText("     but: ");
            matcher.DescribeMismatch(actual, description);

            throw new AssertionError(description.ToString());
        }
    }

    /// Asserts that the provided condition is true. Throws an assertion exception with the given reason if the condition is false.
    /// <param name="reason">The message to include in the exception if the assertion fails.</param>
    /// <param name="assertion">The condition to be evaluated.</param>
    public static void AssertThat(string reason, bool assertion)
    {
        if (!assertion)
        {
            throw new AssertionError(reason);
        }
    }
}
