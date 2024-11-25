using Agenix.Core.Endpoint;
using Agenix.Core.Message;

namespace Agenix.Core.Messaging;

/// <summary>
///     Abstract base class for message consumers, providing common functionality
///     for handling messages within an endpoint configuration context.
/// </summary>
public abstract class AbstractMessageConsumer : IConsumer
{
    private readonly IEndpointConfiguration _endpointConfiguration;

    /// <summary>
    ///     Default constructor using receive timeout setting.
    /// </summary>
    /// <param name="name">The name of the consumer</param>
    /// <param name="endpointConfiguration">Endpoint configuration</param>
    public AbstractMessageConsumer(string name, IEndpointConfiguration endpointConfiguration)
    {
        Name = name;
        _endpointConfiguration = endpointConfiguration;
    }

    public string Name { get; }

    /// <summary>
    ///     Synchronously receives a message with a context provided by TestContext.
    /// </summary>
    /// <param name="context">The context containing the state and environment for the message reception.</param>
    /// <returns>The received message as an instance of IMessage.</returns>
    public IMessage Receive(TestContext context)
    {
        return Receive(context, _endpointConfiguration.Timeout);
    }

    // Abstract method to be implemented by subclasses
    public abstract IMessage Receive(TestContext context, long timeout);
}