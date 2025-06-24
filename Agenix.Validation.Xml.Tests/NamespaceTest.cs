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
using Agenix.Api.Messaging;
using Agenix.Core.Actions;
using Agenix.Core.Message;
using Agenix.Core.Message.Builder;
using Agenix.Core.Validation.Builder;
using Agenix.Core.Validation.Xml;
using Moq;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Xml.Tests;

public class NamespaceTest : AbstractNUnitSetUp
{
    private Mock<IConsumer> _consumer;
    private Mock<IEndpoint> _endpoint;
    private Mock<IEndpointConfiguration> _endpointConfiguration;

    [SetUp]
    public void SetUp()
    {
        _endpoint = new Mock<IEndpoint>();
        _consumer = new Mock<IConsumer>();
        _endpointConfiguration = new Mock<IEndpointConfiguration>();
    }

    [Test]
    public void TestNamespaces()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<ns1:root xmlns:ns1='http://agenix'>" +
                                         "<ns1:element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<ns1:sub-element attribute='A'>text-value</ns1:sub-element>" +
                                         "</ns1:element>" +
                                         "</ns1:root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<ns1:root xmlns:ns1='http://agenix'>" +
                                                                          "<ns1:element attributeA='attribute-value' attributeB='attribute-value'>" +
                                                                          "<ns1:sub-element attribute='A'>text-value</ns1:sub-element>" +
                                                                          "</ns1:element>" +
                                                                          "</ns1:root>"));

        var validationContext = new XmlMessageValidationContext.Builder()
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Act & Assert - Should complete successfully since both messages have matching namespaces
        Assert.That(() => receiveAction.Execute(Context), Throws.Nothing);

        // Verify consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestDifferentNamespacePrefix()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<ns1:root xmlns:ns1='http://agenix'>" +
                                         "<ns1:element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<ns1:sub-element attribute='A'>text-value</ns1:sub-element>" +
                                         "</ns1:element>" +
                                         "</ns1:root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<ns2:root xmlns:ns2='http://agenix'>" +
                                                                          "<ns2:element attributeA='attribute-value' attributeB='attribute-value'>" +
                                                                          "<ns2:sub-element attribute='A'>text-value</ns2:sub-element>" +
                                                                          "</ns2:element>" +
                                                                          "</ns2:root>"));

        var validationContext = new XmlMessageValidationContext.Builder()
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Act & Assert - Should complete successfully despite different namespace prefixes
        Assert.That(() => receiveAction.Execute(Context), Throws.Nothing);

        // Verify consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestAdditionalNamespace()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<ns1:root xmlns:ns1='http://agenix'>" +
                                         "<ns1:element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<ns1:sub-element attribute='A'>text-value</ns1:sub-element>" +
                                         "</ns1:element>" +
                                         "</ns1:root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<ns1:root xmlns:ns1='http://agenix' xmlns:ns2='http://agenix/default'>" +
            "<ns1:element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<ns1:sub-element attribute='A'>text-value</ns1:sub-element>" +
            "</ns1:element>" +
            "</ns1:root>"));

        var validationContext = new XmlMessageValidationContext.Builder()
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Act & Assert - Should complete successfully despite additional namespace declaration
        Assert.That(() => receiveAction.Execute(Context), Throws.Nothing);

        // Verify consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestMissingNamespaceDeclaration()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<ns1:root xmlns:ns1='http://agenix' xmlns:ns2='http://agenix/default'>" +
                                         "<ns1:element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<ns1:sub-element attribute='A'>text-value</ns1:sub-element>" +
                                         "</ns1:element>" +
                                         "</ns1:root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<ns1:root xmlns:ns1='http://agenix'>" +
                                                                          "<ns1:element attributeA='attribute-value' attributeB='attribute-value'>" +
                                                                          "<ns1:sub-element attribute='A'>text-value</ns1:sub-element>" +
                                                                          "</ns1:element>" +
                                                                          "</ns1:root>"));

        var validationContext = new XmlMessageValidationContext.Builder()
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Act & Assert - Should complete successfully despite missing namespace declaration in control
        Assert.That(() => receiveAction.Execute(Context), Throws.Nothing);

        // Verify consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestDefaultNamespaces()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root xmlns='http://agenix'>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<sub-element attribute='A'>text-value</sub-element>" +
                                         "</element>" +
                                         "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<root xmlns='http://agenix'>" +
                                                                          "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                                                          "<sub-element attribute='A'>text-value</sub-element>" +
                                                                          "</element>" +
                                                                          "</root>"));

        var validationContext = new XmlMessageValidationContext.Builder()
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Act & Assert - Should complete successfully with default namespace validation
        Assert.That(() => receiveAction.Execute(Context), Throws.Nothing);

        // Verify consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestDefaultNamespacesInExpectedMessage()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<ns1:root xmlns:ns1='http://agenix'>" +
                                         "<ns1:element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<ns1:sub-element attribute='A'>text-value</ns1:sub-element>" +
                                         "</ns1:element>" +
                                         "</ns1:root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<root xmlns='http://agenix'>" +
                                                                          "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                                                          "<sub-element attribute='A'>text-value</sub-element>" +
                                                                          "</element>" +
                                                                          "</root>"));

        var validationContext = new XmlMessageValidationContext.Builder()
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Act & Assert - Should complete successfully despite different namespace declaration approaches
        Assert.That(() => receiveAction.Execute(Context), Throws.Nothing);

        // Verify consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestDefaultNamespacesInSourceMessage()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root xmlns='http://agenix'>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<sub-element attribute='A'>text-value</sub-element>" +
                                         "</element>" +
                                         "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<ns1:root xmlns:ns1='http://agenix'>" +
                                                                          "<ns1:element attributeA='attribute-value' attributeB='attribute-value'>" +
                                                                          "<ns1:sub-element attribute='A'>text-value</ns1:sub-element>" +
                                                                          "</ns1:element>" +
                                                                          "</ns1:root>"));

        var validationContext = new XmlMessageValidationContext.Builder()
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Act & Assert - Should complete successfully despite different namespace declaration approaches
        Assert.That(() => receiveAction.Execute(Context), Throws.Nothing);

        // Verify consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestMissingNamespace()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<ns1:root xmlns:ns1='http://agenix'>" +
                                         "<ns1:element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<ns1:sub-element attribute='A'>text-value</ns1:sub-element>" +
                                         "</ns1:element>" +
                                         "</ns1:root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<root>" +
                                                                          "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                                                          "<sub-element attribute='A'>text-value</sub-element>" +
                                                                          "</element>" +
                                                                          "</root>"));

        var validationContext = new XmlMessageValidationContext.Builder()
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Act & Assert - Should throw ValidationException due to namespace mismatch
        Assert.That(() => receiveAction.Execute(Context), Throws.TypeOf<ValidationException>());

        // Verify consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestWrongNamespace()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<ns1:root xmlns:ns1='http://agenix'>" +
                                         "<ns1:element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<ns1:sub-element attribute='A'>text-value</ns1:sub-element>" +
                                         "</ns1:element>" +
                                         "</ns1:root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<ns1:root xmlns:ns1='http://agenix/wrong'>" +
                                                                          "<ns1:element attributeA='attribute-value' attributeB='attribute-value'>" +
                                                                          "<ns1:sub-element attribute='A'>text-value</ns1:sub-element>" +
                                                                          "</ns1:element>" +
                                                                          "</ns1:root>"));

        var validationContext = new XmlMessageValidationContext.Builder()
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Act & Assert - Should throw ValidationException due to namespace URI mismatch
        Assert.That(() => receiveAction.Execute(Context), Throws.TypeOf<ValidationException>());

        // Verify consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestExpectDefaultNamespace()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root xmlns='http://agenix'>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<sub-element attribute='A'>text-value</sub-element>" +
                                         "</element>" +
                                         "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<root xmlns='http://agenix'>" +
                                                                          "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                                                          "<sub-element attribute='A'>text-value</sub-element>" +
                                                                          "</element>" +
                                                                          "</root>"));

        var expectedNamespaces = new Dictionary<string, string> { { "", "http://agenix" } };

        var validationContext = new XmlMessageValidationContext.Builder()
            .Namespaces(expectedNamespaces)
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Act & Assert - Should succeed as both messages use the same default namespace
        Assert.That(() => receiveAction.Execute(Context), Throws.Nothing);

        // Verify the action completed successfully
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestExpectNamespace()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<ns1:root xmlns:ns1='http://agenix/ns1'>" +
                                         "<ns1:element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<ns1:sub-element attribute='A'>text-value</ns1:sub-element>" +
                                         "</ns1:element>" +
                                         "</ns1:root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<ns1:root xmlns:ns1='http://agenix/ns1'>" +
                                                                          "<ns1:element attributeA='attribute-value' attributeB='attribute-value'>" +
                                                                          "<ns1:sub-element attribute='A'>text-value</ns1:sub-element>" +
                                                                          "</ns1:element>" +
                                                                          "</ns1:root>"));

        var expectedNamespaces = new Dictionary<string, string> { { "ns1", "http://agenix/ns1" } };

        var validationContext = new XmlMessageValidationContext.Builder()
            .Namespaces(expectedNamespaces)
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Act & Assert - Should succeed as both messages use the same prefixed namespace
        Assert.That(() => receiveAction.Execute(Context), Throws.Nothing);

        // Verify the action completed successfully
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestExpectMixedNamespaces()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root xmlns='http://agenix/default' xmlns:ns1='http://agenix/ns1'>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<sub-element attribute='A'>text-value</sub-element>" +
                                         "</element>" +
                                         "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);


        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<root xmlns='http://agenix/default' xmlns:ns1='http://agenix/ns1'>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-element attribute='A'>text-value</sub-element>" +
            "</element>" +
            "</root>"));

        var expectedNamespaces = new Dictionary<string, string>
        {
            { "", "http://agenix/default" }, // Default namespace
            { "ns1", "http://agenix/ns1" } // Prefixed namespace
        };

        var validationContext = new XmlMessageValidationContext.Builder()
            .Namespaces(expectedNamespaces)
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Act & Assert - Should succeed as both messages declare the same mixed namespaces
        Assert.That(() => receiveAction.Execute(Context), Throws.Nothing);

        // Verify the action completed successfully
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestExpectMultipleNamespaces()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage(
            "<root xmlns='http://agenix/default' xmlns:ns1='http://agenix/ns1' xmlns:ns2='http://agenix/ns2'>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-element attribute='A'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<root xmlns='http://agenix/default' xmlns:ns1='http://agenix/ns1' xmlns:ns2='http://agenix/ns2'>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-element attribute='A'>text-value</sub-element>" +
            "</element>" +
            "</root>"));

        var expectedNamespaces = new Dictionary<string, string>
        {
            { "", "http://agenix/default" }, // Default namespace
            { "ns1", "http://agenix/ns1" }, // First prefixed namespace
            { "ns2", "http://agenix/ns2" } // Second prefixed namespace
        };

        var validationContext = new XmlMessageValidationContext.Builder()
            .Namespaces(expectedNamespaces)
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Act & Assert - Should succeed as both messages declare the same multiple namespaces
        Assert.That(() => receiveAction.Execute(Context), Throws.Nothing);

        // Verify the action completed successfully
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestExpectDefaultNamespaceError()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root xmlns='http://agenix'>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<sub-element attribute='A'>text-value</sub-element>" +
                                         "</element>" +
                                         "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<root xmlns='http://agenix'>" +
                                                                          "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                                                          "<sub-element attribute='A'>text-value</sub-element>" +
                                                                          "</element>" +
                                                                          "</root>"));

        var expectedNamespaces = new Dictionary<string, string>
        {
            { "", "http://agenix/wrong" } // Expected wrong default namespace
        };

        var validationContext = new XmlMessageValidationContext.Builder()
            .Namespaces(expectedNamespaces)
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Act & Assert - Should throw ValidationException due to namespace mismatch
        Assert.That(() => receiveAction.Execute(Context),
            Throws.TypeOf<ValidationException>()
                .With.Message
                .Contains(
                    "Namespace '' values not equal: found 'http://agenix' expected 'http://agenix/wrong' in reference node root"));

        // Verify the consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestExpectNamespaceError()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<ns1:root xmlns:ns1='http://agenix/ns1'>" +
                                         "<ns1:element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<ns1:sub-element attribute='A'>text-value</ns1:sub-element>" +
                                         "</ns1:element>" +
                                         "</ns1:root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<ns1:root xmlns:ns1='http://agenix/ns1'>" +
                                                                          "<ns1:element attributeA='attribute-value' attributeB='attribute-value'>" +
                                                                          "<ns1:sub-element attribute='A'>text-value</ns1:sub-element>" +
                                                                          "</ns1:element>" +
                                                                          "</ns1:root>"));

        var expectedNamespaces = new Dictionary<string, string>
        {
            { "ns1", "http://agenix/ns1/wrong" } // Expected wrong prefixed namespace
        };

        var validationContext = new XmlMessageValidationContext.Builder()
            .Namespaces(expectedNamespaces)
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Act & Assert - Should throw ValidationException due to prefixed namespace mismatch
        Assert.That(() => receiveAction.Execute(Context),
            Throws.TypeOf<ValidationException>()
                .With.Message
                .Contains(
                    "Namespace 'ns1' values not equal: found 'http://agenix/ns1' expected 'http://agenix/ns1/wrong' in reference node root"));

        // Verify the consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestExpectMixedNamespacesError()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root xmlns='http://agenix/default' xmlns:ns1='http://agenix/ns1'>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<sub-element attribute='A'>text-value</sub-element>" +
                                         "</element>" +
                                         "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<root xmlns='http://agenix/default' xmlns:ns1='http://agenix/ns1'>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-element attribute='A'>text-value</sub-element>" +
            "</element>" +
            "</root>"));

        var expectedNamespaces = new Dictionary<string, string>
        {
            { "", "http://agenix/default/wrong" }, // Wrong default namespace
            { "ns1", "http://agenix/ns1" } // Correct prefixed namespace
        };

        var validationContext = new XmlMessageValidationContext.Builder()
            .Namespaces(expectedNamespaces)
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Act & Assert - Should throw ValidationException due to default namespace mismatch
        Assert.That(() => receiveAction.Execute(Context),
            Throws.TypeOf<ValidationException>()
                .With.Message
                .Contains(
                    "Namespace '' values not equal: found 'http://agenix/default' expected 'http://agenix/default/wrong' in reference node root"));

        // Verify the consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestExpectMultipleNamespacesError()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage(
            "<root xmlns='http://agenix/default' xmlns:ns1='http://agenix/ns1' xmlns:ns2='http://agenix/ns2'>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-element attribute='A'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<root xmlns='http://agenix/default' xmlns:ns1='http://agenix/ns1' xmlns:ns2='http://agenix/ns2'>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-element attribute='A'>text-value</sub-element>" +
            "</element>" +
            "</root>"));

        var expectedNamespaces = new Dictionary<string, string>
        {
            { "", "http://agenix/default" }, // Correct default namespace
            { "ns1", "http://agenix/ns1/wrong" }, // Wrong prefixed namespace
            { "ns2", "http://agenix/ns2" } // Correct prefixed namespace
        };

        var validationContext = new XmlMessageValidationContext.Builder()
            .Namespaces(expectedNamespaces)
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Act & Assert - Should throw ValidationException due to ns1 namespace mismatch
        Assert.That(() => receiveAction.Execute(Context),
            Throws.TypeOf<ValidationException>()
                .With.Message
                .Contains(
                    "Namespace 'ns1' values not equal: found 'http://agenix/ns1' expected 'http://agenix/ns1/wrong' in reference node root"));

        // Verify the consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestExpectWrongNamespacePrefix()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage(
            "<root xmlns='http://agenix/default' xmlns:ns1='http://agenix/ns1' xmlns:ns2='http://agenix/ns2'>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-element attribute='A'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<root xmlns='http://agenix/default' xmlns:ns1='http://agenix/ns1' xmlns:ns2='http://agenix/ns2'>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-element attribute='A'>text-value</sub-element>" +
            "</element>" +
            "</root>"));

        var expectedNamespaces = new Dictionary<string, string>
        {
            { "", "http://agenix/default" }, // Correct default namespace
            { "nswrong", "http://agenix/ns1" }, // Wrong prefix for ns1 URI
            { "ns2", "http://agenix/ns2" } // Correct prefixed namespace
        };

        var validationContext = new XmlMessageValidationContext.Builder()
            .Namespaces(expectedNamespaces)
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Act & Assert - Should throw ValidationException due to wrong namespace prefix
        Assert.That(() => receiveAction.Execute(Context),
            Throws.TypeOf<ValidationException>()
                .With.Message.Contains("namespace"));

        // Verify the consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestExpectDefaultNamespaceButNamespace()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage(
            "<ns0:root xmlns:ns0='http://agenix/default' xmlns:ns1='http://agenix/ns1' xmlns:ns2='http://agenix/ns2'>" +
            "<ns0:element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<ns0:sub-element attribute='A'>text-value</ns0:sub-element>" +
            "</ns0:element>" +
            "</ns0:root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<ns0:root xmlns:ns0='http://agenix/default' xmlns:ns1='http://agenix/ns1' xmlns:ns2='http://agenix/ns2'>" +
            "<ns0:element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<ns0:sub-element attribute='A'>text-value</ns0:sub-element>" +
            "</ns0:element>" +
            "</ns0:root>"));

        var expectedNamespaces = new Dictionary<string, string>
        {
            { "", "http://agenix/default" }, // Expects default namespace
            { "ns1", "http://agenix/ns1" }, // Correct prefixed namespace
            { "ns2", "http://agenix/ns2" } // Correct prefixed namespace
        };

        var validationContext = new XmlMessageValidationContext.Builder()
            .Namespaces(expectedNamespaces)
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Act & Assert - Should throw ValidationException due to default namespace expectation vs prefixed declaration
        Assert.That(() => receiveAction.Execute(Context),
            Throws.TypeOf<ValidationException>()
                .With.Message.Contains("namespace"));

        // Verify the consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestExpectNamespaceButDefaultNamespace()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage(
            "<root xmlns='http://agenix/default' xmlns:ns1='http://agenix/ns1' xmlns:ns2='http://agenix/ns2'>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-element attribute='A'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<root xmlns='http://agenix/default' xmlns:ns1='http://agenix/ns1' xmlns:ns2='http://agenix/ns2'>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-element attribute='A'>text-value</sub-element>" +
            "</element>" +
            "</root>"));

        var expectedNamespaces = new Dictionary<string, string>
        {
            { "ns0", "http://agenix/default" }, // Expects prefixed namespace for default URI
            { "ns1", "http://agenix/ns1" }, // Correct prefixed namespace
            { "ns2", "http://agenix/ns2" } // Correct prefixed namespace
        };

        var validationContext = new XmlMessageValidationContext.Builder()
            .Namespaces(expectedNamespaces)
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Act & Assert - Should throw ValidationException due to prefixed namespace expectation vs default declaration
        Assert.That(() => receiveAction.Execute(Context),
            Throws.TypeOf<ValidationException>()
                .With.Message.Contains("namespace"));

        // Verify the consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestExpectAdditionalNamespace()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage(
            "<root xmlns='http://agenix/default' xmlns:ns1='http://agenix/ns1' xmlns:ns2='http://agenix/ns2'>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-element attribute='A'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<root xmlns='http://agenix/default' xmlns:ns1='http://agenix/ns1' xmlns:ns2='http://agenix/ns2'>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-element attribute='A'>text-value</sub-element>" +
            "</element>" +
            "</root>"));

        var expectedNamespaces = new Dictionary<string, string>
        {
            { "", "http://agenix/default" }, // Present in message as default namespace
            { "ns1", "http://agenix/ns1" }, // Present in message with correct prefix
            { "ns2", "http://agenix/ns2" }, // Present in message with correct prefix
            { "ns4", "http://agenix/ns4" } // MISSING - not declared in message
        };

        var validationContext = new XmlMessageValidationContext.Builder()
            .Namespaces(expectedNamespaces)
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Act & Assert - Should throw ValidationException due to missing namespace
        Assert.That(() => receiveAction.Execute(Context),
            Throws.TypeOf<ValidationException>()
                .With.Message.Contains("namespace"));

        // Verify the consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestExpectNamespaceButNamespaceMissing()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage(
            "<root xmlns='http://agenix/default' xmlns:ns1='http://agenix/ns1' xmlns:ns2='http://agenix/ns2' xmlns:ns4='http://agenix/ns4'>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-element attribute='A'>text-value</sub-element>" +
            "</element>" +
            "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<root xmlns='http://agenix/default' xmlns:ns1='http://agenix/ns1' xmlns:ns2='http://agenix/ns2' xmlns:ns4='http://agenix/ns4'>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-element attribute='A'>text-value</sub-element>" +
            "</element>" +
            "</root>"));

        var expectedNamespaces = new Dictionary<string, string>
        {
            { "", "http://agenix/default" }, // Present in message as default namespace
            { "ns1", "http://agenix/ns1" }, // Present in message with correct prefix
            { "ns2", "http://agenix/ns2" } // Present in message with correct prefix
            // Missing from expectations: ns4 -> http://agenix/ns4 (present in message but not expected)
        };

        var validationContext = new XmlMessageValidationContext.Builder()
            .Namespaces(expectedNamespaces)
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Act & Assert - Should throw ValidationException due to unexpected namespace
        Assert.That(() => receiveAction.Execute(Context),
            Throws.TypeOf<ValidationException>()
                .With.Message.Contains("namespace"));

        // Verify the consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestValidateMessageElementsWithAdditionalNamespacePrefix()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root xmlns='http://agenix/default'>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                         "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                         "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                         "</element>" +
                                         "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var validateMessageElements = new Dictionary<string, object>
        {
            { "//ns1:root/ns1:element/ns1:sub-elementA", "text-value" }, { "//ns1:sub-elementB", "text-value" }
        };

        var controlMessageBuilder = new DefaultMessageBuilder();

        var namespaces = new Dictionary<string, string> { { "ns1", "http://agenix/default" } };

        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .NamespaceContext(namespaces)
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Act & Assert - Should pass validation
        Assert.That(() => receiveAction.Execute(Context), Throws.Nothing);

        // Verify the consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestValidateMessageElementsWithDifferentNamespacePrefix()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<ns1:root xmlns:ns1='http://agenix/default'>" +
                                         "<ns1:element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<ns1:sub-elementA attribute='A'>text-value</ns1:sub-elementA>" +
                                         "<ns1:sub-elementB attribute='B'>text-value</ns1:sub-elementB>" +
                                         "<ns1:sub-elementC attribute='C'>text-value</ns1:sub-elementC>" +
                                         "</ns1:element>" +
                                         "</ns1:root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var validateMessageElements = new Dictionary<string, object>
        {
            { "//pfx:root/pfx:element/pfx:sub-elementA", "text-value" }, { "//pfx:sub-elementB", "text-value" }
        };

        var controlMessageBuilder = new DefaultMessageBuilder();

        var namespaces = new Dictionary<string, string> { { "pfx", "http://agenix/default" } };

        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .NamespaceContext(namespaces)
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Act & Assert - Should pass validation
        Assert.That(() => receiveAction.Execute(Context), Throws.Nothing);

        // Verify the consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }

    [Test]
    public void TestWrongNamespaceContext()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<ns1:root xmlns:ns1='http://agenix/default'>" +
                                         "<ns1:element attributeA='attribute-value' attributeB='attribute-value'>" +
                                         "<ns1:sub-elementA attribute='A'>text-value</ns1:sub-elementA>" +
                                         "<ns1:sub-elementB attribute='B'>text-value</ns1:sub-elementB>" +
                                         "<ns1:sub-elementC attribute='C'>text-value</ns1:sub-elementC>" +
                                         "</ns1:element>" +
                                         "</ns1:root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var validateMessageElements = new Dictionary<string, object>
        {
            { "//pfx:root/ns1:element/pfx:sub-elementA", "text-value" }, { "//pfx:sub-elementB", "text-value" }
        };

        var controlMessageBuilder = new DefaultMessageBuilder();

        // Wrong namespace URI mapping - should cause validation failure
        var namespaces = new Dictionary<string, string> { { "pfx", "http://agenix/wrong" } };

        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .NamespaceContext(namespaces)
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Act & Assert - Should throw AgenixSystemException due to wrong namespace mapping
        Assert.That(() => receiveAction.Execute(Context),
            Throws.TypeOf<AgenixSystemException>());

        // Verify the consumer was called
        _consumer.Verify(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()), Times.Once);
    }
}
