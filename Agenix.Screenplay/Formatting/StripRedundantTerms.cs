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

namespace Agenix.Screenplay.Formatting;

/// <summary>
///     A utility class for removing redundant terms from an expression.
/// </summary>
/// <remarks>
///     This class provides a static method to streamline and clean up expressions
///     by stripping predefined redundant prefixes. It is particularly tailored to
///     identify the common terms that may add unnecessary verbosity to text.
/// </remarks>
public class StripRedundantTerms
{
    private static readonly IReadOnlyList<string> RedundantHamcrestPrefixes =
        ["is ", "be ", "should be"];

    public static string From(string expression)
    {
        return RedundantHamcrestPrefixes.Aggregate(expression, RemovePrefix);
    }

    private static string RemovePrefix(string expression, string prefix)
    {
        return expression.StartsWith(prefix)
            ? expression.Substring(prefix.Length)
            : expression;
    }
}
