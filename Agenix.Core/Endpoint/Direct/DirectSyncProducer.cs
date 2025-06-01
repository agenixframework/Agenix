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
using Agenix.Api.Message.Correlation;
using Agenix.Api.Messaging;
using Agenix.Core.Message;
using Agenix.Core.Message.Correlation;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Endpoint.Direct;

/// <summary>
///     The DirectSyncProducer class is responsible for sending messages synchronously to a direct endpoint
///     and handling the related reply messages. This class uses a correlation manager to keep track of
///     message correlations and manage replies.
/// </summary>
public class DirectSyncProducer : DirectProducer, IReplyConsumer
{
    /// <summary>
    ///     Log is a static readonly field that provides logging functionality
    ///     using the log4net library. It is used to record and manage log
    ///     messages for the DirectSyncConsumer class, aiding in debugging,
    ///     monitoring, and maintenance of the application.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(DirectSyncProducer));

    /**
     * Endpoint configuration
     */
    private readonly DirectSyncEndpointConfiguration _endpointConfiguration;

    /**
     * Reply to channel store
     */
    private ICorrelationManager<IMessage> _correlationManager;

    public DirectSyncProducer(string name, DirectSyncEndpointConfiguration endpointConfiguration) : base(name,
        endpointConfiguration)
    {
        _endpointConfiguration = endpointConfiguration;

        _correlationManager =
            new PollingCorrelationManager<IMessage>(endpointConfiguration, "Reply message did not arrive yet");
    }

    /// Gets the correlation manager.
    /// @return
    /// /
    public ICorrelationManager<IMessage> CorrelationManager
    {
        get => _correlationManager;
        set => _correlationManager = value;
    }

    /// <summary>
    ///     Receives a message based on the given TestContext.
    /// </summary>
    /// <param name="context">The context in which the message will be received.</param>
    /// <returns>The received message.</returns>
    public IMessage Receive(TestContext context)
    {
        return Receive(_correlationManager.GetCorrelationKey(
            _endpointConfiguration.Correlator.GetCorrelationKeyName(Name), context), context);
    }

    /// <summary>
    ///     Receives a message based on the given context and timeout.
    /// </summary>
    /// <param name="context">The context in which the message will be received.</param>
    /// <param name="timeout">The timeout duration for receiving the message.</param>
    /// <returns>The received message.</returns>
    public IMessage Receive(TestContext context, long timeout)
    {
        return Receive(_correlationManager.GetCorrelationKey(
            _endpointConfiguration.Correlator.GetCorrelationKeyName(Name), context), context, timeout);
    }

    /// <summary>
    ///     Receives a message based on the given context.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="context">The context in which the message will be received.</param>
    /// <returns>The received message.</returns>
    public IMessage Receive(string selector, TestContext context)
    {
        return Receive(selector, context, _endpointConfiguration.Timeout);
    }

    /// <summary>
    ///     Receives a message based on the given context.
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="context">The context in which the message will be received.</param>
    /// <returns>The received message.</returns>
    public IMessage Receive(string selector, TestContext context, long timeout)
    {
        var message = _correlationManager.Find(selector, timeout);

        if (message == null)
        {
            throw new MessageTimeoutException(timeout, GetDestinationQueueName());
        }

        return message;
    }

    /// <summary>
    ///     Sends a message to the designated destination queue and processes the reply synchronously.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    /// <param name="context">The operational context in which the message is sent.</param>
    public override void Send(IMessage message, TestContext context)
    {
        var correlationKeyName = _endpointConfiguration.Correlator.GetCorrelationKeyName(Name);
        var correlationKey = _endpointConfiguration.Correlator.GetCorrelationKey(message);
        _correlationManager.SaveCorrelationKey(correlationKeyName, correlationKey, context);

        var destinationQueueName = GetDestinationQueueName();

        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug($"Sending message to queue: '{destinationQueueName}'");
            Log.LogDebug($"Message to send is:\n{message}");
        }

        Log.LogInformation($"Message was sent to queue: '{destinationQueueName}'");

        var replyQueue = GetReplyQueue(message, context);
        GetDestinationQueue(context).Send(message);
        var replyMessage = replyQueue.Receive(_endpointConfiguration.Timeout);

        if (replyMessage == null)
        {
            throw new ReplyMessageTimeoutException(_endpointConfiguration.Timeout, destinationQueueName);
        }

        Log.LogInformation("Received synchronous response from reply queue");

        _correlationManager.Store(correlationKey, replyMessage);
    }

    /// <summary>
    ///     Reads reply queue from the message header or creates a new temporary queue.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public IMessageQueue GetReplyQueue(IMessage message, TestContext context)
    {
        if (message.GetHeader(DirectMessageHeaders.ReplyQueue) == null)
        {
            IMessageQueue temporaryQueue = new DefaultMessageQueue(Name + "." + Guid.NewGuid());
            message.SetHeader(DirectMessageHeaders.ReplyQueue, temporaryQueue);
            return temporaryQueue;
        }

        if (message.GetHeader(DirectMessageHeaders.ReplyQueue) is IMessageQueue queue)
        {
            return queue;
        }

        return ResolveQueueName(message.GetHeader(DirectMessageHeaders.ReplyQueue).ToString(), context);
    }
}
