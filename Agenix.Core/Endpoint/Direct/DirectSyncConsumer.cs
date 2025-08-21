#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
//
// Copyright (c) 2025 Agenix
//
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

#endregion

using System.Threading.Tasks;
using Agenix.Api.Context;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Message.Correlation;
using Agenix.Api.Messaging;
using Agenix.Core.Message.Correlation;
using Agenix.Core.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Endpoint.Direct;

/// <summary>
///     Provides a synchronous consumer implementation that interacts with direct endpoints in a messaging context.
///     This class extends functionality from <see cref="DirectConsumer" /> and implements <see cref="IReplyProducer" />
///     to facilitate reply message production and correlation management.
/// </summary>
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
    private readonly DirectSyncEndpointConfiguration _endpointConfiguration;

    /// Represents a synchronous direct consumer which processes and consumes messages using
    /// a specific endpoint configuration tied to direct message queues.
    public DirectSyncConsumer(string name, DirectSyncEndpointConfiguration endpointConfiguration) : base(name,
        endpointConfiguration)
    {
        _endpointConfiguration = endpointConfiguration;

        CorrelationManager =
            new PollingCorrelationManager<IMessageQueue>(endpointConfiguration, "Reply channel not set up yet");
    }

    /// Gets the correlation manager.
    /// @return
    /// /
    public ICorrelationManager<IMessageQueue> CorrelationManager { get; set; }

    /// Sends a message to the appropriate reply queue based on the correlation key found in the context.
    /// <param name="message">The message to be sent.</param>
    /// <param name="context">
    ///     The context in which the message is being processed, containing state and configuration
    ///     information.
    /// </param>
    public async Task Send(IMessage message, TestContext context)
    {
        ObjectHelper.AssertNotNull(message, "Cannot send empty message");

        var correlationKeyName = _endpointConfiguration.Correlator.GetCorrelationKeyName(Name);
        var correlationKey = await CorrelationManager.GetCorrelationKey(correlationKeyName, context);
        var replyQueue = await CorrelationManager.Find(correlationKey, _endpointConfiguration.Timeout);
        ObjectHelper.AssertNotNull(replyQueue,
            $"Failed to find reply channel for message correlation key: {correlationKey}");

        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug("Sending message to reply channel: '{ReplyQueue}'", replyQueue);
            Log.LogDebug("Message to send is:\n{Message}", message);
        }

        await replyQueue.Send(message);
        Log.LogInformation("Message was sent to reply channel: '{ReplyQueue}'", replyQueue);
    }

    /// Receives a message from the specified message queue based on the selector and context provided.
    /// <param name="selector">The selector that determines which messages to receive.</param>
    /// <param name="context">
    ///     The context in which the message is being processed, containing state and configuration
    ///     information.
    /// </param>
    /// <param name="timeout">The maximum time in milliseconds to wait for a message before timing out.</param>
    /// <return>Returns the received message that matches the selector criteria.</return>
    public override async Task<IMessage> Receive(string selector, TestContext context, long timeout)
    {
        var receivedMessage = await base.Receive(selector, context, timeout);
        SaveReplyMessageQueue(receivedMessage, context);

        return receivedMessage;
    }

    /// Saves the reply message queue, if specified in the received message headers.
    /// <param name="receivedMessage">The received message that may contain the reply queue information.</param>
    /// <param name="context">The context in which the message is processed, including configuration and state.</param>
    public void SaveReplyMessageQueue(IMessage receivedMessage, TestContext context)
    {
        IMessageQueue replyQueue = null;

        var replyHeader = receivedMessage.GetHeader(DirectMessageHeaders.ReplyQueue);
        if (replyHeader is IMessageQueue queue)
        {
            replyQueue = queue;
        }
        else if (replyHeader != null)
        {
            var replyName = replyHeader.ToString();
            if (!string.IsNullOrWhiteSpace(replyName))
            {
                replyQueue = ResolveQueueName(replyName, context);
            }
        }

        if (replyQueue != null)
        {
            var correlationKeyName = _endpointConfiguration.Correlator.GetCorrelationKeyName(Name);
            var correlationKey = _endpointConfiguration.Correlator.GetCorrelationKey(receivedMessage);
            CorrelationManager.SaveCorrelationKey(correlationKeyName, correlationKey, context);
            CorrelationManager.Store(correlationKey, replyQueue);
        }
        else
        {
            Log.LogWarning(
                "Unable to retrieve reply message channel for message \n{ReceivedMessage}\n - no reply channel found in message headers!",
                receivedMessage);
        }
    }
}
