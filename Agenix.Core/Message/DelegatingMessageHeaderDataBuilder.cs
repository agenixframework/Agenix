using System.Collections.Generic;

namespace Agenix.Core.Message;

/// <summary>
///     Delegates the building of message payloads to another specified message payload builder.
/// </summary>
/// <param name="builder">The message payload builder to which the building process is delegated.</param>
public class DelegatingMessageHeaderDataBuilder(MessageHeaderDataBuilder builder) : IMessageHeaderDataBuilder
{
    public string BuildHeaderData(TestContext context)
    {
        return builder(context);
    }

    public Dictionary<string, object> BuilderHeaders(TestContext context)
    {
        return new Dictionary<string, object>();
    }
}