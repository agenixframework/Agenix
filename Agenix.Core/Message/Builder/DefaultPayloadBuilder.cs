namespace Agenix.Core.Message.Builder;

/// <summary>
///     Represents a builder class that constructs message payloads with dynamic content using the provided context.
/// </summary>
public class DefaultPayloadBuilder(object payload) : IMessagePayloadBuilder
{
    private readonly object _payload = payload;

    /// <summary>
    ///     Builds the payload based on the provided context, replacing any dynamic content in a string payload.
    /// </summary>
    /// <param name="context">The context that provides methods for replacing dynamic content in a string.</param>
    /// <returns>The processed payload object.</returns>
    public object BuildPayload(TestContext context)
    {
        return payload switch
        {
            null => "",
            string payloadString => context.ReplaceDynamicContentInString(payloadString),
            _ => payload
        };
    }

    /// <summary>
    ///     Retrieves the payload object that is being built.
    /// </summary>
    /// <returns>The payload object.</returns>
    public object GetPayload()
    {
        return _payload;
    }
}