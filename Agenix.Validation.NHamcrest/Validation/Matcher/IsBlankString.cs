using System.Text.RegularExpressions;
using NHamcrest;

namespace Agenix.Validation.NHamcrest.Validation.Matcher;

/// <summary>
///     Represents a matcher that checks if a string consists only of whitespace characters or is empty.
/// </summary>
/// <remarks>
///     This matcher is used to validate if a string is "blank," which means the string contains only whitespace characters
///     (spaces, tabs, etc.) or has a length of zero. It can also be combined with additional matchers for more complex
///     validation.
/// </remarks>
/// <example>
///     Not provided in this context.
/// </example>
/// <remarks>
///     This class is primarily intended to work with the NHamcrest library for fluent validation of string values.
/// </remarks>
public class IsBlankString : IMatcher<string>
{
    public static readonly IsBlankString BlankInstance = new();

    public static readonly IMatcher<string> NullOrBlankInstance =
        global::NHamcrest.Matches.AnyOf(Is.Null(), BlankInstance);

    private static readonly Regex RegexWhitespace = new(@"^\s*$", RegexOptions.Compiled);

    private IsBlankString()
    {
    }

    public void DescribeTo(IDescription description)
    {
        description.AppendText("a blank string");
    }

    public bool Matches(string item)
    {
        return RegexWhitespace.IsMatch(item);
    }

    public void DescribeMismatch(string item, IDescription mismatchDescription)
    {
        mismatchDescription.AppendText("was a ")
            .AppendText(" (")
            .AppendText(item)
            .AppendText(")");
    }
}