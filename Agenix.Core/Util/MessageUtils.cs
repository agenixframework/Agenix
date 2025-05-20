using Agenix.Api.Message;
using Agenix.Core.Message;

namespace Agenix.Core.Util;

/// <summary>
///     Utility class for message-related operations.
/// </summary>
public static class MessageUtils
{
    /// <summary>
    ///     Checks if given message payload is of type XML. If starts with '<' is considered valid XML.
    /// </summary>
    /// <param name="message">to check.</param>
    /// <returns>true if message payload is XML, false otherwise.</returns>
    public static bool HasXmlPayload(IMessage message)
    {
        if (message.Payload is not string) return false;

        var payload = message.GetPayload<string>().Trim();

        return payload.StartsWith('<');
    }

    /// <summary>
    ///     Checks if message payload is of type Json. An empty payload is considered to be a valid Json payload.
    /// </summary>
    /// <param name="message">to check.</param>
    /// <returns>true if payload is Json, false otherwise.</returns>
    public static bool HasJsonPayload(IMessage message)
    {
        if (message.Payload is not string) return false;

        var payload = message.GetPayload<string>().Trim();

        return payload.Length == 0 || payload.StartsWith('{') || payload.StartsWith('[');
    }
}