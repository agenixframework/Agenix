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

using System.Collections.Generic;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Api.Spi;
using Agenix.Core.Endpoint.Direct;
using Agenix.Core.Message;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.Tests.Endpoint.Direct;

public class DirectEndpointSyncConsumerTest
{
    private TestContext _context;
    private Mock<IMessageCorrelator> _messageCorrelatorMock;
    private Mock<IMessageQueue> _queueMock;
    private Mock<IMessageQueue> _replyQueueMock;
    private Mock<IReferenceResolver> _resolverMock;

    [SetUp]
    public void SetupMocks()
    {
        _queueMock = new Mock<IMessageQueue>();
        _replyQueueMock = new Mock<IMessageQueue>();
        _messageCorrelatorMock = new Mock<IMessageCorrelator>();
        _resolverMock = new Mock<IReferenceResolver>();
        _context = new TestContext();
    }

    [Test]
    public void TestReceiveMessageWithReplyQueue()
    {
        var endpoint = new DirectSyncEndpoint();
        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);

        var headers = new Dictionary<string, object> { { DirectMessageHeaders.ReplyQueue, _replyQueueMock.Object } };

        var message = new DefaultMessage("<TestResponse>Hello World!</TestResponse>", headers);

        _queueMock.Setup(q => q.Receive(5000L)).Returns(message);

        var channelSyncConsumer = (DirectSyncConsumer)endpoint.CreateConsumer();
        var receivedMessage = channelSyncConsumer.Receive(_context);

        ClassicAssert.AreEqual(message.Payload, receivedMessage.Payload);
        ClassicAssert.AreEqual(message.GetHeader(MessageHeaders.Id), receivedMessage.GetHeader(MessageHeaders.Id));
        ClassicAssert.AreEqual(message.GetHeader(DirectMessageHeaders.ReplyQueue),
            receivedMessage.GetHeader(DirectMessageHeaders.ReplyQueue));

        var savedReplyQueue = channelSyncConsumer.CorrelationManager.Find(
            endpoint.EndpointConfiguration.Correlator.GetCorrelationKey(receivedMessage),
            endpoint.EndpointConfiguration.Timeout);

