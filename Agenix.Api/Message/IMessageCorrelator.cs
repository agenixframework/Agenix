using Agenix.Api.Message;

namespace Agenix.Core.Message;

/// <summary>
///     Message correlator interface for synchronous reply messages. Correlator uses a specific header entry in messages in
///     order to construct a unique message correlation ke
/// </summary>
public interface IMessageCorrelator
{
    /// Constructs the correlation key from the message header.
    /// @param request The message from which to extract the correlation key.
    /// @return The constructed correlation key.
    /// /
    string GetCorrelationKey(IMessage request);

    /// Constructs the correlation key from the message header.
    /// <param name="request">The message from which to extract the correlation key.</param>
    /// <return>The constructed correlation key.</return>
    string GetCorrelationKey(string id);

    /// Constructs unique correlation key name for the given consumer name.
    /// Correlation key must be unique across all message consumers running inside a test case.
    /// Therefore, consumer name is passed as an argument and must be part of the constructed correlation key name.
    /// <param name="consumerName">The name of the consumer for which to construct the correlation key name.</param>
    /// <return>The constructed unique correlation key name for the given consumer.</return>
    string GetCorrelationKeyName(string consumerName);
}