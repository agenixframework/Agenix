using Agenix.Api.Message;

namespace Agenix.Core.Endpoint.Direct;

/// <summary>
///     The DirectEndpointBuilder class is responsible for constructing instances of
///     <see cref="DirectEndpoint" />. This class provides methods to configure various
///     properties of the <see cref="DirectEndpoint" /> such as queue name and timeout.
/// </summary>
public class DirectEndpointBuilder : AbstractEndpointBuilder<DirectEndpoint>
{
    private readonly DirectEndpoint _endpoint = new();

    /// <summary>
    ///     Retrieves the DirectEndpoint instance associated with this builder.
    /// </summary>
    /// <returns>The DirectEndpoint instance that is being constructed by this builder.</returns>
    protected override DirectEndpoint GetEndpoint()
    {
        return _endpoint;
    }

    /// <summary>
    ///     Sets the queue name property.
    /// </summary>
    /// <param name="queueName">The name of the queue to set.</param>
    /// <returns>The current instance of <see cref="DirectEndpointBuilder" /> for method chaining.</returns>
    public DirectEndpointBuilder Queue(string queueName)
    {
        _endpoint.EndpointConfiguration.SetQueueName(queueName);
        return this;
    }

    /// <summary>
    ///     Sets the queue name property.
    /// </summary>
    /// <param name="queueName">The name of the queue to set.</param>
    /// <returns>The current instance of <see cref="DirectEndpointBuilder" /> for method chaining.</returns>
    public DirectEndpointBuilder Queue(IMessageQueue queue)
    {
        _endpoint.EndpointConfiguration.SetQueue(queue);
        return this;
    }

    /// <summary>
    ///     Sets the default timeout for the DirectEndpoint.
    /// </summary>
    /// <param name="timeout">The timeout duration in milliseconds.</param>
    /// <return>The current instance of the DirectEndpointBuilder for method chaining.</return>
    public DirectEndpointBuilder Timeout(long timeout)
    {
        _endpoint.EndpointConfiguration.Timeout = timeout;
        return this;
    }
}