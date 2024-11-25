using Agenix.Core.Exceptions;
using Agenix.Core.Message;
using Agenix.Core.Message.Selector;
using Agenix.Core.Messaging;
using log4net;

namespace Agenix.Core.Endpoint.Direct;

/// <summary>
///     The DirectConsumer class is responsible for receiving messages from a designated queue
///     based on the provided selector and context within a specified timeout period.
/// </summary>
public class DirectConsumer(string name, DirectEndpointConfiguration endpointConfiguration)
    : AbstractSelectiveMessageConsumer(name, endpointConfiguration)
{
    /// <summary>
    ///     Represents a log instance used within the DirectConsumer class for logging purposes.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(DirectConsumer));

    /// Receives a message from the designated queue based on the provided selector and context within the specified timeout period.
    /// <param name="selector">The message selector used to filter messages.</param>
    /// <param name="context">The current test context.</param>
    /// <param name="timeout">The timeout period in milliseconds to wait for a message.</param>
    /// <returns>The received message.</returns>
    /// <exception cref="MessageTimeoutException">Thrown when a message is not received within the specified timeout period.</exception>
    public override IMessage Receive(string selector, TestContext context, long timeout)
    {
        var destinationQueue = GetDestinationQueue(context);

        var destinationQueueName = !string.IsNullOrWhiteSpace(selector)
            ? $"{GetDestinationQueueName()}({selector})"
            : GetDestinationQueueName();

        if (Log.IsDebugEnabled) Log.Debug($"Receiving message from queue: '{destinationQueueName}'");

        IMessage message;
        if (!string.IsNullOrWhiteSpace(selector))
        {
            var delegatingMessageSelector = new DelegatingMessageSelector(selector, context);
            MessageSelector messageSelector = msg => delegatingMessageSelector.Accept(msg);

            message = timeout <= 0
                ? destinationQueue.Receive(messageSelector)
                : destinationQueue.Receive(messageSelector, timeout);
        }
        else
        {
            message = timeout <= 0 ? destinationQueue.Receive() : destinationQueue.Receive(timeout);
        }

        if (message == null) throw new MessageTimeoutException(timeout, destinationQueueName);

        Log.Info($"Received message from queue: '{destinationQueueName}'");
        return message;
    }

    /// Retrieves the destination queue based on the endpoint configuration.
    /// <param name="context">The current test context.</param>
    /// <returns>The destination queue.</returns>
    /// <exception cref="CoreSystemException">
    ///     Thrown when neither the queue nor the queue name is set in the endpoint
    ///     configuration.
    /// </exception>
    protected IMessageQueue GetDestinationQueue(TestContext context)
    {
        var queue = endpointConfiguration.GetQueue();

        if (queue != null) return queue;

        var queueName = endpointConfiguration.GetQueueName();

        if (!string.IsNullOrWhiteSpace(queueName)) return ResolveQueueName(queueName, context);

        throw new CoreSystemException("Neither queue name nor queue object is set - please specify destination queue");
    }

    /// Retrieves the destination queue name based on the endpoint configuration.
    /// <returns>The name of the destination queue.</returns>
    /// <exception cref="CoreSystemException">
    ///     Thrown when both the queue and the queue name are not set in the endpoint
    ///     configuration.
    /// </exception>
    protected string GetDestinationQueueName()
    {
        var queue = endpointConfiguration.GetQueue();
        if (queue != null) return queue.ToString();

        var queueName = endpointConfiguration.GetQueueName();
        if (!string.IsNullOrWhiteSpace(queueName)) return queueName;

        throw new CoreSystemException("Neither queue name nor queue object is set - please specify destination queue");
    }

    /// Resolves a queue name into an IMessageQueue object using the provided context.
    /// <param name="queueName">The name of the queue to resolve.</param>
    /// <param name="context">The context containing the reference resolver.</param>
    /// <returns>The resolved IMessageQueue object.</returns>
    /// <exception cref="CoreSystemException">Thrown when the reference resolver is missing in the context.</exception>
    protected IMessageQueue ResolveQueueName(string queueName, TestContext context)
    {
        if (context.GetReferenceResolver() != null)
            return context.GetReferenceResolver().Resolve<IMessageQueue>(queueName);

        throw new CoreSystemException("Unable to resolve message queue - missing proper reference resolver in context");
    }
}