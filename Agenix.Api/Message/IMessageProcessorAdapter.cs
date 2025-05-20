namespace Agenix.Api.Message;

// Delegate declaration
public delegate IMessageProcessor MessageProcessorAdapter();

/// <summary>
///     Adapter interface marks that a class is able to act as a message processor.
/// </summary>
public interface IMessageProcessorAdapter
{
    /// <summary>
    ///     Adapt as a message processor
    /// </summary>
    /// <returns></returns>
    IMessageProcessor AsProcessor();
}