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

public class DirectEndpointProducerTest
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
    public void TestSendMessage()
    {
        var endpoint = new DirectEndpoint();

        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);

        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        _queueMock.Reset();

        endpoint.CreateProducer().Send(message, _context);

        _queueMock.Verify(q => q.Send(It.IsAny<IMessage>()), Times.Once);
    }

    [Test]
    public void TestSendMessageQueueNameResolver()
    {
        var endpoint = new DirectEndpoint();

        endpoint.EndpointConfiguration.SetQueueName("testQueue");

        _context.SetReferenceResolver(_resolverMock.Object);

        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        _queueMock.Reset();
        _resolverMock.Reset();

        _resolverMock.Setup(r => r.Resolve<IMessageQueue>("testQueue")).Returns(_queueMock.Object);

        endpoint.CreateProducer().Send(message, _context);

        _queueMock.Verify(q => q.Send(It.IsAny<IMessage>()), Times.Once);
    }

    [Test]
    public void TestSendMessageFailed()
    {
        var endpoint = new DirectEndpoint();

        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);

        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        _queueMock.Reset();

        _queueMock.Setup(q => q.Send(It.IsAny<IMessage>())).Throws(new SystemException("Internal error!"));

        try
        {
            endpoint.CreateProducer().Send(message, _context);
        }
        catch (AgenixSystemException e)
        {
            ClassicAssert.IsTrue(e.Message.StartsWith("Failed to send message to queue: "));
            ClassicAssert.IsNotNull(e.InnerException);
            ClassicAssert.AreEqual(e.InnerException.GetType(), typeof(SystemException));
            ClassicAssert.AreEqual(e.InnerException.Message, "Internal error!");
            return;
        }

        Assert.Fail("Missing " + nameof(AgenixSystemException) + " because no message was received");
    }
}
