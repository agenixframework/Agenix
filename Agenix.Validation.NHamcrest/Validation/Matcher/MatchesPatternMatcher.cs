using System.Text.RegularExpressions;
using NHamcrest;

namespace Agenix.Validation.NHamcrest.Validation.Matcher;

public class MatchesPatternMatcher : IMatcher<string>
{
    private readonly string _pattern;

    public MatchesPatternMatcher(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            throw new ArgumentException("Pattern must not be null or empty", nameof(pattern));
        _pattern = pattern;
    }

    public void DescribeTo(IDescription description)
    {
        description.AppendText("a string matching the pattern ")
            .AppendValue(_pattern);
    }

    public bool Matches(string actual)
    {
        if (actual == null) return false;

        try
        {
            return Regex.IsMatch(actual, _pattern);
        }
        catch (ArgumentException)
        {
            throw new ArgumentException("Invalid regular expression pattern: " + _pattern);
        }
    }

    public void DescribeMismatch(string item, IDescription mismatchDescription)
    {
        mismatchDescription.AppendText("was ")
            .AppendValue(item)
            .AppendText(", which does not match the pattern");
    }
}