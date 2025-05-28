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

/// Singleton class to test if a string represents a XML.
/// An empty string is considered to be a valid XML.
/// /
public class IsXmlPredicate
{
    /// <summary>
    ///     Lazy initialization object for the singleton instance of the <c>IsXmlPredicate</c> class.
    /// </summary>
    private static readonly Lazy<IsXmlPredicate> Lazy = new(() => new IsXmlPredicate());

    private IsXmlPredicate()
    {
        // Singleton
    }

    /// <summary>
    ///     Gets the singleton instance of the <c>IsXmlPredicate</c> class.
    /// </summary>
    public static IsXmlPredicate Instance => Lazy.Value;

    /// <summary>
    ///     Tests if a given string represents an XML. An empty string is considered to be valid XML.
    /// </summary>
    /// <param name="toTest">The string to test for XML validity.</param>
    /// <return>True if the string is valid XML or an empty string; false otherwise.</return>
    public bool Test(string toTest)
    {
        toTest = toTest?.Trim();
        return toTest != null && (toTest.Length == 0 || toTest.StartsWith('<'));
    }
}
