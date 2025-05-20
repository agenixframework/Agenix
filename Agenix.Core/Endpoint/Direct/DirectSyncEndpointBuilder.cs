using Agenix.Api.Message;
using Agenix.Core.Message;

namespace Agenix.Core.Endpoint.Direct;

/// <summary>
///     Provides functionality for building <see cref="DirectSyncEndpoint" /> instances.
/// </summary>
public class DirectSyncEndpointBuilder : AbstractEndpointBuilder<DirectSyncEndpoint>
{
    /// <summary>
    ///     Endpoint target
    /// </summary>
    private readonly DirectSyncEndpoint _endpoint = new();

    /// <summary>
    ///     Gets the instance of <see cref="DirectSyncEndpoint" /> being built.
    /// </summary>
    /// <returns>
    ///     The <see cref="DirectSyncEndpoint" /> instance.
    /// </returns>
    protected override DirectSyncEndpoint GetEndpoint()
    {
        return _endpoint;
    }

    /// <summary>
    ///     Sets the queue name for the endpoint.
    /// </summary>
    /// <param name="queueName">The name of the queue to be set for the endpoint.</param>
    /// <returns>The current instance of <see cref="DirectSyncEndpointBuilder" />.</returns>
    public DirectSyncEndpointBuilder Queue(string queueName)
    {
        _endpoint.EndpointConfiguration.SetQueueName(queueName);
        return this;
    }

    /// <summary>
    ///     Sets the given queue for the endpoint.
    /// </summary>
    /// <param name="queue">The queue to be set for the endpoint.</param>
    /// <returns>The current instance of <see cref="DirectSyncEndpointBuilder" />.</returns>
    public DirectSyncEndpointBuilder Queue(IMessageQueue queue)
    {
        _endpoint.EndpointConfiguration.SetQueue(queue);
        return this;
    }

    /// <summary>
    ///     Sets the polling interval for the endpoint.
    /// </summary>
    /// <param name="pollingInterval">The polling interval, in milliseconds.</param>
    /// <returns>The current <see cref="DirectSyncEndpointBuilder" /> instance.</returns>
    public DirectSyncEndpointBuilder PollingInterval(int pollingInterval)
    {
        _endpoint.EndpointConfiguration.PollingInterval = pollingInterval;
        return this;
    }

    /// <summary>
    ///     Sets the message correlator.
    /// </summary>
    /// <param name="correlator">The message correlator to set.</param>
    /// <returns>The current <see cref="DirectSyncEndpointBuilder" /> instance.</returns>
    public DirectSyncEndpointBuilder Correlator(IMessageCorrelator correlator)
    {
        _endpoint.EndpointConfiguration.Correlator = correlator;
        return this;
    }

    /// <summary>
    ///     Sets the default timeout.
    /// </summary>
    /// <param name="timeout">The timeout value in milliseconds.</param>
    /// <returns>The current instance of <see cref="DirectSyncEndpointBuilder" />.</returns>
    public DirectSyncEndpointBuilder Timeout(long timeout)
    {
        _endpoint.EndpointConfiguration.Timeout = timeout;
        return this;
    }
}