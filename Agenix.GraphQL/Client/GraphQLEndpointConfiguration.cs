using Agenix.Api.Endpoint.Resolver;
using Agenix.Api.Message;
using Agenix.Core.Endpoint;
using Agenix.Core.Endpoint.Resolver;
using Agenix.Core.Message;
using Agenix.GraphQL.Interceptor;
using Agenix.GraphQL.Message;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;

namespace Agenix.GraphQL.Client;

public class GraphQLEndpointConfiguration : AbstractPollableEndpointConfiguration
{
    private GraphQLHttpClient? _graphQLClient;

    public GraphQLEndpointConfiguration()
    {
        ClientHandlers.Add(new LoggingGraphQLClientHandler(new HttpClientHandler()));
    }

    /// <summary>
    ///     Represents the GraphQL endpoint URL used as the destination for GraphQL requests.
    /// </summary>
    public string? EndpointUrl { get; set; }

    /// <summary>
    ///     Specifies the character set used for encoding content in GraphQL communications.
    /// </summary>
    public string Charset { get; set; } = "UTF-8";

    /// <summary>
    ///     Specifies the MIME type of the content being sent or received by the GraphQL endpoint.
    ///     Defaults to application/json as per GraphQL specification.
    /// </summary>
    public string ContentType { get; set; } = "application/json";

    /// <summary>
    ///     Determines whether cookies should be handled automatically by the GraphQL client.
    /// </summary>
    public bool HandleCookies { get; set; } = false;

    /// <summary>
    ///     Specifies whether to use WebSocket for subscriptions.
    /// </summary>
    public bool UseWebSocketForSubscriptions { get; set; } = true;

    /// <summary>
    ///     The WebSocket endpoint URL for GraphQL subscriptions.
    ///     If not specified, will derive from EndpointUrl by replacing http/https with ws/wss.
    /// </summary>
    public string? WebSocketEndpointUrl { get; set; }

    /// <summary>
    ///     Default headers to be sent with every GraphQL request.
    /// </summary>
    public Dictionary<string, string> DefaultHeaders { get; set; } = new();

    /// <summary>
    ///     Endpoint client handlers for HTTP operations.
    /// </summary>
    public List<DelegatingHandler> ClientHandlers { get; set; } = new();

    /// <summary>
    ///     Resolves the dynamic endpoint URI for sending messages to dynamic endpoints,
    ///     determining the target endpoint based on message headers or payload.
    /// </summary>
    public IEndpointUriResolver EndpointUriResolver { get; set; } = new DynamicEndpointUriResolver();

    /// <summary>
    ///     Provides a mechanism to convert messages between GraphQL requests and GraphQL responses.
    /// </summary>
    public GraphQLMessageConverter MessageConverter { get; set; } = new();

    /// <summary>
    ///     Reply message correlator for handling GraphQL response correlation.
    /// </summary>
    public IMessageCorrelator Correlator { get; set; } = new DefaultMessageCorrelator();

    /// <summary>
    ///     GraphQL serializer options for request/response serialization.
    /// </summary>
    public NewtonsoftJsonSerializer SerializerOptions { get; set; } = new();

    /// <summary>
    ///     Authentication configuration for GraphQL requests.
    /// </summary>
    public GraphQLAuthenticationConfiguration? Authentication { get; set; }

    /// <summary>
    ///     Retry policy configuration for failed GraphQL requests.
    /// </summary>
    public GraphQLRetryPolicy RetryPolicy { get; set; } = new();


    /// <summary>
    ///     Provides an instance of the GraphQLHttpClient used to send GraphQL requests
    ///     and receive GraphQL responses from a GraphQL endpoint.
    ///     This client is configured using the specified handlers and options.
    /// </summary>
    public GraphQLHttpClient GraphQLClient
    {
        get
        {
            if (_graphQLClient == null)
            {
                var graphQlClientFactory = new GraphQLClientFactory();
                graphQlClientFactory.AddHandlers(ClientHandlers);

                var options = new GraphQLHttpClientOptions
                {
                    EndPoint = new Uri(EndpointUrl ??
                                       throw new InvalidOperationException("EndpointUrl must be configured")),
                    HttpMessageHandler = graphQlClientFactory.CreateMessageHandler()
                };

                if (UseWebSocketForSubscriptions && !string.IsNullOrEmpty(WebSocketEndpointUrl))
                {
                    options.WebSocketEndPoint = new Uri(WebSocketEndpointUrl);
                }

                _graphQLClient = new GraphQLHttpClient(options, SerializerOptions);

                // Set timeout on the underlying HttpClient
                _graphQLClient.HttpClient.Timeout = TimeSpan.FromMilliseconds(Timeout);

                // Add default headers
                foreach (var header in DefaultHeaders)
                {
                    _graphQLClient.HttpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            return _graphQLClient;
        }
        set => _graphQLClient = value;
    }

    /// <summary>
    ///     Validates the configuration to ensure all required properties are set.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when required configuration is missing.</exception>
    public void Validate()
    {
        if (string.IsNullOrEmpty(EndpointUrl))
        {
            throw new InvalidOperationException("EndpointUrl is required for GraphQL endpoint configuration");
        }

        if (UseWebSocketForSubscriptions && string.IsNullOrEmpty(WebSocketEndpointUrl))
        {
            // Auto-derive WebSocket URL from HTTP URL
            WebSocketEndpointUrl = EndpointUrl.Replace("http://", "ws://").Replace("https://", "wss://");
        }
    }


    /// <summary>
    ///     Configuration options for GraphQL serialization.
    /// </summary>
    public class GraphQLSerializerOptions
    {
        /// <summary>
        ///     Whether to indent JSON output for readability.
        /// </summary>
        public bool IndentJson { get; set; } = false;

        /// <summary>
        ///     Custom JSON serializer settings.
        /// </summary>
        public object? JsonSerializerSettings { get; set; }
    }

    /// <summary>
    ///     Authentication configuration for GraphQL requests.
    /// </summary>
    public class GraphQLAuthenticationConfiguration
    {
        /// <summary>
        ///     Authentication type (Bearer, Basic, etc.).
        /// </summary>
        public string Type { get; set; } = "Bearer";

        /// <summary>
        ///     Authentication token or credentials.
        /// </summary>
        public string? Token { get; set; }

        /// <summary>
        ///     Username for basic authentication.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        ///     Password for basic authentication.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        ///     Custom authentication headers.
        /// </summary>
        public Dictionary<string, string> CustomHeaders { get; set; } = new();
    }

    /// <summary>
    ///     Retry policy configuration for GraphQL requests.
    /// </summary>
    public class GraphQLRetryPolicy
    {
        /// <summary>
        ///     Maximum number of retry attempts.
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        ///     Delay between retry attempts in milliseconds.
        /// </summary>
        public int RetryDelayMilliseconds { get; set; } = 1000;

        /// <summary>
        ///     Whether to use exponential backoff for retry delays.
        /// </summary>
        public bool UseExponentialBackoff { get; set; } = true;

        /// <summary>
        ///     HTTP status codes that should trigger a retry.
        /// </summary>
        public List<int> RetryableStatusCodes { get; set; } = [408, 429, 500, 502, 503, 504];
    }
}
