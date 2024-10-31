namespace Agenix.Core.Message;

/// <summary>
///     Provides methods to set and get a message payload builder.
/// </summary>
public interface IWithPayloadBuilder
{
    /*
     * Sets message payload builder.
     * @param builder
     */
    void SetPayloadBuilder(IMessagePayloadBuilder builder);

    /*
     * Gets the message payload builder.
     * @return
     */
    IMessagePayloadBuilder GetPayloadBuilder();
}