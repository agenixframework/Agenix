namespace Agenix.Core.Message;

public interface IMessageTypeSelector
{
    /// <summary>
    ///     Checks if this message processor is capable of handling the given message type.
    /// </summary>
    /// <param name="messageType">The message type representation as string (e.g., xml, json, csv, plaintext).</param>
    /// <returns>true if this component supports the message type.</returns>
    bool SupportsMessageType(string messageType);
}