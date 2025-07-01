using System.Net.Http.Headers;
using System.Text;

namespace Agenix.Http.Interceptor;

/// <summary>
/// A custom HttpClientHandler that supports sending HTTP requests with basic authentication credentials.
/// </summary>
/// <remarks>
/// This handler can be used to attach basic authentication headers to outgoing HTTP requests,
/// providing seamless integration for authenticated API interactions.
/// </remarks>
public class BasicAuthenticationDelegatingHandler : DelegatingHandler
{
    private readonly string _credentials;

    /// <summary>
    /// A custom HttpClientHandler that supports sending HTTP requests with basic authentication credentials.
    /// </summary>
    /// <remarks>
    /// This handler can be used to attach basic authentication headers to outgoing HTTP requests,
    /// providing seamless integration for authenticated API interactions.
    /// </remarks>
    public BasicAuthenticationDelegatingHandler(string username, string password)
    {
        _credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
    }

    /// <summary>
    /// Sends an HTTP request with an added basic authentication header and returns the response.
    /// </summary>
    /// <param name="request">The HTTP request message to send.</param>
    /// <param name="cancellationToken">A token to cancel the operation, if required.</param>
    /// <returns>The HTTP response message resulting from the request.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _credentials);
        return await base.SendAsync(request, cancellationToken);
    }
}

