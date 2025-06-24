using Agenix.Api.Endpoint;
using Agenix.Http.Client;
using HttpClient = Agenix.Http.Client.HttpClient;

namespace Agenix.Http.Tests.Endpoint.Builder;

/// <summary>
/// Unit tests for the Http endpoint component lookup and builder functionality.
/// Tests the registration and discovery of Http endpoint builders in the framework.
/// </summary>
[TestFixture]
public class HttpEndpointBuilderTests
{
    /// <summary>
    /// Tests that Http endpoint builders are properly registered and can be looked up by name.
    /// Verifies that both client and server Http endpoints are available in the endpoint registry.
    /// </summary>
    [Test]
    public void ShouldLookupEndpoints()
    {
        // Act
        var endpointBuilders = IEndpointBuilder<IEndpoint>.Lookup();

        // Assert
        Assert.That(endpointBuilders.ContainsKey("http.client"), Is.True,
            "Http client endpoint builder should be registered");
    }

    /// <summary>
    /// Tests that specific Http endpoint builders can be looked up individually by name
    /// and that they return the correct builder types.
    /// </summary>
    [Test]
    public void ShouldLookupEndpoint()
    {
        // Act & Assert - Http Client
        var graphqlClientBuilder = IEndpointBuilder<IEndpoint>.Lookup("http.client");
        Assert.That(graphqlClientBuilder.IsPresent, Is.True,
            "Http client endpoint builder should be found");
        Assert.That(graphqlClientBuilder.Value.GetType(), Is.EqualTo(typeof(HttpClientBuilder)),
            "Should return HttpClientBuilder instance");
    }

    /// <summary>
    /// Tests that Http endpoint builders are properly configured with expected default settings.
    /// Verifies that the builders can create functional endpoint instances.
    /// </summary>
    [Test]
    public void ShouldCreateGraphQlEndpoints()
    {
        // Arrange
        var clientBuilder = IEndpointBuilder<IEndpoint>.Lookup("http.client");

        // Act
        var clientEndpoint = clientBuilder.Value.Build();

        // Assert
        Assert.That(clientEndpoint, Is.Not.Null, "Http client endpoint should be created");
        Assert.That(clientEndpoint, Is.InstanceOf<HttpClient>(),
            "Client endpoint should be HttpClient instance");
    }
}
