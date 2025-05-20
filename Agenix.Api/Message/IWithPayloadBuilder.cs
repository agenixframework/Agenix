namespace Agenix.Api.Message;

/// <summary>
///     Provides methods to set and get a message payload builder.
/// </summary>
public interface IWithPayloadBuilder
{
    /// <summary>
    /// Sets the payload builder for the message.
    /// </summary>
    /// <param name="builder">The message payload builder to be set.</param>
    void SetPayloadBuilder(IMessagePayloadBuilder builder);
    
    /// <summary>
    /// Gets the message payload builder.
    /// </summary>
    /// <returns>The current message payload builder.</returns>
    IMessagePayloadBuilder GetPayloadBuilder();
}