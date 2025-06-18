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

using Agenix.Api.Log;
using Agenix.Api.Validation.Matcher;
using Agenix.Core.Validation.Matcher.Core;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Validation.Matcher;

public class DefaultValidationMatcherLibrary : ValidationMatcherLibrary
{
    private static readonly ILogger Log = LogManager.GetLogger(typeof(DefaultValidationMatcherLibrary));

    /// <summary>
    ///     Default constructor adds default matcher implementations.
    /// </summary>
    public DefaultValidationMatcherLibrary()
    {
        Name = " CoreValidationMatcherLibrary";

        Members.Add("EqualsIgnoreCase", new EqualsIgnoreCaseValidationMatcher());
        Members.Add("Empty", new EmptyValidationMatcher());
        Members.Add("Ignore", new IgnoreValidationMatcher());
        Members.Add("ContainsIgnoreCase", new ContainsIgnoreCaseValidationMatcher());
        Members.Add("Contains", new ContainsValidationMatcher());
        Members.Add("DatePattern", new DatePatternValidationMatcher());
        Members.Add("EndsWith", new EndsWithValidationMatcher());
        Members.Add("LowerThan", new LowerThanValidationMatcher());
        Members.Add("GreaterThan", new GreaterThanValidationMatcher());
        Members.Add("IgnoreNewLine", new IgnoreNewLineValidationMatcher());
        Members.Add("IsNumber", new IsNumberValidationMatcher());
        Members.Add("Matches", new MatchesValidationMatcher());
        Members.Add("StartsWith", new StartsWithValidationMatcher());
        Members.Add("StringLength", new StringLengthValidationMatcher());
        Members.Add("Variable", new CreateVariableValidationMatcher());
        Members.Add("Trim", new TrimValidationMatcher());
        Members.Add("TrimAllWhiteSpaces", new TrimAllWhitespacesValidationMatcher());

        LookupValidationMatchers();
    }

    /// <summary>
    ///     Add custom matcher implementations loaded from the resource path lookup.
    /// </summary>
    private void LookupValidationMatchers()
    {
        foreach (var (key, matcher) in IValidationMatcher.Lookup())
        {
            Members.Add(key, matcher);

            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug("Register message matcher '{Key}' as {Type}", key, matcher.GetType());
            }
        }
    }
}
