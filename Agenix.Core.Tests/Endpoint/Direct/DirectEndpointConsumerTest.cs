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
using System.Threading.Tasks;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Api.Spi;
using Agenix.Core.Endpoint.Direct;
using Agenix.Core.Message;
using Moq;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.Tests.Endpoint.Direct;

public class DirectEndpointConsumerTest
{
    private TestContext _context;

    private Mock<IMessageQueue> _queueMock;
    private Mock<IReferenceResolver> _resolverMock;

    [SetUp]
    public void SetupMocks()
    {
        _queueMock = new Mock<IMessageQueue>();
        _resolverMock = new Mock<IReferenceResolver>();
        _context = new TestContext();
    }

    [Test]
    public async Task TestReceiveMessage()
    {
        var endpoint = new DirectEndpoint();
        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);

        var headers = new Dictionary<string, object>();
        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>", headers);

        _queueMock.Reset();

        _queueMock.Setup(q => q.Receive(5000L)).ReturnsAsync(message);

        var receivedMessage = await endpoint.CreateConsumer().Receive(_context);

        Assert.That(receivedMessage.Payload, Is.EqualTo(message.Payload));
        Assert.That(receivedMessage.GetHeader(MessageHeaders.Id), Is.EqualTo(message.Id));
    }

    [Test]
    public async Task TestReceiveMessageQueueNameResolver()
    {
        var endpoint = new DirectEndpoint();
        endpoint.EndpointConfiguration.SetQueueName("testQueue");

        _context.SetReferenceResolver(_resolverMock.Object);

        var headers = new Dictionary<string, object>();
        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>", headers);

        _queueMock.Reset();
        _resolverMock.Reset();

        _resolverMock.Setup(r => r.Resolve<IMessageQueue>("testQueue")).Returns(_queueMock.Object);
        _queueMock.Setup(q => q.Receive(5000L)).ReturnsAsync(message);

        var receivedMessage = await endpoint.CreateConsumer().Receive(_context);

        Assert.That(receivedMessage.Payload, Is.EqualTo(message.Payload));
        Assert.That(receivedMessage.GetHeader(MessageHeaders.Id), Is.EqualTo(message.Id));
    }

    [Test]
    public async Task TestReceiveMessageWithCustomTimeout()
    {
        var endpoint = new DirectEndpoint();

        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);
        endpoint.EndpointConfiguration.Timeout = 10000L;

        var headers = new Dictionary<string, object>();
        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>", headers);

        _queueMock.Reset();

        _queueMock.Setup(q => q.Receive(10000L)).ReturnsAsync(message);

        var receivedMessage = await endpoint.CreateConsumer().Receive(_context);

        Assert.That(receivedMessage.Payload, Is.EqualTo(message.Payload));
        Assert.That(receivedMessage.GetHeader(MessageHeaders.Id), Is.EqualTo(message.Id));
    }

    [Test]
    public async Task TestReceiveMessageTimeoutOverride()
    {
        var endpoint = new DirectEndpoint();

        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);
        endpoint.EndpointConfiguration.Timeout = 10000L;

        var headers = new Dictionary<string, object>();
        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>", headers);

        _queueMock.Reset();

        _queueMock.Setup(q => q.Receive(25000L)).ReturnsAsync(message);

        var receivedMessage = await endpoint.CreateConsumer().Receive(_context, 25000L);

        Assert.That(receivedMessage.Payload, Is.EqualTo(message.Payload));
        Assert.That(receivedMessage.GetHeader(MessageHeaders.Id), Is.EqualTo(message.Id));
    }

    [Test]
    public async Task TestReceiveTimeout()
    {
        var endpoint = new DirectEndpoint();

        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);

        _queueMock.Reset();

        _queueMock.Setup(q => q.Receive(5000L)).ReturnsAsync((IMessage)null);

        try
        {
            await endpoint.CreateConsumer().Receive(_context);
            Assert.Fail("Missing ActionTimeoutException because no message was received");
        }
        catch (ActionTimeoutException e)
        {
            Assert.That(
                e.Message,
                Does.StartWith("Action timeout after 5000 milliseconds. Failed to receive message on endpoint"));
        }
    }

    [Test]
    public async Task TestReceiveSelected()
    {
        var endpoint = new DirectEndpoint();

        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);
        endpoint.EndpointConfiguration.Timeout = 0L;

        try
        {
            await endpoint.CreateConsumer().Receive("Operation = 'sayHello'", _context);
            Assert.Fail("Missing exception due to unsupported operation");
        }
        catch (AgenixSystemException e)
        {
            Assert.That(e.Message, Is.Not.Null);
        }

        var queueQueueMock = new Mock<IMessageQueue>();
        var message = new DefaultMessage("Hello").SetHeader("Operation", "sayHello");

        queueQueueMock
            .Setup(q => q.Receive(It.IsAny<MessageSelector>()))
            .ReturnsAsync(message);

        endpoint.EndpointConfiguration.SetQueue(queueQueueMock.Object);
        var receivedMessage = await endpoint.CreateConsumer().Receive("Operation = 'sayHello'", _context);

        Assert.That(receivedMessage.Payload, Is.EqualTo(message.Payload));
        Assert.That(receivedMessage.GetHeader(MessageHeaders.Id), Is.EqualTo(message.Id));
        Assert.That(receivedMessage.GetHeader("Operation"), Is.EqualTo("sayHello"));
    }

    [Test]
    public async Task TestReceiveSelectedNoMessageWithTimeout()
    {
        var endpoint = new DirectEndpoint();

        _queueMock.Reset();
        _queueMock.Setup(q => q.Receive(It.IsAny<MessageSelector>(), 1500L))
            .ReturnsAsync((IMessage)null); // force retry

        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);

        try
        {
            await endpoint.CreateConsumer().Receive("Operation = 'sayHello'", _context, 1500L);
            Assert.Fail("Missing ActionTimeoutException because no message was received");
        }
        catch (ActionTimeoutException e)
        {
            Assert.That(
                e.Message,
                Does.StartWith("Action timeout after 1500 milliseconds. Failed to receive message on endpoint"));
        }
    }
}
