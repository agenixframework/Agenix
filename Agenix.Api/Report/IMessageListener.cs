using Agenix.Api.Context;
using Agenix.Api.Message;

namespace Agenix.Api.Report;

/// Provides mechanisms to listen to inbound and outbound message events,
/// enabling actions to be taken on raw message data at different stages of
/// message processing.
/// /
public interface IMessageListener
{
    /// Invoked on inbound message event. Raw message data is passed to this listener
    /// in a very early state of message processing. 
    /// @param message The inbound message being processed.
    /// @param context The context in which the message is processed, providing environment and additional metadata.
    /// /
    void OnInboundMessage(IMessage message, TestContext context);

    /// Invoked on outbound message event. Raw message data is passed to this listener
    /// in a very late state of message processing. 
    /// @param message
    /// @param context
    /// /
    void OnOutboundMessage(IMessage message, TestContext context);
}