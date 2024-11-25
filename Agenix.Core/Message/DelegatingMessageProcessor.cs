namespace Agenix.Core.Message;

/// <summary>
///     Processes messages by delegating the processing work to another specified message processor.
/// </summary>
/// <param name="messageProcessor">The message processor to which the processing work is delegated.</param>
public class DelegatingMessageProcessor(MessageProcessor messageProcessor) : IMessageProcessor
{
    public void Process(IMessage message, TestContext context)
    {
        messageProcessor(message, context);
    }
}