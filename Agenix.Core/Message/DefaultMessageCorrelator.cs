namespace Agenix.Core.Message;

/// <summary>
///     Default message correlator implementation using the Citrus message id as correlation key.
/// </summary>
public class DefaultMessageCorrelator : IMessageCorrelator
{
    /// <summary>
    ///     Generates a correlation key for the given request message.
    /// </summary>
    /// <param name="request">The message for which to generate the correlation key.</param>
    /// <returns>The correlation key constructed from the message id.</returns>
    public string GetCorrelationKey(IMessage request)
    {
        return MessageHeaders.Id + " = '" + request.Id + "'";
    }

    /// <summary>
    ///     Constructs the correlation key from the given identifier.
    /// </summary>
    /// <param name="id">The identifier from which to generate the correlation key.</param>
    /// <returns>The correlation key constructed from the identifier.</returns>
    public string GetCorrelationKey(string id)
    {
        return MessageHeaders.Id + " = '" + id + "'";
    }

    /// <summary>
    ///     Generates a correlation key name for the given consumer name.
    /// </summary>
    /// <param name="consumerName">The name of the consumer for which to generate the correlation key name.</param>
    /// <returns>The correlation key name constructed from the consumer name.</returns>
    public string GetCorrelationKeyName(string consumerName)
    {
        return MessageHeaders.MessageCorrelationKey + "_" + consumerName;
    }
}