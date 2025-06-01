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
///     Tests if a string represents a JSON. An empty string is considered to be a
///     valid JSON.
/// </summary>
public class IsJsonPredicate
{
    private IsJsonPredicate()
    {
        // Singleton
    }

    /// <summary>
    ///     Singleton instance of the IsJsonPredicate class.
    /// </summary>
    public static IsJsonPredicate Instance { get; } = new();

    /// <summary>
    ///     Tests if a string represents a JSON. An empty string is considered to be a
    ///     valid JSON.
    /// </summary>
    /// <param name="toTest">The string to test for JSON format.</param>
    /// <returns>True if the string is a valid JSON or an empty string, otherwise false.</returns>
    public bool Test(string toTest)
    {
        if (toTest != null)
        {
            toTest = toTest.Trim();
        }

        return toTest != null && (toTest.Length == 0 || toTest.StartsWith('{') || toTest.StartsWith('['));
    }
}
