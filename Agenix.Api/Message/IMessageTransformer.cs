using Agenix.Api.Context;

namespace Agenix.Api.Message;

/// Interface for transforming a message within a specified test context.
public interface IMessageTransformer
{
    /// Transform a message with a given test context and return a new message.
    /// @param message the message to process.
    /// @param context the current test context.
    /// @return the processed message.
    /// /
    extern IMessage Transform(IMessage message, TestContext context);

    /// Defines a generic builder interface for constructing instances of IMessageTransformer.
    /// @type param T The type of IMessageTransformer that this builder will create.
    /// @type param TB The type of the builder itself, to allow for fluent builder patterns.
    /// /
    interface IBuilder<out T, TB> where T : IMessageTransformer where TB : IBuilder<T, TB>
    {
        /// Builds a new message processor instance.
        /// @return new instance of message processor.
        T Build();
    }
}