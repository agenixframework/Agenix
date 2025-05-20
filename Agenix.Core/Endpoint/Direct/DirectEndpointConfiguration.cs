using Agenix.Api.Message;
using Agenix.Core.Message;

namespace Agenix.Core.Endpoint.Direct;

/// <summary>
///     DirectEndpointConfiguration manages configuration settings specifically
///     for direct endpoints, such as setting and getting the message queue
///     and queue name.
/// </summary>
public class DirectEndpointConfiguration : AbstractEndpointConfiguration
{
    /// <summary>
    ///     Destination queue.
    /// </summary>
    private IMessageQueue _queue;

    /// <summary>
    ///     Destination queue name.
    /// </summary>
    private string _queueName;

    /// <summary>
    ///     Set the message queue.
    /// </summary>
    /// <param name="queue">The queue to set.</param>
    public void SetQueue(IMessageQueue queue)
    {
        _queue = queue;
    }

    /// <summary>
    ///     Sets the destination queue name.
    /// </summary>
    /// <param name="queueName">The queue name to set.</param>
    public void SetQueueName(string queueName)
    {
        _queueName = queueName;
    }

    /// <summary>
    ///     Gets the queue.
    /// </summary>
    /// <returns>The queue.</returns>
    public IMessageQueue GetQueue()
    {
        return _queue;
    }

    /// <summary>
    ///     Gets the queue name.
    /// </summary>
    /// <returns>The queue name.</returns>
    public string GetQueueName()
    {
        return _queueName;
    }
}