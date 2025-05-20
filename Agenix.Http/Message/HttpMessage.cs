using System.Net;
using System.Text;
using Agenix.Api.Endpoint.Resolver;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Api.Util;
using Agenix.Core.Message;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace Agenix.Http.Message;

/// Represents a HTTP message with methods to set and retrieve HTTP-specific
/// details such as method, URI, headers, and status codes. This class extends
/// DefaultMessage and provides additional functionality for modifying and
/// inspecting HTTP message components.
public class HttpMessage : DefaultMessage
{
    /// Converts cookies to and from their string representations for HTTP headers.
    private readonly CookieConverter _cookieConverter = new();

    /// Collection of HTTP cookies mapped by their name.
    /// /
    private readonly Dictionary<string, Cookie> _cookies = new();

    /// Dictionary of query parameters, where each name is associated with a list of values.
    private readonly Dictionary<string, LinkedList<string>> _queryParams = new();

    /// Represents a HTTP message with methods to set and retrieve HTTP-specific
    /// details such as method, URI, headers, and status codes. This class extends
    /// DefaultMessage and provides additional functionality for modifying and
    /// inspecting HTTP message components.
    /// /
    public HttpMessage()
    {
    }

    /// Represents a HTTP message designed for setting and retrieving various HTTP-specific
    /// attributes such as method, URI, headers, status codes, and more. It builds on the
    /// DefaultMessage class to offer enhanced capabilities for configuring and manipulating
    /// the different components of an HTTP message for networked communication scenarios.
    public HttpMessage(IMessage message) : this(message, false)
    {
    }

    /// Represents a HTTP message with methods to set and retrieve HTTP-specific
    /// details such as method, URI, query parameters, headers, status codes, and cookies.
    /// This class extends DefaultMessage, offering additional functionality for modifying
    /// and inspecting various HTTP message components.
    public HttpMessage(IMessage message, bool forceAgenixHeaderUpdate) : base(message, forceAgenixHeaderUpdate)
    {
        CopyCookies(message);
    }

    /// Represents a HTTP message with methods to manage HTTP-specific
    /// details such as method, URI, headers, status codes, and payload.
    /// Inherits from DefaultMessage and offers extensions to manipulate
    /// HTTP message components effectively.
    public HttpMessage(object payload) : base(payload)
    {
    }

    /// Represents an HTTP message, providing methods to define and manipulate
    /// HTTP-specific properties such as method, URI, headers, status code, and more.
    /// Extends the DefaultMessage class to offer additional operations for managing
    /// HTTP message components, enabling customization and construction of messages
    /// for HTTP communication.
    public HttpMessage(object payload, Dictionary<string, object> headers) : base(payload, headers)
    {
    }

    /// Sets the cookies extracted from the given message if it is an instance of HttpMessage.
    /// If the message contains cookies, they are copied into the current message's cookie collection.
    /// <param name="message">The message from which to extract cookies.</param>
    private void CopyCookies(IMessage message)
    {
        if (message is HttpMessage httpMessage)
            foreach (var cookie in httpMessage.GetCookiesMap())
                _cookies[cookie.Key] = cookie.Value;
    }

    /// Sets the HTTP request method header for this message.
    /// <param name="method">The HTTP method to set as the request method header.</param>
    /// <return>The modified HttpMessage instance with the updated method header.</return>
    public HttpMessage Method(HttpMethod method)
    {
        SetHeader(HttpMessageHeaders.HttpRequestMethod, method.ToString());
        return this;
    }

    /// Sets the HTTP version header for the message.
    /// <param name="version">The HTTP version header value to use.</param>
    /// <returns>The modified HttpMessage with the updated HTTP version header.</returns>
    public HttpMessage Version(string version)
    {
        SetHeader(HttpMessageHeaders.HttpVersion, version);
        return this;
    }

    /// Sets the HTTP status code and reason phrase headers in the message.
    /// <param name="statusCode">The status code to set in the HTTP message header.</param>
    /// <returns>The modified HttpMessage with the updated status code and reason phrase headers.</returns>
    public virtual HttpMessage Status(HttpStatusCode statusCode)
    {
        SetHeader(HttpMessageHeaders.HttpStatusCode, (int)statusCode);
        var status = (HttpStatusCode)(int)statusCode;
        if (Enum.IsDefined(typeof(HttpStatusCode), (int)statusCode))
            SetHeader(HttpMessageHeaders.HttpReasonPhrase, status.ToString());

        return this;
    }

