namespace Agenix.Api.Message;

/// <summary>
///     Specific message headers.
/// </summary>
public class MessageHeaders
{
    /// <summary>
    ///     Common header name prefix
    /// </summary>
    public static readonly string Prefix = "agenix_";

    /// <summary>
    ///     Message-related header prefix
    /// </summary>
    public static readonly string MessagePrefix = Prefix + "message_";

    /// <summary>
    ///     Unique message id
    /// </summary>
    public static readonly string Id = MessagePrefix + "id";

    /// <summary>
    ///     Time message was created
    /// </summary>
    public static readonly string Timestamp = MessagePrefix + "timestamp";

    /// <summary>
    ///     Header indicating the message type (e.g., XML, JSON, csv, plaintext, etc.)
    /// </summary>
    public static readonly string MessageType = MessagePrefix + "type";

    /// <summary>
    ///     Synchronous message correlation
    /// </summary>
    public static readonly string MessageCorrelationKey = MessagePrefix + "correlator";

    /// <summary>
    ///     Synchronous reply to message destination name
    /// </summary>
    public static readonly string MessageReplyTo = MessagePrefix + "replyTo";

    /// <summary>
    ///     Prevent instantiation.
    /// </summary>
    private MessageHeaders()
    {
    }
}