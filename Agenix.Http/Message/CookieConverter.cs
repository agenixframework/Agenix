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
using System.Text;

namespace Agenix.Http.Message;

/// Handles conversion of objects to and from cookies.
/// This class is intended to be replaced by a third-party cookie parser.
/// Serializable due to dependencies with HttpMessage implementation.
/// /
public class CookieConverter
{
    private const string Name = "Name";
    private const string Value = "Value";
    private const string Secure = "Secure";
    private const string Path = "Path";
    private const string Domain = "Domain";
    private const string MaxAge = "Max-Age";
    private const string HttpOnly = "HttpOnly";

    /// Converts cookies from an HttpEntity into Cookie objects.
    /// <param name="httpEntity">The message to convert.</param>
    /// <return>An array of converted cookies.</return>
    /// /
    public Cookie[] ConvertCookies(HttpRequestMessage httpEntity)
    {
        var outboundCookies = httpEntity.Headers.GetValues("Set-Cookie").ToList();

        return outboundCookies.Select(ConvertCookieString).ToArray();
    }

    /// Converts cookies from an HttpResponseMessage into Cookie objects.
    /// <param name="httpEntity">The HttpResponseMessage containing cookies to convert.</param>
    /// <return>An array of Cookie objects extracted from the HttpResponseMessage.</return>
    public virtual Cookie[] ConvertCookies(HttpResponseMessage httpEntity)
    {
        var inboundCookies = httpEntity.Headers.GetValues("Set-Cookie").ToList();

        return inboundCookies.Select(ConvertCookieString).ToArray();
    }

    /// Converts a given cookie into a HTTP conform cookie string representation.
    /// @param cookie The cookie to convert.
    /// @return The cookie string representation of the given cookie.
    /// /
    public string GetCookieString(Cookie cookie)
    {
        var builder = new StringBuilder();

        builder.Append(cookie.Name);
        builder.Append('=');
        builder.Append(cookie.Value);

        if (!string.IsNullOrEmpty(cookie.Path))
        {
            builder.Append(";" + Path + "=").Append(cookie.Path);
        }

        if (!string.IsNullOrEmpty(cookie.Domain))
        {
            builder.Append(";" + Domain + "=").Append(cookie.Domain);
        }

        if (cookie.Expires != default)
        {
            // Calculate Max-Age in seconds
            var maxAgeSeconds = (int)(cookie.Expires - DateTime.UtcNow).TotalSeconds;
            // Ensure it is at least 0 since cookies may have already expired
            maxAgeSeconds = Math.Max(0, maxAgeSeconds);

            builder.Append($";Max-Age={maxAgeSeconds}");
        }

        if (cookie.Secure)
        {
            builder.Append(";" + Secure);
        }

        if (cookie.HttpOnly)
        {
            builder.Append(";" + HttpOnly);
        }

        return builder.ToString();
    }

    /// Converts a cookie string from an HTTP header value into a Cookie object.
    /// <param name="cookieString">The string to convert.</param>
    /// <return>The Cookie representation of the given string.</return>
    /// /
    private Cookie ConvertCookieString(string cookieString)
    {
        var cookie = new Cookie(GetCookieParam(Name, cookieString), GetCookieParam(Value, cookieString));

        if (cookieString.Contains(Path))
        {
            cookie.Path = GetCookieParam(Path, cookieString);
        }

        if (cookieString.Contains(Domain))
        {
            cookie.Domain = GetCookieParam(Domain, cookieString);
        }

        if (cookieString.Contains(MaxAge))
        {
            // Parse the 'max-age' parameter to an integer
            if (int.TryParse(GetCookieParam(MaxAge, cookieString), out var maxAgeSeconds))
            {
                try
                {
                    // Convert Max-Age seconds to an expiration date
                    cookie.Expires = DateTime.UtcNow.AddSeconds(maxAgeSeconds);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    // Handle or log the exception
                    Console.WriteLine($"Invalid 'max-age' value: {maxAgeSeconds}. Exception: {ex.Message}");
                    // Optionally, set a default or minimum expiration
                    cookie.Expires = DateTime.UtcNow;
                }
            }
            else
            {
                // Handle the potential parsing failure
                Console.WriteLine($"Failed to parse 'max-age' from cookie string: {cookieString}");
                // Optionally, set a default expiration
                cookie.Expires = DateTime.UtcNow;
            }
        }

        if (cookieString.Contains(Secure))
        {
            cookie.Secure = Convert.ToBoolean(GetCookieParam(Secure, cookieString));
        }

        if (cookieString.Contains(HttpOnly))
        {
            cookie.HttpOnly = Convert.ToBoolean(GetCookieParam(HttpOnly, cookieString));
        }

        return cookie;
    }

    /// Extracts a specified parameter from a cookie string as provided by the "Set-Cookie" header.
    /// @param param The parameter to extract from the cookie string.
    /// @param cookieString The cookie string from which to extract the parameter.
    /// @return The value of the requested parameter.
    /// @throws Exception if the specified parameter cannot be found within the cookie string.
    private string GetCookieParam(string param, string cookieString)
    {
        if (param.Equals(Name))
        {
            return cookieString[..cookieString.IndexOf('=')];
        }

        if (param.Equals(Value))
        {
            var startIdx = cookieString.IndexOf('=') + 1;
            var endIdx = cookieString.IndexOf(';', startIdx);
            return endIdx > startIdx
                ? cookieString[startIdx..endIdx]
                : cookieString[startIdx..];
        }

        if (ContainsFlag(Secure, param, cookieString) || ContainsFlag(HttpOnly, param, cookieString))
        {
            return bool.TrueString;
        }

        if (cookieString.Contains(param + '='))
        {
            var beginIndex = cookieString.IndexOf(param + '=', StringComparison.Ordinal) + param.Length + 1;
            var endParam = cookieString.IndexOf(';', beginIndex);
            return endParam > 0
                ? cookieString[beginIndex..endParam]
                : cookieString[beginIndex..];
        }

        throw new Exception($"Unable to get cookie argument '{param}' from cookie String: {cookieString}");
    }

    /// Checks if a specified flag is contained within a given cookie string.
    /// @param flag The flag to check for.
    /// @param param The parameter that the flag is associated with.
    /// @param cookieString The cookie string to search within.
    /// @return True if the specified flag is found in the cookie string, otherwise false.
    /// /
    private bool ContainsFlag(string flag, string param, string cookieString)
    {
        return flag.Equals(param) && cookieString.Contains(flag);
    }
}
