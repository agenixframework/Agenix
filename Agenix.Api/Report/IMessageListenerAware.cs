namespace Agenix.Api.Report;

/// Interface to be implemented by classes that are aware of and can manage message listeners.
/// /
public interface IMessageListenerAware
{
    /// Adds a new message listener.
    /// @param listener The listener to be added.
    /// /
    void AddMessageListener(IMessageListener listener);
}