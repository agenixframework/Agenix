using System.Collections.ObjectModel;
using Agenix.Api.Exceptions;
using Agenix.Core;

namespace Agenix.Api.Message;

/// <summary>
///     Provides operations to manage and process a collection of message processors.
/// </summary>
public class MessageProcessors
{
    private List<IMessageProcessor> _messageProcessors = [];

    /// <summary>
    ///     Sets the messageProcessors property.
    /// </summary>
    /// <param name="messageProcessors">List of message processors.</param>
    public void SetMessageProcessors(List<IMessageProcessor> messageProcessors)
    {
        _messageProcessors = messageProcessors;
    }

    /// <summary>
    ///     Gets the message processors.
    /// </summary>
    /// <returns>Unmodifiable list of message processors.</returns>
    public IReadOnlyList<IMessageProcessor> GetMessageProcessors()
    {
        return new ReadOnlyCollection<IMessageProcessor>(_messageProcessors);
    }

    /// <summary>
    ///     Adds a new message processor.
    /// </summary>
    /// <param name="processor">Message processor to add.</param>
    public void AddMessageProcessor(IMessageProcessor processor)
    {
        if (processor is IScoped scopedProcessor && !scopedProcessor.IsGlobalScope())
            throw new AgenixSystemException("Unable to add non-global scoped processor to global message processors - " +
                                          "either declare processor as global scope or explicitly add it to test actions instead");
        _messageProcessors.Add(processor);
    }
}