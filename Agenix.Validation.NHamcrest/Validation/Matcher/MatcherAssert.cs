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
///     - Supports assertions for values using matchers (`IMatcher')
///     `).
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
    public static void AssertThat<T>(T actual, IMatcher<T> matcher)
    {
        AssertThat("", actual, matcher);
    }

    // Overload 2: Assert with a reason, actual value, and matcher
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

    // Overload 3: Assert with a reason and boolean value
    public static void AssertThat(string reason, bool assertion)
    {
        if (!assertion)
        {
            throw new AssertionError(reason);
        }
    }
}
