using System.Collections.Generic;
using System.Text;
using Agenix.Api.Endpoint.Resolver;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Api.Util;
using Microsoft.Extensions.Primitives;

namespace Agenix.Core.Endpoint.Resolver;

/// <summary>
///     Endpoint uri resolver working on message headers. Resolver is searching for a specific header entry which holds the
///     actual target endpoint uri.
/// </summary>
public class DynamicEndpointUriResolver : IEndpointUriResolver
{
    /**
     * Default fallback uri
     */
    private string _defaultEndpointUri;

    /// Resolves the endpoint URI based on the message header entry with a fallback to the default URI.
    /// <param name="message">The incoming message containing headers with optional URI information.</param>
    /// <param name="defaultUri">The default URI to be used if no valid URI is found in the message headers.</param>
    /// <return>The resolved endpoint URI after appending any request path or query parameters.</return>
    /// <exception cref="AgenixSystemException">
    ///     Thrown when neither a header entry nor a default URI is available for
    ///     resolution.
    /// </exception>
    /// /
    public string ResolveEndpointUri(IMessage message, string defaultUri)
    {
        var headers = message.GetHeaders();

        string requestUri;
        if (headers.TryGetValue(IEndpointUriResolver.EndpointUriHeaderName, out var value))
        {
            requestUri = value.ToString();
        }
        else if (StringUtils.HasText(defaultUri))
        {
            requestUri = defaultUri;
        }
        else
        {
            requestUri = _defaultEndpointUri;
        }

        if (requestUri == null)
        {
            throw new AgenixSystemException("Unable to resolve dynamic endpoint uri! Neither header entry '" +
                                            IEndpointUriResolver.EndpointUriHeaderName +
                                            "' nor default endpoint uri is set");
        }

        requestUri = AppendRequestPath(requestUri, headers);
        requestUri = AppendQueryParams(requestUri, headers);

        return requestUri;
    }

    /// Appends an optional request path to the given endpoint URI using headers.
    /// Removes any trailing slashes from the URI and leading slashes from the path
    /// before concatenating them.
    /// <param name="uri">The base endpoint URI to append the request path to.</param>
    /// <param name="headers">A dictionary containing the headers which may include a request path.</param>
    /// <return>The endpoint URI with the appended request path, if provided in headers; otherwise, the original URI.</return>
    /// /
    private string AppendRequestPath(string uri, IDictionary<string, object> headers)
    {
        if (!headers.TryGetValue(IEndpointUriResolver.RequestPathHeaderName, out var value))
        {
            return uri;
        }

        var requestUri = uri;
        var path = value.ToString();

        while (requestUri.EndsWith('/'))
        {
            requestUri = requestUri.Substring(0, requestUri.Length - 1);
        }

        while (path.StartsWith('/') && path.Length > 0)
        {
            path = path.Length == 1 ? "" : path.Substring(1);
        }

        return requestUri + "/" + path;
    }

    /// Appends one or more query parameter key-value pairs to the provided URI.
    /// The key-value pairs should be formatted as a comma-separated string.
    /// This results in a URI with query parameters, for example, http://localhost:8080/test?param1=value1&param2=value2.
    /// <param name="uri">The original URI to which query parameters are to be appended.</param>
    /// <param name="headers">The collection of message headers which may include query parameters.</param>
    /// <return>Returns the URI with appended query parameters if available; otherwise, returns the original URI.</return>
    /// /
    private string AppendQueryParams(string uri, IDictionary<string, object> headers)
    {
        if (!headers.TryGetValue(IEndpointUriResolver.QueryParamHeaderName, out var value))
        {
            return uri;
        }

        var requestUri = uri;
        var queryParamBuilder = new StringBuilder();
        var queryParams = value.ToString() ?? string.Empty;

        var tokenizer = new StringTokenizer(queryParams, [',']);

        // Remove trailing slashes
        while (requestUri.EndsWith('/'))
        {
            requestUri = requestUri[..^1];
        }

        var isFirstToken = true;
        foreach (var token in tokenizer)
        {
            if (isFirstToken)
            {
                queryParamBuilder.Append('?').Append(token.ToString());
                isFirstToken = false;
            }
            else
            {
                queryParamBuilder.Append('&').Append(token.ToString());
            }
        }

        return requestUri + queryParamBuilder;
    }

    /// Sets the default fallback endpoint URI.
    /// <param name="newDefaultEndpointUri">the new default URI to set</param>
    /// /
    public void SetDefaultEndpointUri(string newDefaultEndpointUri)
    {
        _defaultEndpointUri = newDefaultEndpointUri;
    }
}
