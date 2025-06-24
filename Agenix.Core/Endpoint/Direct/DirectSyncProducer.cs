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
