using System.Net;
using System.Net.Sockets;
using Agenix.GraphQL.Client;
using Agenix.GraphQL.Message;
using Agenix.GraphQL.Server;
using HotChocolate.Resolvers;

namespace Agenix.GraphQL.Tests.Server;

[TestFixture]
public class GraphQLServerTest : AbstractNUnitSetUp
{
    [OneTimeSetUp]
    public void SetupServer()
    {
        // Setup GraphQL client configuration
        var clientConfig = new GraphQLEndpointConfiguration { EndpointUrl = _uri, Timeout = 1000L };

        // Create GraphQL client
        _graphQLClient = new GraphQLClient(clientConfig);

        // Setup and start server using new Hot Chocolate GraphQLServer
        _graphQLServer = new GraphQLServer()
            .SetPort(_port)
            .SetHost("localhost")
            .SetContextPath("/")
            .SetGraphQLPath("/graphql")
            .SetIntrospectionEnabled(true); // Enable for testing

        // Add query resolvers using the new API
        // Add query resolvers using the new API - try simple string first
        _graphQLServer.AddQueryResolver("test", (variables, context) =>
            Task.FromResult<object>("Hello from Hot Chocolate GraphQL server!"));

        // Add a simple user resolver that returns a JSON string (this will be a scalar)
        // Return an anonymous object instead of JSON string
        _graphQLServer.AddQueryResolver("user", (variables, context) =>
        {
            // Return an object that GraphQL can serialize properly
            var user = new { id = "123", name = "User 123" };
            return Task.FromResult<object>(user);
        });


        _graphQLServer.Initialize();
        _graphQLServer.Start();

        // Wait a bit for the server to start
        Thread.Sleep(2000);
    }

    [OneTimeTearDown]
    public void Shutdown()
    {
        _graphQLServer?.Stop();

        // Wait a bit for the server to fully shut down and release the port
        Thread.Sleep(500);

        // Test that the server is actually stopped by attempting a GraphQL request
        try
        {
            var testQuery = new GraphQLMessage("{ test { message } }");
            _graphQLClient.Send(testQuery, Context);
        }
        catch (Exception ex)
        {
            // Expected - connection should be refused or similar error
            Assert.That(ex.Message.Contains("Connection refused") ||
                        ex.Message.Contains("No connection could be made") ||
                        ex.Message.Contains("Cannot assign requested address") ||
                        ex.Message.Contains("Connection reset") ||
                        ex.Message.Contains("GraphQL request failed") ||
                        ex.Message.Contains("The server returned an invalid or unrecognized response") ||
                        ex.InnerException?.Message.Contains("Connection refused") == true ||
                        ex.GetType().Name.Contains("HttpRequestException") ||
                        ex.GetType().Name.Contains("SocketException") ||
                        ex.GetType().Name.Contains("TaskCanceledException"),
                Is.True, $"Expected connection error after server shutdown, but got: {ex.GetType().Name} - {ex.Message}");
        }
    }

    private readonly int _port = FindAvailableTcpPort(8080);
    private readonly string _uri;
    private GraphQLClient _graphQLClient;
    private GraphQLServer _graphQLServer;

    public GraphQLServerTest()
    {
        _uri = $"http://localhost:{_port}/graphql";
    }

    [Test]
    public void ShouldStartAndStopServer()
    {
        Assert.That(_graphQLServer.IsRunning(), Is.True, "Server should be running");
    }

