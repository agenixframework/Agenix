#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Endpoint.Direct;

/// The DirectProducer class is responsible for sending messages to a destination queue
/// defined in the endpoint configuration.
public class DirectProducer(string name, DirectEndpointConfiguration endpointConfiguration) : IProducer
{
    /// <summary>
    ///     Represents a log instance used within the DirectProducer class for logging purposes.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(DirectProducer));

    public string Name => name;

    /// Sends a message to the destination queue defined in the endpoint configuration.
    /// <param name="message">The message to be sent.</param>
    /// <param name="context">The current test context.</param>
    /// <exception cref="AgenixSystemException">Thrown when the message fails to send to the destination queue.</exception>
    public virtual void Send(IMessage message, TestContext context)
    {
        var destinationQueueName = GetDestinationQueueName();

        Log.LogDebug($"Sending message to queue: '{destinationQueueName}'");
        Log.LogDebug($"Message to send is:\n{message.Print(context)}");

        try
        {
            GetDestinationQueue(context).Send(message);
        }
        catch (Exception e)
        {
            throw new AgenixSystemException($"Failed to send message to queue: '{destinationQueueName}'", e);
        }

        Log.LogInformation($"Message was sent to queue: '{destinationQueueName}'");
    }

    /// Retrieves the destination queue based on the endpoint configuration.
    /// <param name="context">The current test context.</param>
    /// <returns>The destination queue.</returns>
    /// <exception cref="AgenixSystemException">
    ///     Thrown when neither the queue nor the queue name is set in the endpoint
    ///     configuration.
    /// </exception>
    protected IMessageQueue GetDestinationQueue(TestContext context)
    {
        var queue = endpointConfiguration.GetQueue();

        if (queue != null) return queue;

        var queueName = endpointConfiguration.GetQueueName();

        if (!string.IsNullOrWhiteSpace(queueName)) return ResolveQueueName(queueName, context);

        throw new AgenixSystemException(
            "Neither queue name nor queue object is set - please specify destination queue");
    }

    /// Retrieves the destination queue name based on the endpoint configuration.
    /// <returns>The name of the destination queue.</returns>
    /// <exception cref="AgenixSystemException">
    ///     Thrown when both the queue and the queue name are not set in the endpoint
    ///     configuration.
    /// </exception>
    protected string GetDestinationQueueName()
    {
        var queue = endpointConfiguration.GetQueue();
        if (queue != null) return queue.ToString();

        var queueName = endpointConfiguration.GetQueueName();
        if (!string.IsNullOrWhiteSpace(queueName)) return queueName;

        throw new AgenixSystemException(
            "Neither queue name nor queue object is set - please specify destination queue");
    }


    /// Resolves a queue name into an IMessageQueue object using the provided context.
    /// <param name="queueName">The name of the queue to resolve.</param>
    /// <param name="context">The context containing the reference resolver.</param>
    /// <returns>The resolved IMessageQueue object.</returns>
    /// <exception cref="AgenixSystemException">Thrown when the reference resolver is missing in the context.</exception>
    protected IMessageQueue ResolveQueueName(string queueName, TestContext context)
    {
        if (context.ReferenceResolver != null)
            return context.ReferenceResolver.Resolve<IMessageQueue>(queueName);

        throw new AgenixSystemException(
            "Unable to resolve message queue - missing proper reference resolver in context");
    }
}
