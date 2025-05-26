using Agenix.Api.Context;
using Agenix.Api.Message;

namespace Agenix.Core.Message;

/// <summary>
///     Delegates the building of message payloads to another specified message payload builder.
/// </summary>
/// <param name="builder">The message payload builder to which the building process is delegated.</param>
public class DelegatingMessagePayloadBuilder(MessagePayloadBuilder builder) : IMessagePayloadBuilder
{
    /// <summary>
    ///     Builds the payload for a message using the provided TestContext by delegating to the specified message payload
    ///     builder.
    /// </summary>
    /// <param name="context">The context of the test execution, providing relevant variables, functions, and configurations.</param>
    /// <returns>The constructed payload object as created by the delegated message payload builder.</returns>
    public object BuildPayload(TestContext context)
    {
        return builder(context);
    }
}