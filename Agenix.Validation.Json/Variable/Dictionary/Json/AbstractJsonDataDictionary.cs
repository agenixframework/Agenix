using Agenix.Api.Message;
using Agenix.Core.Variable.Dictionary;

namespace Agenix.Validation.Json.Variable.Dictionary.Json;

/// <summary>
/// Abstract json data dictionary works on json message data. Each value is translated with a dictionary.
/// </summary>
public abstract class AbstractJsonDataDictionary : AbstractDataDictionary<string>
{
    /// <summary>
    /// Checks if this message interceptor is capable of this message type. XML message interceptors may only apply to this message
    /// type while JSON message interceptor implementations do not and vice versa.
    /// </summary>
    /// <param name="messageType">The message type representation as string (e.g. XML, JSON, CSV, plaintext).</param>
    /// <returns>True if this message interceptor supports the message type.</returns>
    public override bool SupportsMessageType(string messageType)
    {
        return nameof(MessageType.JSON).Equals(messageType, StringComparison.OrdinalIgnoreCase);
    }
}
