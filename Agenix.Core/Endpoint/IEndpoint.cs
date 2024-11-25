using Agenix.Core.Common;
using Agenix.Core.Messaging;

namespace Agenix.Core.Endpoint;

public interface IEndpoint : INamed
{
    /// <summary>
    ///     Gets the endpoint configuration holding all endpoint specific properties such as endpoint uri, connection timeout,
    ///     ports, etc.
    /// </summary>
    IEndpointConfiguration EndpointConfiguration { get; }

    /// <summary>
    ///     Gets/ Sets the endpoint name
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Creates a message producer for this endpoint for sending messages to this endpoint.
    /// </summary>
    /// <returns></returns>
    IProducer CreateProducer();

    /// <summary>
    ///     Creates a message consumer for this endpoint. Consumer receives messages on this endpoint.
    /// </summary>
    /// <returns></returns>
    IConsumer CreateConsumer();
}