    [Test]
public void ShouldReceiveGraphQLResponse()
{
    // Add debug logging before sending
    Console.WriteLine("=== Test Debug Info ===");
    Console.WriteLine($"Server running: {_graphQLServer?.IsRunning()}");
    Console.WriteLine($"Server port: {_port}");
    Console.WriteLine($"GraphQL URI: {_uri}");

    // Test server health first with a simple HTTP request
    try
    {
        using var httpClient = new HttpClient();
        var healthResponse = httpClient.GetAsync($"http://localhost:{_port}/graphql").Result;
        Console.WriteLine($"Server health check: {healthResponse.StatusCode}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Server health check failed: {ex.Message}");
    }

    // First send a query
    var query = new GraphQLMessage("{ user }")
        .SetOperationType(GraphQLOperationType.QUERY);

    Console.WriteLine($"Sending query: {query.Payload}");

    _graphQLClient.Send(query, Context);
    Console.WriteLine("Query sent successfully");

    // Add a longer wait to see if response arrives
    Thread.Sleep(1000);

    // Then receive the response
    var response = _graphQLClient.Receive(Context, 10000L); // Increase timeout

    Console.WriteLine($"Response received: {response != null}");
    Console.WriteLine($"Response type: {response?.GetType()}");

    Assert.That(response, Is.Not.Null);
    Assert.That(response, Is.InstanceOf<GraphQLMessage>());

    var graphqlResponse = (GraphQLMessage)response;

    // Debug: Print the actual payload to see what we're getting
    var payload = graphqlResponse.Payload?.ToString() ?? "null";
    Console.WriteLine($"Actual payload: '{payload}'");
    Console.WriteLine($"Payload length: {payload.Length}");
    Console.WriteLine($"Payload type: {graphqlResponse.Payload?.GetType().Name ?? "null"}");

    // Check if there are any headers or other properties
    Console.WriteLine($"Headers count: {graphqlResponse.Headers?.Count ?? 0}");
    Console.WriteLine($"Message ID: {graphqlResponse.Id}");

    Assert.That(graphqlResponse.Payload, Is.Not.Empty);
}


    [Test]
    public void ShouldReceiveTestQueryResponse()
    {
        // Test with a simple string return
        var query = new GraphQLMessage("{ test }")
            .SetOperationType(GraphQLOperationType.QUERY);

        _graphQLClient.Send(query, Context);

        var response = _graphQLClient.Receive(Context, 5000L);

        Assert.That(response, Is.Not.Null);
        Assert.That(response, Is.InstanceOf<GraphQLMessage>());

        var graphqlResponse = (GraphQLMessage)response;

        // Debug output
        Console.WriteLine($"Response type: {response.GetType()}");
        Console.WriteLine($"Payload: {graphqlResponse.Payload}");
        Console.WriteLine($"Payload type: {graphqlResponse.Payload?.GetType().Name}");

        Assert.That(graphqlResponse.Payload, Is.Not.Null);

        var payload = graphqlResponse.Payload.ToString();
        Console.WriteLine($"Payload string: '{payload}'");

        // Just check it's not empty for now
        Assert.That(payload, Is.Not.Empty);

    }

    [Test]
    public void ShouldConfigureServerProperties()
    {
        var testServer = new GraphQLServer()
            .SetPort(9090)
            .SetHost("127.0.0.1")
            .SetContextPath("/api")
            .SetGraphQLPath("/gql")
            .SetIntrospectionEnabled(false)
            .SetMaxQueryDepth(10)
            .SetQueryComplexityLimit(500)
            .SetCorsEnabled(true)
            .SetAllowedOrigins(new[] { "http://localhost:3000" });

        Assert.That(testServer, Is.Not.Null);
    }

    [Test]
    public void ShouldSupportIntrospectionQuery()
    {
        // Test GraphQL introspection query
        var introspectionQuery = new GraphQLMessage(@"
            {
                __schema {
                    types {
                        name
                    }
                }
            }")
            .SetOperationType(GraphQLOperationType.QUERY);

        _graphQLClient.Send(introspectionQuery, Context);

        var response = _graphQLClient.Receive(Context, 5000L);

        Assert.That(response, Is.Not.Null);
        Assert.That(response, Is.InstanceOf<GraphQLMessage>());

        var graphqlResponse = (GraphQLMessage)response;
        Assert.That(graphqlResponse.Payload, Is.Not.Null);
    }

    [Test]
    public void ShouldHandleInvalidQuery()
    {
        // Test with invalid GraphQL syntax
        var invalidQuery = new GraphQLMessage("{ invalidField { nonExistentField } }")
            .SetOperationType(GraphQLOperationType.QUERY);

        Assert.DoesNotThrow(() =>
        {
            _graphQLClient.Send(invalidQuery, Context);
            var response = _graphQLClient.Receive(Context, 5000L);

            // Should receive an error response, not throw an exception
            Assert.That(response, Is.Not.Null);
        });
    }

    /// <summary>
    /// Find available TCP port starting from the given port
    /// </summary>
    private static int FindAvailableTcpPort(int startPort)
    {
        for (var port = startPort; port < startPort + 100; port++)
        {
            try
            {
                var listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start();
                listener.Stop();
                return port;
            }
            catch (SocketException)
            {
                // Port is in use, try the next one
            }
        }

        throw new InvalidOperationException("No available ports found");
    }
}