    /// Sets the HTTP response reason phrase header.
    /// <param name="reasonPhrase">The reason phrase header value to use</param>
    /// <return>The altered HttpMessage</return>
    public HttpMessage ReasonPhrase(string reasonPhrase)
    {
        SetHeader(HttpMessageHeaders.HttpReasonPhrase, reasonPhrase);
        return this;
    }

    /// Sets the HTTP request URI header values. This method is used to configure
    /// the URI associated with the HTTP request by setting the appropriate headers.
    /// <param name="requestUri">The URI string to be set as the request URI header.</param>
    /// <returns>The modified instance of HttpMessage with the updated URI headers.</returns>
    public HttpMessage Uri(string requestUri)
    {
        SetHeader(IEndpointUriResolver.EndpointUriHeaderName, requestUri);
        SetHeader(HttpMessageHeaders.HttpRequestUri, requestUri);
        return this;
    }

    /// Sets the HTTP request content type header.
    /// <param name="contentType">The content type header value to use</param>
    /// <return>The altered HttpMessage</return>
    public HttpMessage ContentType(string contentType)
    {
        SetHeader("Content-Type", contentType);
        return this;
    }

    /// Sets the HTTP accepted content type header for the response.
    /// <param name="accept">The accept header value to set.</param>
    /// <return>The altered HttpMessage.</return>
    public HttpMessage Accept(string accept)
    {
        SetHeader("Accept", accept);
        return this;
    }

    /// Sets the HTTP request context path header.
    /// <param name="contextPath">The context path header value to use</param>
    /// <return>The altered HttpMessage</return>
    public HttpMessage ContextPath(string contextPath)
    {
        SetHeader(HttpMessageHeaders.HttpContextPath, contextPath);
        return this;
    }

    /// Sets the HTTP request query parameters from a query string. The query string is a series
    /// of key-value pairs separated by commas, such as "key1=value1,key2=value2".
    /// This method also updates the appropriate headers and splits each key-value pair into
    /// an internal collection for further processing. An empty query string is permitted.
    /// <param name="queryParamString">The query parameter string to be processed.</param>
    /// <return>The modified HttpMessage instance with updated query parameters.</return>
    public HttpMessage QueryParams(string queryParamString)
    {
        Header(HttpMessageHeaders.HttpQueryParams, queryParamString);
        Header(IEndpointUriResolver.QueryParamHeaderName, queryParamString);

        queryParamString.Split(',')
            .Select(keyValue => keyValue.Split('='))
            .Where(keyValue => StringUtils.HasText(keyValue[0]))
            .Select(keyValue => keyValue.Length < 2 ? [keyValue[0], ""] : keyValue)
            .ToList()
            .ForEach(keyValue => AddQueryParam(keyValue[0], keyValue[1]));

        return this;
    }

    /// Sets a new HTTP request query parameter.
    /// <param name="name">The name of the request query parameter. Must not be empty or null.</param>
    /// <param name="value">The value of the request query parameter. Can be null if no value is specified.</param>
    /// <return>The altered HttpMessage with the newly added query parameter.</return>
    public HttpMessage QueryParam(string? name, string? value = null)
    {
        if (!StringUtils.HasText(name)) throw new AgenixSystemException("Invalid query param name - must not be empty!");

        AddQueryParam(name, value);

        // Sort the query parameters and then build the queryParamString
        var sortedQueryParamString = string.Join(",",
            _queryParams.OrderBy(entry => entry.Key)
                .Select(OutputQueryParam));

        Header(HttpMessageHeaders.HttpQueryParams, sortedQueryParamString);
        Header(IEndpointUriResolver.QueryParamHeaderName, sortedQueryParamString);

        return this;
    }

    /// Sets the request path that is dynamically added to the base URI.
    /// <param name="path">The part of the path to add.</param>
    /// <return>The altered HttpMessage.</return>
    public HttpMessage Path(string path)
    {
        Header(HttpMessageHeaders.HttpRequestUri, path);
        Header(IEndpointUriResolver.RequestPathHeaderName, path);
        return this;
    }

    /// Sets a new header name-value pair for the HTTP message.
    /// <param name="headerName">The name of the header.</param>
    /// <param name="headerValue">The value of the header.</param>
    /// <return>The altered HttpMessage instance.</return>
    public HttpMessage Header(string headerName, object headerValue)
    {
        return (HttpMessage)base.SetHeader(headerName, headerValue);
    }

    /// Sets a header in the HTTP message with the specified name and value,
    /// allowing for customization of the HTTP message components.
    /// <param name="headerName">The name of the header to set in the HTTP message.</param>
    /// <param name="headerValue">The value to assign to the specified header.</param>
    /// <return>Returns the updated instance of the HttpMessage.</return>
    public override HttpMessage SetHeader(string headerName, object headerValue)
    {
        return (HttpMessage)base.SetHeader(headerName, headerValue);
    }

