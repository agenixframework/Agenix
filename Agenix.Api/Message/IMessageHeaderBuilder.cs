using Agenix.Api.Context;

namespace Agenix.Api.Message;

/// Defines a contract for building headers for messages.
/// /
public interface IMessageHeaderBuilder
{
    /// Builds headers for a message.
    /// @param context The current test context used for building headers.
    /// @return A dictionary of message headers with corresponding key-value pairs.
    /// /
    Dictionary<string, object> BuilderHeaders(TestContext context);
}