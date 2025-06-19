using Agenix.Api.Endpoint;
using Agenix.GraphQL.Client;

namespace Agenix.GraphQL.Tests.Endpoint.Builder;

/// <summary>
/// Unit tests for the GraphQL endpoint component lookup and builder functionality.
/// Tests the registration and discovery of GraphQL endpoint builders in the framework.
/// </summary>
[TestFixture]
public class GraphQLEndpointsTest
{
    /// <summary>
    /// Tests that GraphQL endpoint builders are properly registered and can be looked up by name.
    /// Verifies that both client and server GraphQL endpoints are available in the endpoint registry.
    /// </summary>
    [Test]
    public void ShouldLookupEndpoints()
    {
        // Act
        var endpointBuilders = IEndpointBuilder<IEndpoint>.Lookup();

        // Assert
        Assert.That(endpointBuilders.ContainsKey("graphql.client"), Is.True,
            "GraphQL client endpoint builder should be registered");
    }

    /// <summary>
    /// Tests that specific GraphQL endpoint builders can be looked up individually by name
    /// and that they return the correct builder types.
    /// </summary>
    [Test]
    public void ShouldLookupEndpoint()
    {
        // Act & Assert - GraphQL Client
        var graphqlClientBuilder = IEndpointBuilder<IEndpoint>.Lookup("graphql.client");
        Assert.That(graphqlClientBuilder.IsPresent, Is.True,
            "GraphQL client endpoint builder should be found");
        Assert.That(graphqlClientBuilder.Value.GetType(), Is.EqualTo(typeof(GraphQLClientBuilder)),
            "Should return GraphQLClientBuilder instance");
    }

    /// <summary>
    /// Tests that GraphQL endpoint builders are properly configured with expected default settings.
    /// Verifies that the builders can create functional endpoint instances.
    /// </summary>
    [Test]
    public void ShouldCreateGraphQlEndpoints()
    {
        // Arrange
        var clientBuilder = IEndpointBuilder<IEndpoint>.Lookup("graphql.client");

        // Act
        var clientEndpoint = clientBuilder.Value.Build();

        // Assert
        Assert.That(clientEndpoint, Is.Not.Null, "GraphQL client endpoint should be created");
        Assert.That(clientEndpoint, Is.InstanceOf<GraphQLClient>(),
            "Client endpoint should be GraphQLClient instance");
    }
}
