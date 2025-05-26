using Agenix.Api.Context;
using Agenix.Api.Message;

namespace Agenix.Core.Message;

/// <summary>
///     Processes messages by delegating the processing work to another specified message processor.
/// </summary>
/// <param name="messageProcessor">The message processor to which the processing work is delegated.</param>
public class DelegatingMessageProcessor(MessageProcessor messageProcessor) : IMessageProcessor
{
    /// <summary>
    ///     Processes a message by delegating the processing work to another specified message processor.
    /// </summary>
    /// <param name="message">The message object to be processed.</param>
    /// <param name="context">The test context in which the message is processed.</param>
    public void Process(IMessage message, TestContext context)
    {
        messageProcessor(message, context);
    }
}