    /// Adds HTTP-specific header data to the message.
    /// <param name="headerData">The header data to include in the HTTP message.</param>
    /// <returns>Returns the updated HttpMessage instance with the specified header data.</returns>
    public override HttpMessage AddHeaderData(string headerData)
    {
        return (HttpMessage)base.AddHeaderData(headerData);
    }

    /// Retrieves the HTTP request method from the message headers.
    /// <return>The HttpMethod used in the request. If no method is specified, defaults to GET.</return>
    public HttpMethod? GetRequestMethod()
    {
        var method = GetHeader(HttpMessageHeaders.HttpRequestMethod);

        return method != null ? new HttpMethod(method.ToString() ?? "GET") : null;
    }

    /// Retrieves the HTTP request URI associated with this message by accessing the
    /// relevant header field.
    /// <return>The request URI as a string</return>
    public string GetUri()
    {
        var requestUri = GetHeader(HttpMessageHeaders.HttpRequestUri);

        return requestUri?.ToString()!;
    }

    /// Retrieves the HTTP request context path from the message headers.
    /// <returns>The context path as a string.</returns>
    public string GetContextPath()
    {
        var contextPath = GetHeader(HttpMessageHeaders.HttpContextPath);

        return contextPath?.ToString()!;
    }

    /// Retrieves the HTTP content type header value.
    /// <return>The content type header value as a string</return>
    public string GetContentType()
    {
        var contentType = GetHeader(HttpMessageHeaders.HttpContentType);

        return contentType?.ToString()!;
    }

    /// Retrieves the value of the HTTP "Accept" header from the message.
    /// return The accept header value
    public string GetAccept()
    {
        var accept = GetHeader("Accept");

        return accept?.ToString()!;
    }

    /// Retrieves the HTTP request query parameters.
    /// <return>
    ///     A dictionary containing the query parameters as key-value pairs. Each key represents a query parameter name,
    ///     and the corresponding value is a list of strings representing the values associated with that parameter.
    /// </return>
    public IDictionary<string, LinkedList<string>> GetQueryParams()
    {
        return _queryParams;
    }

    /// Retrieves the HTTP request query parameter string.
    /// <return>The query parameter string.</return>
    public string GetQueryParamString()
    {
        return (GetHeader(HttpMessageHeaders.HttpQueryParams) ?? "").ToString() ?? string.Empty;
    }

