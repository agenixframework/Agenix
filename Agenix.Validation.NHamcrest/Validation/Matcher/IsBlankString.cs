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
