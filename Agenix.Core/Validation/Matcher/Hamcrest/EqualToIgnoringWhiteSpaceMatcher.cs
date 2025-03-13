using System;
using System.Text.RegularExpressions;
using NHamcrest;

namespace Agenix.Core.Validation.Matcher.Hamcrest;

public class EqualToIgnoringWhiteSpaceMatcher(string expected) : IMatcher<string>
{
    private readonly string _expected = NormalizeWhiteSpace(expected);

    public void DescribeTo(IDescription description)
    {
        description.AppendText("a string equal to ")
            .AppendValue(_expected)
            .AppendText(" ignoring whitespace differences");
    }

    public bool Matches(string actual)
    {
        if (actual == null && _expected == null) return true;

        if (actual == null || _expected == null) return false;

        return NormalizeWhiteSpace(actual).Equals(_expected, StringComparison.Ordinal);
    }

    public void DescribeMismatch(string item, IDescription mismatchDescription)
    {
        mismatchDescription.AppendText("was ").AppendValue(item);
    }

    // A utility method to normalize whitespace in a string
    private static string NormalizeWhiteSpace(string input)
    {
        return input == null
            ? null
            :
            // Replace multiple spaces with a single space and trim leading/trailing spaces
            Regex.Replace(input.Trim(), "\\s+", " ");
    }
}