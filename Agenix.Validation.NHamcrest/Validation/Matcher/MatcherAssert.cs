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
///         `).
///         - Allows specifying a reason/message to provide context to assertion failures.
///         - Throws `AssertionError` with detailed messages when conditions are not met.
///         Typical Usage:
///         - Assert boolean conditions or validate that a value satisfies a specific matcher.
///         - Used most commonly in unit testing frameworks such as NHamcrest.
///         Example Usage:
///         MatcherAssert.AssertThat(10, Is.GreaterThan(5));                // Asserts a condition.
///         MatcherAssert.AssertThat("Value must be 10", value, Is.EqualTo(10)); // Asserts with a reason.
///         MatcherAssert.AssertThat("Expression should evaluate true", true);  // Boolean assertion.
///         Exception Thrown:
///         - AssertionError: Thrown when an assertion fails, providing a clear and descriptive error message.
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
        if (!assertion) throw new AssertionError(reason);
    }
}