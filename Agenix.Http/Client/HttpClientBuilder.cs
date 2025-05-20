using Agenix.Api.Endpoint.Resolver;
using Agenix.Core.Endpoint;
using Agenix.Core.Endpoint.Resolver;
using Agenix.Core.Message;
using Agenix.Http.Message;

namespace Agenix.Http.Client;

public class HttpClientBuilder: AbstractEndpointBuilder<HttpClient>
{
    ///  Endpoint target
    private readonly HttpClient _endpoint = new();

    /// <summary>
    /// Provides the implementation for retrieving the HTTP client endpoint instance
    /// initialized within the builder.
    /// </summary>
    /// <returns>
    /// The initialized instance of HttpClient representing the endpoint.
    /// </returns>
    protected override HttpClient GetEndpoint()
    {
        return _endpoint;
    }

    /// <summary>
    /// Sets the request URL for the HTTP client endpoint configuration.
    /// </summary>
    /// <param name="uri">
    /// The URI to be set as the request URL.
    /// </param>
    /// <returns>
    /// The current instance of <see cref="HttpClientBuilder"/> to allow method chaining.
    /// </returns>
    public HttpClientBuilder RequestUrl(string uri)
    {
        _endpoint.EndpointConfiguration.RequestUrl = uri;
        return this;
    }

    /// <summary>
    /// Represents a configurable client designed to handle HTTP communication
    /// within the application, built using the HttpClientBuilder.
    /// </summary>
    public HttpClientBuilder HttpClient(System.Net.Http.HttpClient httpClient) {
        _endpoint.EndpointConfiguration.HttpClient = httpClient;
        return this;
    }

    /// <summary>
    /// Configures the HTTP request method for the HTTP client endpoint.
    /// </summary>
    /// <param name="requestMethod">
    /// The HTTP request method to be set, such as GET, POST, PUT, or DELETE.
    /// </param>
    /// <returns>
    /// The current instance of <see cref="HttpClientBuilder"/> to enable method chaining.
    /// </returns>
    public HttpClientBuilder RequestMethod(HttpMethod requestMethod)
    {
        _endpoint.EndpointConfiguration.RequestMethod = requestMethod;
        return this;
    }

    /// <summary>
    /// Sets the message converter to be used for converting HTTP messages in the endpoint configuration.
    /// </summary>
    /// <param name="messageConverter">
    /// The instance of <see cref="HttpMessageConverter"/> to handle message conversions.
    /// </param>
    /// <returns>
    /// An instance of <see cref="HttpClientBuilder"/> with the message converter configured.
    /// </returns>
    public HttpClientBuilder MessageConverter(HttpMessageConverter messageConverter)
    {
        _endpoint.EndpointConfiguration.MessageConverter = messageConverter;
        return this;
    }

    /// <summary>
    /// Configures the message correlator for the HTTP client. The correlator is used to manage
    /// the correlation of synchronous reply messages by applying a unique correlation key
    /// derived from message headers.
    /// </summary>
    /// <param name="correlator">
    /// An instance of <see cref="IMessageCorrelator"/> to be used for correlation of messages.
    /// </param>
    /// <returns>
    /// The current instance of <see cref="HttpClientBuilder"/> to allow method chaining.
    /// </returns>
    public HttpClientBuilder Correlator(IMessageCorrelator correlator)
    {
        _endpoint.EndpointConfiguration.Correlator = correlator;
        return this;
    }

    /// <summary>
    /// Configures the resolver for resolving endpoint URIs.
    /// </summary>
    /// <param name="resolver">
    /// The implementation of <see cref="IEndpointUriResolver"/> to resolve dynamic endpoint URIs based on message content.
    /// </param>
    /// <returns>
    /// The instance of <see cref="HttpClientBuilder"/> to allow method chaining for further configuration.
    /// </returns>
    public HttpClientBuilder EndpointResolver(IEndpointUriResolver resolver)
    {
        _endpoint.EndpointConfiguration.EndpointUriResolver = resolver;
        return this;
    }

    /// <summary>
    /// Sets the default character set for the HTTP client endpoint configuration.
    /// </summary>
    /// <param name="charset">
    /// The character set to be set as the default for the HTTP client endpoint.
    /// </param>
    /// <returns>
    /// The current instance of <see cref="HttpClientBuilder"/> to support method chaining.
    /// </returns>
    public HttpClientBuilder Charset(string charset)
    {
        _endpoint.EndpointConfiguration.Charset = charset;
        return this;
    }

    /// <summary>
    /// Configures the HTTP client to handle cookies for requests and responses.
    /// </summary>
    /// <param name="flag">
    /// A boolean value indicating whether cookies handling should be enabled or disabled.
    /// </param>
    /// <returns>
    /// The current instance of <see cref="HttpClientBuilder"/> to allow method chaining.
    /// </returns>
    public HttpClientBuilder HandleCookies(bool flag)
    {
        _endpoint.EndpointConfiguration.HandleCookies = flag;
        return this;
    }

    /// <summary>
    /// Sets the content type for the HTTP client endpoint configuration.
    /// </summary>
    /// <param name="contentType">
    /// The content type to be set (e.g., "application/json").
    /// </param>
    /// <returns>
    /// The current instance of HttpClientBuilder for fluent configuration.
    /// </returns>
    public HttpClientBuilder ContentType(string contentType)
    {
        _endpoint.EndpointConfiguration.ContentType = contentType;
        return this;
    }

    /// <summary>
    /// Sets the polling interval for the HTTP client endpoint.
    /// </summary>
    /// <param name="pollingInterval">
    /// The interval, in milliseconds, at which the endpoint performs polling operations.
    /// </param>
    /// <returns>
    /// The current instance of HttpClientBuilder for method chaining.
    /// </returns>
    public HttpClientBuilder PollingInterval(int pollingInterval)
    {
        _endpoint.EndpointConfiguration.PollingInterval = pollingInterval;
        return this;
    }

    /// <summary>
    /// Configures the default timeout value for the HTTP client endpoint.
    /// </summary>
    /// <param name="timeout">
    /// The timeout value in milliseconds to be set for the HTTP client endpoint.
    /// </param>
    /// <returns>
    /// An instance of HttpClientBuilder to allow further configuration calls.
    /// </returns>
    public HttpClientBuilder Timeout(long timeout)
    {
        _endpoint.EndpointConfiguration.Timeout = timeout;
        return this;
    }
}