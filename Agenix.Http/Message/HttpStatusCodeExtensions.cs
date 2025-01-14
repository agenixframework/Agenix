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