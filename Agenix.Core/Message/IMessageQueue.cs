namespace Agenix.Core.Message;

/// Send new message to queue.
/// @param message the message to be sent to the queue.
public interface IMessageQueue
{
    /// Send new message to queue.
    /// @param message the message to be sent to the queue.
    /// /
    void Send(IMessage message);

    /// Receive any message on the queue. If no message is present return null.
    /// @return the first message on the queue or null if no message available.
    /// /
    IMessage Receive()
    {
        return Receive(_ => true);
    }

    /// Receive any message on the queue. If no message is present, return null.
    /// @return the first message on the queue or null if no message is available.
    /// /
    IMessage Receive(long timeout)
    {
        return Receive(_ => true, timeout);
    }

    /// Receive any message on the queue. If no message is present, return null.
    /// @return the first message on the queue or null if no message is available.
    /// /
    IMessage Receive(MessageSelector selector);

    /// Receive any message on the queue. If no message is present return null.
    /// @return the first message on the queue or null if no message available.
    /// /
    IMessage Receive(MessageSelector selector, long timeout);

    /// Purge messages selected by given selector.
    /// @param selector the criteria to select the messages to be purged.
    /// /
    void Purge(IMessageSelector selector);
}