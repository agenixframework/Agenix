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
using Agenix.Api.Context;

namespace Agenix.Http.Message;

/// Provides functionality to enrich a set of cookies by integrating dynamic information from a specified test context.
/// /
public class CookieEnricher
{
    /// Replaces the dynamic content in the provided list of cookies using context variables.
    /// <param name="cookies">The list of Cookies to be enriched.</param>
    /// <param name="context">The context used to replace dynamic variables within the cookie attributes.</param>
    /// <return>A list of enriched Cookies where dynamic content has been replaced.</return>
    /// /
    public List<Cookie> Enrich(List<Cookie> cookies, TestContext context)
    {
        var enrichedCookies = new List<Cookie>();

        foreach (var cookie in cookies)
        {
            var enrichedCookie = new Cookie(cookie.Name, cookie.Value);

            if (cookie.Value != null) enrichedCookie.Value = context.ReplaceDynamicContentInString(cookie.Value);

            if (cookie.Path != null) enrichedCookie.Path = context.ReplaceDynamicContentInString(cookie.Path);

            if (cookie.Domain != null) enrichedCookie.Domain = context.ReplaceDynamicContentInString(cookie.Domain);

            enrichedCookie.HttpOnly = cookie.HttpOnly;
            enrichedCookie.Secure = cookie.Secure;

            enrichedCookies.Add(enrichedCookie);
        }

        return enrichedCookies;
    }
}
