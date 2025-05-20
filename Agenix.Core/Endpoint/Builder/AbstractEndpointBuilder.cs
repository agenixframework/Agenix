using Agenix.Api.Endpoint;

namespace Agenix.Core.Endpoint.Builder;

/// <summary>
///     The AbstractEndpointBuilder class provides a base implementation for endpoint builders.
///     This abstract class is designed to streamline the creation of endpoint instances
///     by providing core functionalities required by specific endpoint builders.
/// </summary>
/// <typeparam name="TB">The type of the concrete endpoint builder.</typeparam>
public abstract class AbstractEndpointBuilder<TB>
    where TB : IEndpointBuilder<IEndpoint>
{
    protected readonly TB _builder;

    /// <summary>
    ///     Default constructor using the provided builder implementation.
    /// </summary>
    /// <param name="builder">The specific endpoint builder.</param>
    public AbstractEndpointBuilder(TB builder)
    {
        _builder = builder;
    }
}