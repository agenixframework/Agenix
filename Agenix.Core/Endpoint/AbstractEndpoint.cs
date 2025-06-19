using Agenix.Api.Endpoint;
using Agenix.Api.Messaging;

namespace Agenix.Core.Endpoint;

/// <summary>
///     Abstract message endpoint handles send/receive timeout setting
/// </summary>
/// <remarks>
///     Default constructor using endpoint configuration.
/// </remarks>
/// <param name="endpointConfiguration">the endpoint configuration</param>
public abstract class AbstractEndpoint(IEndpointConfiguration endpointConfiguration) : IEndpoint
{
    /// <summary>
    ///     Gets the endpoints consumer name.
    /// </summary>
    public string ConsumerName => Name + ":consumer";

    /// <summary>
    ///     Gets the endpoints producer name.
    /// </summary>
    public string ProducerName => Name + ":producer";

    public virtual IEndpointConfiguration EndpointConfiguration { get; } = endpointConfiguration;

    /// <summary>
    ///     Gets/ Sets the endpoints producer name.
    /// </summary>
    public virtual string Name { get; set; } = nameof(AbstractEndpoint);

    public void SetName(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     Creates a message producer for this endpoint. Must be implemented by concrete subclasses.
    /// </summary>
    /// <returns>A message producer.</returns>
    public abstract IProducer CreateProducer();

    /// <summary>
    ///     Creates a message consumer for this endpoint. Must be implemented by concrete subclasses.
    /// </summary>
    /// <returns>A message consumer.</returns>
    public abstract IConsumer CreateConsumer();
}
