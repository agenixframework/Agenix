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

using Agenix.Api.Context;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Message.Correlation;
using Agenix.Api.Messaging;
using Agenix.Core.Message.Correlation;
using Agenix.Core.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Endpoint.Direct;

public class DirectSyncConsumer : DirectConsumer, IReplyProducer
{
    /// <summary>
    ///     Log is a static readonly field that provides logging functionality
    ///     using the log4net library. It is used to record and manage log
    ///     messages for the DirectSyncConsumer class, aiding in debugging,
    ///     monitoring, and maintenance of the application.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(DirectSyncConsumer));

    /**
     * Endpoint configuration
     */
    private readonly DirectSyncEndpointConfiguration endpointConfiguration;

    /**
     * Reply channel store
     */
    private ICorrelationManager<IMessageQueue> _correlationManager;

    public DirectSyncConsumer(string name, DirectSyncEndpointConfiguration endpointConfiguration) : base(name,
        endpointConfiguration)
    {
        this.endpointConfiguration = endpointConfiguration;

        _correlationManager =
            new PollingCorrelationManager<IMessageQueue>(endpointConfiguration, "Reply channel not set up yet");
    }

    /// Gets the correlation manager.
    /// @return
    /// /
    public ICorrelationManager<IMessageQueue> CorrelationManager
    {
        get => _correlationManager;
        set => _correlationManager = value;
    }

    /// Sends a message to the appropriate reply queue based on the correlation key found in the context.
    /// <param name="message">The message to be sent.</param>
    /// <param name="context">
    ///     The context in which the message is being processed, containing state and configuration
    ///     information.
    /// </param>
    public void Send(IMessage message, TestContext context)
    {
        ObjectHelper.AssertNotNull(message, "Cannot send empty message");

        var correlationKeyName = endpointConfiguration.Correlator.GetCorrelationKeyName(Name);
        var correlationKey = _correlationManager.GetCorrelationKey(correlationKeyName, context);
        var replyQueue = _correlationManager.Find(correlationKey, endpointConfiguration.Timeout);
        ObjectHelper.AssertNotNull(replyQueue,
            $"Failed to find reply channel for message correlation key: {correlationKey}");

        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug($"Sending message to reply channel: '{replyQueue}'");
            Log.LogDebug($"Message to send is:\n{message}");
        }

        replyQueue.Send(message);
        Log.LogInformation($"Message was sent to reply channel: '{replyQueue}'");
    }

    /// Receives a message from the specified message queue based on the selector and context provided.
    /// <param name="selector">The selector that determines which messages to receive.</param>
    /// <param name="context">
    ///     The context in which the message is being processed, containing state and configuration
    ///     information.
    /// </param>
    /// <param name="timeout">The maximum time in milliseconds to wait for a message before timing out.</param>
    /// <return>Returns the received message that matches the selector criteria.</return>
    public override IMessage Receive(string selector, TestContext context, long timeout)
    {
        var receivedMessage = base.Receive(selector, context, timeout);
        SaveReplyMessageQueue(receivedMessage, context);

        return receivedMessage;
    }

    /// Saves the reply message queue, if specified in the received message headers.
    /// <param name="receivedMessage">The received message that may contain the reply queue information.</param>
    /// <param name="context">The context in which the message is processed, including configuration and state.</param>
    public void SaveReplyMessageQueue(IMessage receivedMessage, TestContext context)
    {
        IMessageQueue replyQueue = null;

        if (receivedMessage.GetHeader(DirectMessageHeaders.ReplyQueue) is IMessageQueue queue)
        {
            replyQueue = queue;
        }
        else if (!string.IsNullOrWhiteSpace(receivedMessage.GetHeader(DirectMessageHeaders.ReplyQueue)?.ToString()))
        {
            replyQueue = ResolveQueueName(receivedMessage.GetHeader(DirectMessageHeaders.ReplyQueue).ToString(),
                context);
        }

        if (replyQueue != null)
        {
            var correlationKeyName = endpointConfiguration.Correlator.GetCorrelationKeyName(Name);
            var correlationKey = endpointConfiguration.Correlator.GetCorrelationKey(receivedMessage);
            _correlationManager.SaveCorrelationKey(correlationKeyName, correlationKey, context);
            _correlationManager.Store(correlationKey, replyQueue);
        }
        else
        {
            Log.LogWarning(
                "Unable to retrieve reply message channel for message \n{0}\n - no reply channel found in message headers!",
                receivedMessage);
        }
    }
}
