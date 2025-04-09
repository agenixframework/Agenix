using System.Linq;

namespace Agenix.Core.Util
{
    /// <summary> Utility methods for simple pattern matching.
    /// </summary>
    public abstract class PatternMatchUtils
    {
        /// <summary> Match a String against the given pattern, supporting the following simple
        /// pattern styles: "xxx*", "*xxx" and "*xxx*" matches, as well as direct equality.
        /// </summary>
        /// <param name="pattern">the pattern to match against
        /// </param>
        /// <param name="str">the String to match
        /// </param>
        /// <returns> whether the String matches the given pattern
        /// </returns>
        public static bool SimpleMatch(string pattern, string str)
        {
            if (ObjectUtils.NullSafeEquals(pattern, str) || "*".Equals(pattern))
            {
                return true;
            }
            if (pattern == null || str == null)
            {
                return false;
            }
            if (pattern.StartsWith($"*") && pattern.EndsWith("*") &&
                str.Contains(pattern.Substring(1, (pattern.Length - 1) - (1))))
            {
                return true;
            }
            if (pattern.StartsWith($"*") && str.EndsWith(pattern.Substring(1, (pattern.Length) - (1))))
            {
                return true;
            }
            return pattern.EndsWith($"*") && str.StartsWith(pattern.Substring(0, (pattern.Length - 1) - (0)));
        }

        /// <summary> Match a String against the given patterns, supporting the following simple
        /// pattern styles: "xxx*", "*xxx" and "*xxx*" matches, as well as direct equality.
        /// </summary>
        /// <param name="patterns">the patterns to match against
        /// </param>
        /// <param name="str">the String to match
        /// </param>
        /// <returns> whether the String matches any of the given patterns
        /// </returns>
        public static bool SimpleMatch(System.String[] patterns, System.String str)
        {
            return patterns != null && patterns.Any(t => SimpleMatch(t, str));
        }

    }
}
