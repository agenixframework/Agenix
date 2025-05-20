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