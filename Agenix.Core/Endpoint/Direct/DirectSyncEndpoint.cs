using Agenix.Core.Messaging;

namespace Agenix.Core.Endpoint.Direct;

/// <summary>
///     Direct message endpoint implementation sends and receives message from in memory message queue.
/// </summary>
public class DirectSyncEndpoint : DirectEndpoint
{
    /**
     * Cached producer or consumer
     */
    private DirectSyncConsumer _syncConsumer;

    private DirectSyncProducer _syncProducer;

    /// A class representing a direct message endpoint.
    /// This class is responsible for creating producers and consumers for the direct endpoint.
    /// /
    public DirectSyncEndpoint() : base(new DirectSyncEndpointConfiguration())
    {
    }

    /// <summary>
    ///     Direct message endpoint implementation sends and receives message from in memory message queue.
    /// </summary>
    public DirectSyncEndpoint(DirectSyncEndpointConfiguration configuration) : base(configuration)
    {
    }

    /// <summary>
    ///     Provides the specific configuration settings used by the DirectSyncEndpoint.
    /// </summary>
    public override DirectSyncEndpointConfiguration EndpointConfiguration =>
        (DirectSyncEndpointConfiguration)base.EndpointConfiguration;

    /// <summary>
    ///     Creates a new producer instance for the direct endpoint.
    /// </summary>
    /// <returns>Returns an instance of <see cref="IProducer" />.</returns>
    public override IProducer CreateProducer()
    {
        return _syncProducer ??= new DirectSyncProducer(ProducerName, EndpointConfiguration);
    }

    /// Creates a consumer for the direct message endpoint.
    /// This method instantiates and returns a new DirectConsumer if one does not already exist.
    /// <returns>An IConsumer instance representing the message consumer.</returns>
    public override ISelectiveConsumer CreateConsumer()
    {
        return _syncConsumer ??= new DirectSyncConsumer(ConsumerName, EndpointConfiguration);
    }
}