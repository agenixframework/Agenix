namespace Agenix.Api.Message;

/// The MessageTypeAware interface provides a mechanism to set message types.
/// /
public interface IMessageTypeAware
{
    /// Sets the type of the message. <param name="messageType">The type of the message being set.</param>
    /// /
    void SetMessageType(string messageType);
}