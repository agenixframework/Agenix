using Agenix.Api.Context;
using Agenix.Api.Message;
using Agenix.Core.Endpoint;

namespace Agenix.Api.Messaging;

/// <summary>
///     An abstract class that represents a selective message consumer capable of receiving messages
///     based on a given selector and context.
/// </summary>
public abstract class AbstractSelectiveMessageConsumer(string name, IEndpointConfiguration endpointConfiguration)
    : AbstractMessageConsumer(name, endpointConfiguration), ISelectiveConsumer
{
    private readonly IEndpointConfiguration _endpointConfiguration1 = endpointConfiguration;

    /// <summary>
    ///     Receives a message from a queue based on the specified selector and context.
    /// </summary>
    /// <param name="selector">The message selector to filter messages.</param>
    /// <param name="context">The context containing information about the test and execution environment.</param>
    /// <returns>An instance of IMessage if a message is received, otherwise null.</returns>
    public IMessage Receive(string selector, TestContext context)
    {
        return Receive(selector, context, _endpointConfiguration1.Timeout);
    }

    /// <summary>
    ///     Receives a message from a queue based on the specified selector and context.
    /// </summary>
    /// <param name="context">The context containing information about the test and execution environment.</param>
    /// <param name="timeout">The maximum time to wait for a message, in milliseconds.</param>
    /// <returns>An instance of <see cref="IMessage" /> if a message is received, otherwise null.</returns>
    public override IMessage Receive(TestContext context, long timeout)
    {
        return Receive(null, context, timeout);
    }

    /// <summary>
    ///     Receives a message from a queue based on the specified selector, context, and timeout.
    /// </summary>
    /// <param name="selector">The message selector to filter messages.</param>
    /// <param name="context">The context containing information about the test and execution environment.</param>
    /// <param name="timeout">The maximum time to wait for a message.</param>
    /// <returns>An instance of IMessage if a message is received, otherwise null.</returns>
    public abstract IMessage Receive(string selector, TestContext context, long timeout);
}