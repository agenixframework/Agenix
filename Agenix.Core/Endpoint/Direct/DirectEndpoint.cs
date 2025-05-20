using Agenix.Api.Messaging;

namespace Agenix.Core.Endpoint.Direct;

/// <summary>
///     Direct message endpoint implementation sends and receives message from in memory message queue.
/// </summary>
public class DirectEndpoint : AbstractEndpoint
{
    /**
     * Cached producer or consumer
     */
    private DirectConsumer _channelConsumer;

    private DirectProducer _channelProducer;

    /// A class representing a direct message endpoint.
    /// This class is responsible for creating producers and consumers for the direct endpoint.
    /// /
    public DirectEndpoint() : base(new DirectEndpointConfiguration())
    {
    }

    /// <summary>
    ///     Direct message endpoint implementation sends and receives message from in memory message queue.
    /// </summary>
    public DirectEndpoint(DirectEndpointConfiguration configuration) : base(configuration)
    {
    }

    /// <summary>
    ///     Provides the specific configuration settings used by the DirectEndpoint.
    /// </summary>
    public override DirectEndpointConfiguration EndpointConfiguration =>
        (DirectEndpointConfiguration)base.EndpointConfiguration;

    /// <summary>
    ///     Creates a new producer instance for the direct endpoint.
    /// </summary>
    /// <returns>Returns an instance of <see cref="IProducer" />.</returns>
    public override IProducer CreateProducer()
    {
        return _channelProducer ??= new DirectProducer(ProducerName, EndpointConfiguration);
    }

    /// Creates a consumer for the direct message endpoint.
    /// This method instantiates and returns a new DirectConsumer if one does not already exist.
    /// <returns>An IConsumer instance representing the message consumer.</returns>
    public override ISelectiveConsumer CreateConsumer()
    {
        return _channelConsumer ??= new DirectConsumer(ConsumerName, EndpointConfiguration);
    }
}