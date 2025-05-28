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

using System.Net;

namespace Agenix.Http.Message;

/// <summary>
///     Provides extension methods for working with the <see cref="HttpStatusCode" /> enumeration.
/// </summary>
public static class HttpStatusCodeExtensions
{
    /// <summary>
    ///     Converts an integer status code to its corresponding <see cref="HttpStatusCode" /> enumeration value, if defined.
    /// </summary>
    /// <param name="statusCode">The integer value of the status code to convert.</param>
    /// <returns>
    ///     The <see cref="HttpStatusCode" /> enumeration value corresponding to the specified integer status code, or null if
    ///     the status code is not defined in the <see cref="HttpStatusCode" /> enumeration.
    /// </returns>
    public static HttpStatusCode? ValueOf(int statusCode)
    {
        if (Enum.IsDefined(typeof(HttpStatusCode), statusCode)) return (HttpStatusCode)statusCode;
        return null; // Or handle custom status codes differently
    }
}
