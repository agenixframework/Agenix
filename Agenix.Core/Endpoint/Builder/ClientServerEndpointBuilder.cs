namespace Agenix.Core.Endpoint.Builder;

/// <summary>
///     Represents a builder that constructs client and server endpoints.
/// </summary>
/// <typeparam name="TC">The type of the client builder.</typeparam>
/// <typeparam name="TS">The type of the server builder.</typeparam>
public class ClientServerEndpointBuilder<TC, TS>
    where TC : IEndpointBuilder<IEndpoint>
    where TS : IEndpointBuilder<IEndpoint>
{
    private readonly TC _clientBuilder;
    private readonly TS _serverBuilder;

    /// <summary>
    ///     Constructs a ClientServerEndpointBuilder with the specified client and server builders.
    /// </summary>
    /// <typeparam name="TC">The type of the client builder.</typeparam>
    /// <typeparam name="TS">The type of the server builder.</typeparam>
    /// <param name="clientBuilder">The client builder implementation.</param>
    /// <param name="serverBuilder">The server builder implementation.</param>
    public ClientServerEndpointBuilder(TC clientBuilder, TS serverBuilder)
    {
        _clientBuilder = clientBuilder;
        _serverBuilder = serverBuilder;
    }

    /// <summary>
    ///     Gets the client builder.
    /// </summary>
    /// <returns>The client builder.</returns>
    public TC Client()
    {
        return _clientBuilder;
    }

    /// <summary>
    ///     Gets the server builder.
    /// </summary>
    /// <returns>The server builder.</returns>
    public TS Server()
    {
        return _serverBuilder;
    }
}