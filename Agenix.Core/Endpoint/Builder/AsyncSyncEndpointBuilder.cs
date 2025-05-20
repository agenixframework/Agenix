using Agenix.Api.Endpoint;

namespace Agenix.Core.Endpoint.Builder;

/// <summary>
///     Provides a builder for both asynchronous and synchronous endpoints, enabling the construction of endpoints that
///     support both types of operations.
/// </summary>
/// <typeparam name="TA">The type of the asynchronous endpoint builder.</typeparam>
/// <typeparam name="TS">The type of the synchronous endpoint builder.</typeparam>
public class AsyncSyncEndpointBuilder<TA, TS>
    where TA : IEndpointBuilder<IEndpoint>
    where TS : IEndpointBuilder<IEndpoint>
{
    private readonly TA _asyncEndpointBuilder;
    private readonly TS _syncEndpointBuilder;

    /// <summary>
    ///     Default constructor setting the sync and async builder implementation.
    /// </summary>
    /// <param name="asyncEndpointBuilder">The asynchronous endpoint builder.</param>
    /// <param name="syncEndpointBuilder">The synchronous endpoint builder.</param>
    public AsyncSyncEndpointBuilder(TA asyncEndpointBuilder, TS syncEndpointBuilder)
    {
        _asyncEndpointBuilder = asyncEndpointBuilder;
        _syncEndpointBuilder = syncEndpointBuilder;
    }

    /// <summary>
    ///     Gets the async endpoint builder.
    /// </summary>
    /// <returns>The asynchronous endpoint builder.</returns>
    public TA Asynchronous()
    {
        return _asyncEndpointBuilder;
    }

    /// <summary>
    ///     Gets the sync endpoint builder.
    /// </summary>
    /// <returns>The synchronous endpoint builder.</returns>
    public TS Synchronous()
    {
        return _syncEndpointBuilder;
    }
}