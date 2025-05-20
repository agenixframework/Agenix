using Agenix.Core.Validation;
using Agenix.Core.Variable;

namespace Agenix.Core.Dsl;

/// Static entrance for all message-related DSL functionalities.
/// <return>A new instance of MessageSupport</return>
public class MessageSupport
{
    /// Static entrance for all message-related Java DSL functionalities.
    /// <return>A new instance of MessageSupport</return>
    public static MessageSupport Message()
    {
        return new MessageSupport();
    }

    /// Static entrance for all message-header-related DSL functionalities.
    /// <return>A new instance of MessageHeaderVariableExtractor.Builder</return>
    public MessageHeaderVariableExtractor.Builder Headers()
    {
        return MessageHeaderSupport.FromHeaders();
    }

    /// Entry point for extracting a payload variable from the message body.
    /// @return A new instance of DelegatingPayloadVariableExtractor.Builder.
    public DelegatingPayloadVariableExtractor.Builder Body()
    {
        return MessageBodySupport.FromBody();
    }

    /// Static entrance for all message-header-related C# DSL functionalities.
    /// Returns a new instance of MessageHeaderVariableExtractor.Builder.
    /// /
    public static class MessageHeaderSupport
    {
        /// Static entrance for all message-header-related Java DSL functionalities.
        /// <return>A new instance of MessageHeaderVariableExtractor.Builder</return>
        public static MessageHeaderVariableExtractor.Builder FromHeaders()
        {
            return MessageHeaderVariableExtractor.Builder.FromHeaders();
        }
    }

    /// Provides a C# Domain Specific Language (DSL) helper for message bodies.
    /// This class offers static methods to facilitate operations related to message bodies in the DSL.
    /// /
    public static class MessageBodySupport
    {
        /// Static entrance for all messaging body-related DSL functionalities.
        /// <return> A new instance of DelegatingPayloadVariableExtractor.Builder.
        public static DelegatingPayloadVariableExtractor.Builder FromBody()
        {
            return DelegatingPayloadVariableExtractor.Builder.FromBody();
        }
    }
}