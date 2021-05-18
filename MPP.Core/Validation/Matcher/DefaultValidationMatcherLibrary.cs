using MPP.Core.Validation.Matcher.Core;

namespace MPP.Core.Validation.Matcher
{
    public class DefaultValidationMatcherLibrary : ValidationMatcherLibrary
    {
        /// <summary>
        /// Default constructor adds default matcher implementations.
        /// </summary>
        public DefaultValidationMatcherLibrary()
        {
            Name = " CoreValidationMatcherLibrary";

            Members.Add("EqualsIgnoreCase", new EqualsIgnoreCaseValidationMatcher());
            Members.Add("Ignore", new IgnoreValidationMatcher());
        }
    }
}