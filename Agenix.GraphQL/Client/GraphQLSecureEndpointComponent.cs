namespace Agenix.GraphQL.Client;

/// <summary>
/// GraphQL endpoint component that enforces HTTPS for secure communication.
/// Automatically uses HTTPS for HTTP requests and WSS for WebSocket subscriptions.
/// </summary>
public class GraphQLSecureEndpointComponent : GraphQLEndpointComponent
{
    /// <summary>
    /// Creates a secure GraphQL endpoint component with HTTPS enabled by default.
    /// </summary>
    public GraphQLSecureEndpointComponent() : this("graphql-secure")
    {
    }

    /// <summary>
    /// Creates a secure GraphQL endpoint component with HTTPS enabled by default.
    /// </summary>
    /// <param name="name">The name identifier for this secure GraphQL endpoint component.</param>
    public GraphQLSecureEndpointComponent(string name) : base(name)
    {
    }

    /// <summary>
    /// Gets the HTTPS URI scheme used for secure GraphQL endpoints.
    /// </summary>
    /// <returns>A string representing the HTTPS scheme ("https://").</returns>
    protected override string Scheme => "https://";

    /// <summary>
    /// Gets the secure WebSocket URI scheme used for secure GraphQL subscriptions.
    /// </summary>
    /// <returns>A string representing the secure WebSocket scheme ("wss://").</returns>
    protected override string WebSocketScheme => "wss://";
}
