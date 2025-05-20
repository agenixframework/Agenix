namespace Agenix.Api.Message
{
    /// Attribute that specifies the type of payload a message payload builder is expected to produce.
    /// It is used to associate a message builder with an expected enumeration value defining the message payload type.
    /// /
    [AttributeUsage(AttributeTargets.Class)]
    public class MessagePayloadAttribute(MessageType value) : Attribute
    {
        /// Gets the type of message produced by the payload builder.
        /// @return the type of the message as a MessageType.
        /// /
        public MessageType Value { get; } = value;
    }
}