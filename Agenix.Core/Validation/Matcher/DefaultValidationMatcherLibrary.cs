using Agenix.Api.Validation.Matcher;
using Agenix.Core.Validation.Matcher.Core;
using log4net;

namespace Agenix.Core.Validation.Matcher;

public class DefaultValidationMatcherLibrary : ValidationMatcherLibrary
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(DefaultValidationMatcherLibrary));
    
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
        Members.Add("Trim", new TrimValidationMatcher());
        Members.Add("TrimAllWhiteSpaces", new TrimAllWhitespacesValidationMatcher());
        
        LookupValidationMatchers();
    }
    
    /// <summary>
    /// Add custom matcher implementations loaded from the resource path lookup.
    /// </summary>
    private void LookupValidationMatchers()
    {
        foreach (var (key, matcher) in IValidationMatcher.Lookup())
        {
            Members.Add(key, matcher);

            if (Log.IsDebugEnabled)
            {
                Log.Debug($"Register message matcher '{key}' as {matcher.GetType()}");
            }
        }
    }
}