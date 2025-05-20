using Agenix.Api.Context;
using Agenix.Api.Message;

namespace Agenix.Api.Messaging;

/// <summary>
///     Provides methods for producing and sending messages.
/// </summary>
public interface IProducer
{
    /// <summary>
    ///     Gets the producer name.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Sends the message.
    /// </summary>
    /// <param name="message">the message object to send.</param>
    /// <param name="context">the internal context.</param>
    void Send(IMessage message, TestContext context);
}