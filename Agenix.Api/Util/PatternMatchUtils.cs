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

namespace Agenix.Api.Util;

/// <summary>
///     Utility methods for simple pattern matching.
/// </summary>
public abstract class PatternMatchUtils
{
    /// <summary>
    ///     Match a String against the given pattern, supporting the following simple
    ///     pattern styles: "xxx*", "*xxx" and "*xxx*" matches, as well as direct equality.
    /// </summary>
    /// <param name="pattern">
    ///     the pattern to match against
    /// </param>
    /// <param name="str">
    ///     the String to match
    /// </param>
    /// <returns>
    ///     whether the String matches the given pattern
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

        if (pattern.StartsWith("*") && pattern.EndsWith("*") &&
            str.Contains(pattern.Substring(1, pattern.Length - 1 - 1)))
        {
            return true;
        }

        if (pattern.StartsWith("*") && str.EndsWith(pattern.Substring(1, pattern.Length - 1)))
        {
            return true;
        }

        return pattern.EndsWith("*") && str.StartsWith(pattern.Substring(0, pattern.Length - 1 - 0));
    }

    /// <summary>
    ///     Match a String against the given patterns, supporting the following simple
    ///     pattern styles: "xxx*", "*xxx" and "*xxx*" matches, as well as direct equality.
    /// </summary>
    /// <param name="patterns">
    ///     the patterns to match against
    /// </param>
    /// <param name="str">
    ///     the String to match
    /// </param>
    /// <returns>
    ///     whether the String matches any of the given patterns
    /// </returns>
    public static bool SimpleMatch(string[] patterns, string str)
    {
        return patterns != null && patterns.Any(t => SimpleMatch(t, str));
    }
}
