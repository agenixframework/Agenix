using System;
using Agenix.Core.Exceptions;
using Agenix.Core.Message;
using Agenix.Core.Messaging;
using log4net;

namespace Agenix.Core.Endpoint.Direct;

/// The DirectProducer class is responsible for sending messages to a destination queue
/// defined in the endpoint configuration.
public class DirectProducer(string name, DirectEndpointConfiguration endpointConfiguration) : IProducer
{
    /// <summary>
    ///     Represents a log instance used within the DirectProducer class for logging purposes.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(DirectProducer));

    public string Name => name;

    /// Sends a message to the destination queue defined in the endpoint configuration.
    /// <param name="message">The message to be sent.</param>
    /// <param name="context">The current test context.</param>
    /// <exception cref="CoreSystemException">Thrown when the message fails to send to the destination queue.</exception>
    public virtual void Send(IMessage message, TestContext context)
    {
        var destinationQueueName = GetDestinationQueueName();

        Log.Debug($"Sending message to queue: '{destinationQueueName}'");
        Log.Debug($"Message to send is:\n{message.Print(context)}");

        try
        {
            GetDestinationQueue(context).Send(message);
        }
        catch (Exception e)
        {
            throw new CoreSystemException($"Failed to send message to queue: '{destinationQueueName}'", e);
        }

        Log.Info($"Message was sent to queue: '{destinationQueueName}'");
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