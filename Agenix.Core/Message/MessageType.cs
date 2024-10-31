using System;
using System.Linq;

namespace Agenix.Core.Message;

/// <summary>
///     Enumeration for message protocol types used in test cases.
/// </summary>
public enum MessageType
{
    UNSPECIFIED,
    XML,
    XHTML,
    CSV,
    JSON,
    PLAINTEXT,
    BINARY,
    BINARY_BASE64,
    GZIP,
    GZIP_BASE64,
    MSCONS
}

/// <summary>
///     Provides extension methods for the MessageType enumeration.
/// </summary>
public class MessageTypeExtensions
{
    /**
     * Check if this message type name is matching a enum value.
     * 
     * @param messageTypeName
     * @return
     */
    public static bool Knows(string messageTypeName)
    {
        return Enum.GetValues(typeof(MessageType)).Cast<object>().Any(value =>
            value.ToString()!.Equals(messageTypeName, StringComparison.OrdinalIgnoreCase));
    }

    /**
     * Checks for the given message type to be handled as binary content.
     * @param messageType
     * @return
     */
    public static bool IsBinary(string messageTypeName)
    {
        return MessageType.GZIP.ToString().Equals(messageTypeName, StringComparison.OrdinalIgnoreCase)
               || MessageType.BINARY.ToString().Equals(messageTypeName, StringComparison.OrdinalIgnoreCase);
    }
}