using Agenix.Api.Message;

namespace Agenix.Core.Endpoint.Direct;

/// <summary>
///     Represents headers for direct messages within the Agenix.Core.Endpoint.Direct namespace.
/// </summary>
public class DirectMessageHeaders
{
    /**
     * Message reply queue name header
     */
    public static readonly string ReplyQueue = MessageHeaders.Prefix + "reply_queue";

    /// Provides constants for various message headers specific to direct messaging.
    /// These headers are used to facilitate direct communication between endpoints.
    /// /
    private DirectMessageHeaders()
    {
        // utility class
    }
}