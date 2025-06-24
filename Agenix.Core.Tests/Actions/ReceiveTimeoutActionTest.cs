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

using Agenix.Api.Endpoint;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Core.Actions;
using Agenix.Core.Message;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Actions;

/// <summary>
///     Unit tests for the <c>ReceiveTimeoutAction</c> class, using NUnit Framework.
/// </summary>
public class ReceiveTimeoutActionTest : AbstractNUnitSetUp
{
    private Mock<ISelectiveConsumer> _consumer;
    private Mock<IEndpoint> _endpoint;
    private Mock<IEndpointConfiguration> _endpointConfiguration;

    [SetUp]
    public void SetUp()
    {
        _endpoint = new Mock<IEndpoint>();
        _consumer = new Mock<ISelectiveConsumer>();
        _endpointConfiguration = new Mock<IEndpointConfiguration>();
    }

    [Test]
    public void TestReceiveTimeout()
    {
        // Arrange
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        _consumer.Setup(c => c.Receive(Context, 1000L)).Returns((IMessage)null);

        var receiveTimeout = new ReceiveTimeoutAction.Builder()
            .Endpoint(_endpoint.Object)
            .Build();

        // Act
        receiveTimeout.Execute(Context);
    }

    [Test]
    public void TestReceiveTimeoutCustomTimeout()
    {
        // Arrange
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        _consumer.Setup(c => c.Receive(Context, 500L)).Returns((IMessage)null);

        var receiveTimeout = new ReceiveTimeoutAction.Builder()
            .Endpoint(_endpoint.Object)
            .Timeout(500L)
            .Build();

        // Act
        receiveTimeout.Execute(Context);
    }

    [Test]
    public void TestReceiveTimeoutFail()
    {
        // Arrange
        var message = new DefaultMessage("<TestMessage>Hello World!</TestMessage>");
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        _consumer.Setup(c => c.Receive(Context, 1000L)).Returns(message);

        var receiveTimeout = new ReceiveTimeoutAction.Builder()
            .Endpoint(_endpoint.Object)
            .Build();

        // Act & Assert
        var ex = Assert.Throws<AgenixSystemException>(() => receiveTimeout.Execute(Context));
        Assert.That(ex, Is.Not.Null);
        ClassicAssert.AreEqual(
            "Message timeout validation failed! Received message while waiting for timeout on destination", ex.Message);
    }

    [Test]
    public void TestReceiveTimeoutWithMessageSelector()
    {
        // Arrange
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        _consumer.Setup(c => c.Receive("Operation = 'sayHello'", Context, 1000L)).Returns((IMessage)null);

        var receiveTimeout = new ReceiveTimeoutAction.Builder()
            .Endpoint(_endpoint.Object)
            .Selector("Operation = 'sayHello'")
            .Build();

        // Act
        receiveTimeout.Execute(Context);
    }

    [Test]
    public void TestReceiveTimeoutWithMessageSelectorVariableSupport()
    {
        // Arrange
        Context.SetVariable("operation", "sayHello");

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        _consumer.Setup(c => c.Receive("Operation = 'sayHello'", Context, 1000L)).Returns((IMessage)null);

        var receiveTimeout = new ReceiveTimeoutAction.Builder()
            .Endpoint(_endpoint.Object)
            .Selector("Operation = '${operation}'")
            .Build();

        // Act
        receiveTimeout.Execute(Context);
    }
}
