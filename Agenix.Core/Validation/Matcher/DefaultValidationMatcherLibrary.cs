using Agenix.Core.Validation.Matcher.Core;

namespace Agenix.Core.Validation.Matcher
{
    public class DefaultValidationMatcherLibrary : ValidationMatcherLibrary
    {
        /// <summary>
        ///     Default constructor adds default matcher implementations.
        /// </summary>
        public DefaultValidationMatcherLibrary()
        {
            Name = " CoreValidationMatcherLibrary";

            Members.Add("EqualsIgnoreCase", new EqualsIgnoreCaseValidationMatcher());
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
            Members.Add("Trim", new TrimValidationMatcher());
            Members.Add("TrimAllWhiteSpaces", new TrimAllWhitespacesValidationMatcher());
        }
    }
}