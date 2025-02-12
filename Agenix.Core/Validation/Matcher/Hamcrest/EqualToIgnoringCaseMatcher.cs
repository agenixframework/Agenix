using System;
using NHamcrest;

namespace Agenix.Core.Validation.Matcher.Hamcrest;

public class EqualToIgnoringCaseMatcher(string expected) : IMatcher<string>
{
    public void DescribeTo(IDescription description)
    {
        description.AppendText("a string equal to ")
            .AppendValue(expected)
            .AppendText(" ignoring case");
    }

    public bool Matches(string actual)
    {
        if (actual == null && expected == null) return true;

        if (actual == null || expected == null) return false;

        return string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase);
    }

    public void DescribeMismatch(string item, IDescription mismatchDescription)
    {
        mismatchDescription.AppendText("was ").AppendValue(item);
    }
}