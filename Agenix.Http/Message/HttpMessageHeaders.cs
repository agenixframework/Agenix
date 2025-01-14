using Agenix.Core.Message;

namespace Agenix.Http.Message;

/// Represents HTTP-specific message headers used for HTTP communication.
/// This class provides a set of constant string fields that define common
/// headers for HTTP messages, including HTTP-specific properties such as
/// status code, version, method, etc. The headers are prefixed with a base
/// HTTP prefix defined by the concatenation of a general prefix and an HTTP
/// specific prefix. It serves as a key-value pair representation of the
/// headers for use in HTTP-based communication, allowing applications to
/// easily access and utilize these common HTTP headers.
public abstract class HttpMessageHeaders
{
    /// Defines the general prefix used for constructing HTTP-specific header keys in HTTP message headers.
    public static readonly string HttpPrefix = MessageHeaders.Prefix + "http_";

    /// Represents the status code of an HTTP message. This header field is used to
    /// define the response code returned by an HTTP server, indicating the result
    /// of the HTTP request.
    public static readonly string HttpStatusCode = HttpPrefix + "status_code";

    /// Defines the header key for specifying the HTTP version in HTTP message headers.
    public static readonly string HttpVersion = HttpPrefix + "version";

    /// Defines the key for the HTTP reason phrase header in HTTP message headers.
    /// This header provides a short textual description associated with the HTTP status code,
    /// typically sent by servers in HTTP response messages.
    public static readonly string HttpReasonPhrase = HttpPrefix + "reason_phrase";

    /// Specifies the header key used to define the HTTP request method within
    /// HTTP message headers.
    /// The HTTP request method identifies the desired action to be performed
    /// for a given resource, such as GET, POST, PUT, DELETE, etc. This constant
    /// constructs the header key by appending "method" to the HTTP
    public static readonly string HttpRequestMethod = HttpPrefix + "method";

    /// Represents the context path of an HTTP request as part of the HTTP message headers.
    /// This string field is used to identify the specific context path within a web application
    /// that an incoming HTTP request is targeting. It is constructed with a predefined HTTP-specific
    /// prefix to ensure consistent naming conventions in the representation of HTTP headers.
    public static readonly string HttpContextPath = HttpPrefix + "context_path";

    /// Represents the URI of the HTTP request.
    public static readonly string HttpRequestUri = HttpPrefix + "request_uri";

    /// Represents the HTTP query parameters header.
    public static readonly string HttpQueryParams = HttpPrefix + "query_params";

    /// Specifies the HTTP cookie prefix header.
    public static readonly string HttpCookiePrefix = HttpPrefix + "cookie_";

    /// Specifies the HTTP content type header.
    public static readonly string HttpContentType = "Content-Type";

    /// Specifies the HTTP Accept header.
    public static readonly string HttpAccept = "Accept";

    /// Represents HTTP-specific message headers.
    /// /
    private HttpMessageHeaders()
    {
    }
}