        ClassicAssert.NotNull(savedReplyQueue);
        ClassicAssert.AreEqual(_replyQueueMock.Object, savedReplyQueue);
    }

    [Test]
    public void TestReceiveMessageQueueNameResolver()
    {
        var endpoint = new DirectSyncEndpoint();
        endpoint.EndpointConfiguration.SetQueueName("testQueue");

        _context.SetReferenceResolver(_resolverMock.Object);

        var headers = new Dictionary<string, object>();
        var message = new DefaultMessage("<TestResponse>Hello World!</TestResponse>", headers)
            .SetHeader(DirectMessageHeaders.ReplyQueue, _replyQueueMock.Object);

        Mock.Get(_queueMock.Object).Reset();
        Mock.Get(_replyQueueMock.Object).Reset();
        Mock.Get(_resolverMock.Object).Reset();

        _resolverMock.Setup(r => r.Resolve<IMessageQueue>("testQueue")).Returns(_queueMock.Object);
        _queueMock.Setup(q => q.Receive(5000L)).Returns(message);

        var channelSyncConsumer = (DirectSyncConsumer)endpoint.CreateConsumer();
        var receivedMessage = channelSyncConsumer.Receive(_context);

        ClassicAssert.AreEqual(receivedMessage.Payload, message.Payload);
        ClassicAssert.AreEqual(receivedMessage.GetHeader(MessageHeaders.Id), message.Id);
        ClassicAssert.AreEqual(receivedMessage.GetHeader(DirectMessageHeaders.ReplyQueue),
            message.GetHeader(DirectMessageHeaders.ReplyQueue));

        var savedReplyQueue = channelSyncConsumer.CorrelationManager.Find(
            endpoint.EndpointConfiguration.Correlator.GetCorrelationKey(receivedMessage),
            endpoint.EndpointConfiguration.Timeout);

        ClassicAssert.NotNull(savedReplyQueue);
        ClassicAssert.AreEqual(savedReplyQueue, _replyQueueMock.Object);
    }

    [Test]
    public void TestReceiveMessageWithReplyQueueName()
    {
        var endpoint = new DirectSyncEndpoint();
        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);

        var headers = new Dictionary<string, object>();
        var message = new DefaultMessage("<TestResponse>Hello World!</TestResponse>", headers)
            .SetHeader(DirectMessageHeaders.ReplyQueue, "replyQueue");

        _context.SetReferenceResolver(_resolverMock.Object);

        Mock.Get(_queueMock.Object).Reset();
        Mock.Get(_replyQueueMock.Object).Reset();
        Mock.Get(_resolverMock.Object).Reset();

        _queueMock.Setup(q => q.Receive(5000L)).Returns(message);
        _resolverMock.Setup(r => r.Resolve<IMessageQueue>("replyQueue")).Returns(_replyQueueMock.Object);

        var channelSyncConsumer = (DirectSyncConsumer)endpoint.CreateConsumer();
        var receivedMessage = channelSyncConsumer.Receive(_context);

        ClassicAssert.AreEqual(receivedMessage.Payload, message.Payload);
        ClassicAssert.AreEqual(receivedMessage.GetHeader(MessageHeaders.Id), message.Id);
        ClassicAssert.AreEqual(receivedMessage.GetHeader(DirectMessageHeaders.ReplyQueue), "replyQueue");

        var savedReplyQueue = channelSyncConsumer.CorrelationManager.Find(
            endpoint.EndpointConfiguration.Correlator.GetCorrelationKey(receivedMessage),
            endpoint.EndpointConfiguration.Timeout);

        ClassicAssert.NotNull(savedReplyQueue);
        ClassicAssert.AreEqual(savedReplyQueue, _replyQueueMock.Object);
    }

    [Test]
    public void TestReceiveMessageWithCustomTimeout()
    {
        var endpoint = new DirectSyncEndpoint();
        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);
        endpoint.EndpointConfiguration.Timeout = 10000L;

        var headers = new Dictionary<string, object>();
        var message = new DefaultMessage("<TestResponse>Hello World!</TestResponse>", headers)
            .SetHeader(DirectMessageHeaders.ReplyQueue, _replyQueueMock.Object);

        Mock.Get(_queueMock.Object).Reset();
        Mock.Get(_replyQueueMock.Object).Reset();

        _queueMock.Setup(q => q.Receive(10000L)).Returns(message);

        var channelSyncConsumer = (DirectSyncConsumer)endpoint.CreateConsumer();
        var receivedMessage = channelSyncConsumer.Receive(_context);

        ClassicAssert.AreEqual(receivedMessage.Payload, message.Payload);
        ClassicAssert.AreEqual(receivedMessage.GetHeader(MessageHeaders.Id), message.Id);

        var savedReplyQueue = channelSyncConsumer.CorrelationManager.Find(
            endpoint.EndpointConfiguration.Correlator.GetCorrelationKey(receivedMessage),
            endpoint.EndpointConfiguration.Timeout);

        ClassicAssert.NotNull(savedReplyQueue);
        ClassicAssert.AreEqual(savedReplyQueue, _replyQueueMock.Object);
    }

    [Test]
    public void TestReceiveMessageWithReplyMessageCorrelator()
    {
        var endpoint = new DirectSyncEndpoint();
        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);
        endpoint.EndpointConfiguration.Correlator = _messageCorrelatorMock.Object;
        endpoint.EndpointConfiguration.Timeout = 500L;
        endpoint.EndpointConfiguration.PollingInterval = 100;

        var headers = new Dictionary<string, object>();
        var message = new DefaultMessage("<TestResponse>Hello World!</TestResponse>", headers)
            .SetHeader(DirectMessageHeaders.ReplyQueue, _replyQueueMock.Object);

        Mock.Get(_queueMock.Object).Reset();
        Mock.Get(_replyQueueMock.Object).Reset();
        Mock.Get(_messageCorrelatorMock.Object).Reset();

        _queueMock.Setup(q => q.Receive(500L)).Returns(message);
        _messageCorrelatorMock.Setup(mc => mc.GetCorrelationKey(It.IsAny<IMessage>()))
            .Returns(MessageHeaders.Id + " = '123456789'");
        _messageCorrelatorMock.Setup(mc => mc.GetCorrelationKeyName(It.IsAny<string>())).Returns("correlationKeyName");

        var channelSyncConsumer = (DirectSyncConsumer)endpoint.CreateConsumer();
        var receivedMessage = channelSyncConsumer.Receive(_context);

        ClassicAssert.AreEqual(receivedMessage.Payload, message.Payload);
        ClassicAssert.AreEqual(receivedMessage.GetHeader(MessageHeaders.Id), message.Id);

        ClassicAssert.IsNull(channelSyncConsumer.CorrelationManager.Find("", endpoint.EndpointConfiguration.Timeout));
        ClassicAssert.IsNull(channelSyncConsumer.CorrelationManager.Find(MessageHeaders.Id + " = 'totally_wrong'",
            endpoint.EndpointConfiguration.Timeout));

        var savedReplyQueue = channelSyncConsumer.CorrelationManager.Find(MessageHeaders.Id + " = '123456789'",
            endpoint.EndpointConfiguration.Timeout);
        ClassicAssert.IsNotNull(savedReplyQueue);
        ClassicAssert.AreEqual(savedReplyQueue, _replyQueueMock.Object);
    }

    [Test]
    public void TestReceiveNoMessage()
    {
        var endpoint = new DirectSyncEndpoint();
        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);

        Mock.Get(_queueMock.Object).Reset();
        Mock.Get(_replyQueueMock.Object).Reset();

        _queueMock.Setup(q => q.Receive(5000L)).Returns((IMessage)null);

        try
        {
            var channelSyncConsumer = (DirectSyncConsumer)endpoint.CreateConsumer();
            channelSyncConsumer.Receive(_context);
        }
        catch (ActionTimeoutException e)
        {
            StringAssert.StartsWith("Action timeout after 5000 milliseconds. Failed to receive message on endpoint",
                e.Message);
            return;
        }

        Assert.Fail("Expected ActionTimeoutException because no message was received.");
    }

    [Test]
    public void TestReceiveMessageNoReplyQueue()
    {
        var endpoint = new DirectSyncEndpoint();
        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);
        endpoint.EndpointConfiguration.Timeout = 500L;
        endpoint.EndpointConfiguration.PollingInterval = 150L;

        var headers = new Dictionary<string, object>();
        var message = new DefaultMessage("<TestResponse>Hello World!</TestResponse>", headers);

        Mock.Get(_queueMock.Object).Reset();
        Mock.Get(_replyQueueMock.Object).Reset();

        _queueMock.Setup(q => q.Receive(500L)).Returns(message);

        var channelSyncConsumer = (DirectSyncConsumer)endpoint.CreateConsumer();
        var receivedMessage = channelSyncConsumer.Receive(_context);

        ClassicAssert.AreEqual(receivedMessage.Payload, message.Payload);
        ClassicAssert.AreEqual(receivedMessage.GetHeader(MessageHeaders.Id), message.Id);

        var savedReplyQueue = channelSyncConsumer.CorrelationManager.Find("", endpoint.EndpointConfiguration.Timeout);
        ClassicAssert.IsNull(savedReplyQueue);
    }

    [Test]
    public void TestSendReplyMessage()
    {
        var endpoint = new DirectSyncEndpoint();

        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        Mock.Get(_replyQueueMock.Object).Reset();

        var channelSyncConsumer = (DirectSyncConsumer)endpoint.CreateConsumer();
        channelSyncConsumer.SaveReplyMessageQueue(
            new DefaultMessage("").SetHeader(DirectMessageHeaders.ReplyQueue, _replyQueueMock.Object), _context);
        channelSyncConsumer.Send(message, _context);

        _replyQueueMock.Verify(rq => rq.Send(It.IsAny<IMessage>()), Times.Once);
    }

    [Test]
    public void TestSendReplyMessageWithReplyMessageCorrelator()
    {
        var endpoint = new DirectSyncEndpoint();

        var correlator = new DefaultMessageCorrelator();
        endpoint.EndpointConfiguration.Correlator = correlator;

        var request = new DefaultMessage("").SetHeader(DirectMessageHeaders.ReplyQueue, _replyQueueMock.Object);

        var channelSyncConsumer = (DirectSyncConsumer)endpoint.CreateConsumer();
        channelSyncConsumer.CorrelationManager.SaveCorrelationKey(
            endpoint.EndpointConfiguration.Correlator.GetCorrelationKeyName(endpoint.CreateConsumer().Name),
            request.Id, _context);

        var headers = new Dictionary<string, object>();
        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>", headers);

        Mock.Get(_replyQueueMock.Object).Reset();

        _replyQueueMock.Setup(rq => rq.Send(It.IsAny<IMessage>()))
            .Callback<IMessage>(msg => { ClassicAssert.AreEqual(message.Payload, msg.Payload); });

        channelSyncConsumer.SaveReplyMessageQueue(request, _context);
        channelSyncConsumer.Send(message, _context);

        _replyQueueMock.Verify(rq => rq.Send(It.IsAny<IMessage>()), Times.Once);
    }

    [Test]
    public void TestSendReplyMessageWithMissingCorrelatorKey()
    {
        var endpoint = new DirectSyncEndpoint();

        var correlator = new DefaultMessageCorrelator();
        endpoint.EndpointConfiguration.Correlator = correlator;

        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        try
        {
            var channelSyncConsumer = (DirectSyncConsumer)endpoint.CreateConsumer();
            channelSyncConsumer.Send(message, _context);
        }
        catch (AgenixSystemException e)
        {
            StringAssert.StartsWith("Failed to get correlation key for", e.Message);
            return;
        }

        Assert.Fail("Expected CoreSystemException due to missing correlation key.");
    }

    [Test]
    public void TestNoCorrelationKeyFound()
    {
        var endpoint = new DirectSyncEndpoint();

        var correlator = new DefaultMessageCorrelator();
        endpoint.EndpointConfiguration.Correlator = correlator;

        var dummyEndpoint = new DirectSyncEndpoint { Name = "dummyEndpoint" };
        var dummyConsumer = (DirectSyncConsumer)dummyEndpoint.CreateConsumer();
        dummyConsumer.CorrelationManager.SaveCorrelationKey(
            dummyEndpoint.EndpointConfiguration.Correlator.GetCorrelationKeyName(dummyEndpoint.CreateConsumer().Name),
            "123456789", _context);

        var headers = new Dictionary<string, object>();
        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>", headers);

        try
        {
            var channelSyncConsumer = (DirectSyncConsumer)endpoint.CreateConsumer();
            channelSyncConsumer.Send(message, _context);
        }
        catch (AgenixSystemException e)
        {
            StringAssert.StartsWith("Failed to get correlation key", e.Message);
            return;
        }

        Assert.Fail("Expected CoreSystemException due to no reply destination found.");
    }

    [Test]
    public void TestNoReplyDestinationFound()
    {
        var endpoint = new DirectSyncEndpoint();
        endpoint.EndpointConfiguration.Timeout = 1000L;

        var correlator = new DefaultMessageCorrelator();
        endpoint.EndpointConfiguration.Correlator = correlator;

        var dummyConsumer = (DirectSyncConsumer)endpoint.CreateConsumer();
        dummyConsumer.CorrelationManager.SaveCorrelationKey(
            endpoint.EndpointConfiguration.Correlator.GetCorrelationKeyName(endpoint.CreateConsumer().Name),
            "123456789", _context);

        var headers = new Dictionary<string, object>();
        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>", headers);

        var ex = Assert.Throws<AgenixSystemException>(() =>
        {
            var channelSyncConsumer = (DirectSyncConsumer)endpoint.CreateConsumer();
            channelSyncConsumer.Send(message, _context);
        });

        StringAssert.IsMatch("Failed to find reply channel for message correlation key: 123456789", ex.Message);
    }

    [Test]
    public void TestSendEmptyMessage()
    {
        var endpoint = new DirectSyncEndpoint();
        var channelSyncConsumer = (DirectSyncConsumer)endpoint.CreateConsumer();

        var ex = Assert.Throws<AgenixSystemException>(() => { channelSyncConsumer.Send(null, _context); });

        StringAssert.IsMatch("Cannot send empty message", ex.Message);
    }

    [Test]
    public void TestSendReplyMessageFail()
    {
        var endpoint = new DirectSyncEndpoint();
        var replyQueue = new Mock<IMessageQueue>();

        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        replyQueue.Setup(rq => rq.Send(It.IsAny<IMessage>())).Throws(new AgenixSystemException("Internal error!"));

        var ex = Assert.Throws<AgenixSystemException>(() =>
        {
            var channelSyncConsumer = (DirectSyncConsumer)endpoint.CreateConsumer();
            channelSyncConsumer.SaveReplyMessageQueue(
                new DefaultMessage("").SetHeader(DirectMessageHeaders.ReplyQueue, replyQueue.Object),
                _context);
            channelSyncConsumer.Send(message, _context);
        });

        ClassicAssert.AreEqual(typeof(AgenixSystemException), ex.GetType());
        ClassicAssert.AreEqual("Internal error!", ex.Message);
    }
}
