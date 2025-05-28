using Agenix.Api.Endpoint.Resolver;
using Agenix.Api.Message;
using Agenix.Core.Endpoint;
using Agenix.Core.Endpoint.Resolver;
using Agenix.Core.Message;
using Agenix.Http.Interceptor;
using Agenix.Http.Message;

namespace Agenix.Http.Client;

/// Represents the configuration for an HTTP endpoint, inheriting from AbstractPollableEndpointConfiguration
/// to include pollable behavior and endpoint configuration capabilities.
public class HttpEndpointConfiguration : AbstractPollableEndpointConfiguration
{
    private System.Net.Http.HttpClient _httpClient = null!;

    public HttpEndpointConfiguration()
    {
        ClientHandlers.Add(new LoggingClientHandler(new HttpClientHandler()));
    }

    /// Represents the HTTP URL endpoint used as the destination for HTTP requests.
    public string? RequestUrl { get; set; } = null;

    /// Specifies the HTTP method to be used when sending HTTP requests.
    /// Initialized to use the POST method by default.
    public HttpMethod? RequestMethod { get; set; } = HttpMethod.Post;

    /// Specifies the character set used for encoding content in HTTP communications.
    public string Charset { get; set; } = "UTF-8";

    /// Specifies the MIME type of the content being sent or received by the HTTP endpoint.
    public string ContentType { get; set; } = "text/plain";

    public bool HandleCookies { get; set; } = false;

    /// <summary>
    ///     Endpoint clientInterceptors
    /// </summary>
    public List<DelegatingHandler> ClientHandlers { get; set; } = new();

    /// <summary>
    ///     Resolves the dynamic endpoint URI for sending messages to dynamic endpoints,
    ///     determining the target endpoint based on message headers or payload.
    /// </summary>
    public IEndpointUriResolver EndpointUriResolver { get; set; } = new DynamicEndpointUriResolver();

    /// Provides a mechanism to convert messages between HTTP requests and HTTP responses.
    public HttpMessageConverter MessageConverter { get; set; } = new();

    /**
     * Reply message correlator
     */
    public IMessageCorrelator Correlator { get; set; } = new DefaultMessageCorrelator();

    /// Provides an instance of the HttpClient used to send HTTP requests
    /// and receive HTTP responses from a resource identified by a URI.
    /// This client is configured using an internal message handler,
    /// which can include capabilities such as logging or authentication.
    public System.Net.Http.HttpClient HttpClient
    {
        get
        {
            if (_httpClient == null)
            {
                var httpClientFactory = new HttpClientFactory();
                httpClientFactory.AddHandlers(ClientHandlers);
                _httpClient = httpClientFactory.CreateHttpClient();
            }

            return _httpClient;
        }
        set => _httpClient = value;
    }
}
