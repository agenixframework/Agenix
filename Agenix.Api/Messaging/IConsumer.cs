using Agenix.Api.Context;
using Agenix.Api.Message;

namespace Agenix.Api.Messaging;

/// <summary>
///     Represents a message consumer capable of receiving messages.
/// </summary>
public interface IConsumer
{
    /// <summary>
    ///     Gets the consumer name.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Receive message with default timeout.
    /// </summary>
    /// <param name="context">the internal context</param>
    /// <returns>the internal message</returns>
    IMessage Receive(TestContext context);

    /// <summary>
    ///     Receive message with a given timeout.
    /// </summary>
    /// <param name="context">the internal context</param>
    /// <param name="timeout">the timeout in milliseconds</param>
    /// <returns>the internal message</returns>
    IMessage Receive(TestContext context, long timeout);
}