namespace Agenix.Core.Message;

/// <summary>
///     Defines the contract for processing messages within the given context.
/// </summary>
public interface IMessageProcessor : IMessageTransformer
{
    /// <summary>
    ///     Processes the given message payload within the specified test context.
    /// </summary>
    /// <param name="message">The message to process.</param>
    /// <param name="context">The context in which the message is being processed.</param>
    void Process(IMessage message, TestContext context);

    /// <summary>
    ///     Transforms the given message within the specified test context.
    /// </summary>
    /// <param name="message">The message to transform.</param>
    /// <param name="context">The context in which the message is being transformed.</param>
    /// <returns>The transformed message.</returns>
    new IMessage Transform(IMessage message, TestContext context)
    {
        Process(message, context);
        return message;
    }

    /// <summary>
    ///     Interface IBuilder defines the contract for building instances of
    ///     IMessageProcessor implementations.
    /// </summary>
    interface IBuilder<out T, TB> where T : IMessageProcessor where TB : IBuilder<T, TB>
    {
        /// <summary>
        ///     Builds new message processor instance.
        /// </summary>
        /// <returns></returns>
        T Build();
    }
}