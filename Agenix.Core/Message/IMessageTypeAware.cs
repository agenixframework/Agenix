namespace Agenix.Core.Message;

/// The MessageTypeAware interface provides a mechanism to set message types.
/// /
public interface IMessageTypeAware
{
    /**
     * Sets the message type.
     */
    void SetMessageType(string messageType);
}