    /// Retrieves the HTTP response status code from the message headers.
    /// <return>The status code of the message, or null if not available</return>
    public HttpStatusCode? GetStatusCode()
    {
        var statusCode = GetHeader(HttpMessageHeaders.HttpStatusCode);

        if (statusCode != null)
            return statusCode switch
            {
                HttpStatusCode httpStatusCode => httpStatusCode,
                int integer => (HttpStatusCode)integer,
                _ => (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), statusCode.ToString() ?? string.Empty)
            };
        return null;
    }

    /// Gets the HTTP response reason phrase.
    /// <return>The reason phrase of the message.</return>
    public string GetReasonPhrase()
    {
        var reasonPhrase = GetHeader(HttpMessageHeaders.HttpReasonPhrase);

        return reasonPhrase?.ToString()!;
    }

    /// Retrieves the HTTP version from the message headers.
    /// <return>The HTTP version of the message</return>
    public string GetVersion()
    {
        var version = GetHeader(HttpMessageHeaders.HttpVersion);

        return version?.ToString()!;
    }

    /// Gets the request path after the context path.
    /// <returns>The request path of the message</returns>
    public string GetPath()
    {
        var path = GetHeader(IEndpointUriResolver.RequestPathHeaderName);

        return path?.ToString()!;
    }

    /// Retrieves the list of cookies associated with this HTTP message.
    /// <returns>The list of cookies for this message</returns>
    public List<Cookie> GetCookies()
    {
        return _cookies.Values.ToList();
    }

    /// Retrieves a map containing cookies, where each cookie is mapped by its name as the key.
    /// <return> A dictionary of Cookies identified by the cookie name. </return>
    private Dictionary<string, Cookie> GetCookiesMap()
    {
        return _cookies;
    }

    /// Sets the cookies for the HTTP message.
    /// <param name="cookies">
    ///     The cookies to set, provided as an array of Cookie objects. If null, the cookies collection is
    ///     cleared.
    /// </param>
    public void SetCookies(Cookie[]? cookies)
    {
        _cookies.Clear();
        if (cookies != null)
            foreach (var cookie in cookies)
                Cookie(cookie);
    }

    /// Adds a new cookie to the HTTP message. If a cookie with the same name
    /// already exists, it will be replaced with the new cookie.
    /// <param name="cookie">The Cookie to be added or updated in the HTTP message.</param>
    /// <return>Returns the modified instance of HttpMessage with the new cookie set.</return>
    public HttpMessage Cookie(Cookie cookie)
    {
        _cookies[cookie.Name] = cookie;

        SetHeader(
            HttpMessageHeaders.HttpCookiePrefix + cookie.Name,
            _cookieConverter.GetCookieString(cookie));

        return this;
    }

    /// Parses a complete HTTP request dump into an HttpMessage object.
    /// This method processes the request dump string and extracts the HTTP
    /// method, URI, and version, encapsulating them in a HttpMessage instance.
    /// <param name="requestData">The request dump to parse.</param>
    /// <return>The parsed dump as an HttpMessage.</return>
    public static HttpMessage FromRequestData(string requestData)
    {
        using var reader = new StringReader(requestData);
        var request = new HttpMessage();

        var requestLine = reader.ReadLine()?.Split(' ');
        if (requestLine?.Length > 0) request.Method(new HttpMethod(requestLine[0]));

        if (requestLine?.Length > 1) request.Uri(requestLine[1]);

        if (requestLine?.Length > 2) request.Version(requestLine[2]);

        return ParseHttpMessage(new StringReader(requestData), request);
    }

    /// Parses a complete HTTP response dump and converts it into an HttpMessage instance.
    /// This method processes the response data, extracting details such as the HTTP version and status code.
    /// <param name="responseData">The response dump to parse</param>
    /// <return>The parsed response as an HttpMessage</return>
    public static HttpMessage FromResponseData(string responseData)
    {
        using var reader = new StringReader(responseData);
        var response = new HttpMessage();

        var statusLine = reader.ReadLine()?.Split(' ');
        if (statusLine?.Length > 0) response.Version(statusLine[0]);

        if (statusLine?.Length > 1) response.Status((HttpStatusCode)int.Parse(statusLine[1]));

        return ParseHttpMessage(new StringReader(responseData), response);
    }

    /// Adds a query parameter to the HTTP message. If the parameter with the given
    /// name already exists, the new value is added to the existing list of values
    /// for that parameter.
    /// <param name="name">The name of the query parameter to add.</param>
    /// <param name="value">The value of the query parameter to add.</param>
    private void AddQueryParam(string? name, string? value)
    {
        if (name != null)
        {
            if (!_queryParams.TryGetValue(name, out var values))
            {
                values = [];
                _queryParams[name] = values;
            }

            values.AddFirst(value!);
        }
    }

    /// Formats a query parameter entry as a string by concatenating the
    /// parameter key with its values, using `=` and `,` as separators.
    /// <param name="entry">
    ///     A key-value pair representing the query parameter, where the key
    ///     is the parameter name and the value is a list of parameter values.
    /// </param>
    /// <return>
    ///     A string representation of the query parameter, where each value
    ///     is appended to the key using an equals sign, and multiple values
    ///     are separated by commas.
    /// </return>
    private string OutputQueryParam(KeyValuePair<string, LinkedList<string>> entry)
    {
        return string.Join(",",
            entry.Value.Select(entryValue => entry.Key + (entryValue != null ? "=" + entryValue : "")));
    }

    /// Parses an HTTP message from the given StringReader and constructs an HttpMessage object.
    /// Reads headers and payload from the provided reader and populates the given HttpMessage instance.
    /// <param name="reader">The StringReader object from which the HTTP message data will be read.</param>
    /// <param name="message">The HttpMessage instance to be populated with data from the reader.</param>
    /// <returns>An HttpMessage object populated with headers and payload extracted from the reader.</returns>
    /// <exception cref="AgenixSystemException">Thrown when there is an invalid header syntax in the input data.</exception>
    private static HttpMessage ParseHttpMessage(StringReader reader, HttpMessage message)
    {
        string? line;
        while ((line = reader.ReadLine()) != null && StringUtils.HasText(line))
        {
            if (!line.Contains(':'))
                throw new AgenixSystemException(
                    $"Invalid header syntax in line - expected 'key:value' but was '{line}'");

            var keyValue = line.Split(':');
            message.SetHeader(keyValue[0].Trim(), keyValue[1].Trim());
        }

        var bodyBuilder = new StringBuilder();
        while ((line = reader.ReadLine()) != null && StringUtils.HasText(line))
            bodyBuilder.Append(line).Append(Environment.NewLine);

        message.Payload = bodyBuilder.ToString().Trim();

        return message;
    }
}