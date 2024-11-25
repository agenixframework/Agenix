namespace Agenix.Core.Message;

/// <summary>
///     Delegates the building of message payloads to another specified message payload builder.
/// </summary>
/// <param name="builder">The message payload builder to which the building process is delegated.</param>
public class DelegatingMessagePayloadBuilder(MessagePayloadBuilder builder) : IMessagePayloadBuilder
{
    public object BuildPayload(TestContext context)
    {
        return builder(context);
    }
}