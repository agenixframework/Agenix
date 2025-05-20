using Agenix.Api.Context;
using Agenix.Api.Message;

namespace Agenix.Api.Messaging;

/// <summary>
///     Interface representing a selective message consumer capable of receiving messages based on specified selectors.
/// </summary>
public interface ISelectiveConsumer : IConsumer
{
    /// Receive a message with a message selector and default timeout.
    /// @param selector Specifies the criteria for selecting messages.
    /// @param context The context within which the message is to be received.
    /// @return The received message.
    /// /
    IMessage Receive(string selector, TestContext context);

    /// Receive a message with a message selector and default timeout.
    /// <param name="selector">Specifies the criteria for selecting messages.</param>
    /// <param name="context">The context within which the message is to be received.</param>
    /// <return>The received message.</return>
    IMessage Receive(string selector, TestContext context, long timeout);
}