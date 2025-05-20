using Agenix.Api.Common;
using Agenix.Api.Messaging;
using Agenix.Core.Endpoint;

namespace Agenix.Api.Endpoint;

/// <summary>
/// Represents an endpoint for messaging processes that includes configurations,
/// and provides capabilities to produce and consume messages.
/// </summary>
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