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
