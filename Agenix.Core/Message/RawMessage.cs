namespace Agenix.Core.Message
{
    /// <summary>
    /// Represents a raw message entity that extends the DefaultMessage class, providing constructors
    /// for initializing with or without a message payload.
    /// </summary>
    public class RawMessage : DefaultMessage
    {
        /// Represents a raw message entity that extends the DefaultMessage class,
        /// providing constructors for initializing with or without a message payload.
        /// /
        public RawMessage() : base()
        {
        }

        /// Represents a raw message entity that provides constructors for initializing with or without a message payload.
        /// /
        public RawMessage(string messageData) : base(messageData)
        {
        }

        public string Print()
        {
            return GetPayload<string>();
        }
    }
}