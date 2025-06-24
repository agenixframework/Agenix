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
using Agenix.Core.Message.Builder;
using Agenix.Core.Validation.Builder;
using Agenix.Core.Validation.Xml;
using Agenix.Core.Variable;
using Agenix.Validation.Xml.Validation.Xml;
using Moq;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Xml.Tests.Validation.Xml;

public class ReceiveMessageActionTest : AbstractNUnitSetUp
{
    private readonly Mock<ISelectiveConsumer> _consumer = new();
    private readonly Mock<IEndpoint> _endpoint = new();
    private readonly Mock<IEndpointConfiguration> _endpointConfiguration = new();
    private readonly Mock<IMessageQueue> _mockQueue = new();

    [SetUp]
    public void SetUpTest()
    {
        Context.ReferenceResolver.Bind("mockQueue", _mockQueue.Object);
    }

    [Test]
    public void TestReceiveMessageWithEndpointUri()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(
            new DefaultPayloadBuilder("<TestRequest><Message>Hello World!</Message></TestRequest>"));

        var controlMessage = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        _mockQueue.Setup(q => q.Receive(15000)).Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint("direct:mockQueue?timeout=15000")
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestReceiveMessageWithVariableEndpointName()
    {
        Context.SetVariable("varEndpoint", "direct:mockQueue");

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(
            new DefaultPayloadBuilder("<TestRequest><Message>Hello World!</Message></TestRequest>"));

        var controlMessage = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        _mockQueue.Setup(q => q.Receive(5000)).Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint("${varEndpoint}")
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestReceiveMessageWithMessagePayloadData()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(
            new DefaultPayloadBuilder("<TestRequest><Message>Hello World!</Message></TestRequest>"));

        var controlMessage = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestReceiveMessageWithMessagePayloadResource()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(
            new FileResourcePayloadBuilder(
                "assembly://Agenix.Validation.Xml.Tests/Agenix.Validation.Xml.Tests.Resources.Actions/test-request-payload.xml"));

        var controlMessage =
            new DefaultMessage(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><TestRequest><Message>Hello World!</Message></TestRequest>");

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestReceiveMessageWithMessagePayloadDataVariablesSupport()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(
            new DefaultPayloadBuilder("<TestRequest><Message>${myText}</Message></TestRequest>"));

        Context.SetVariable("myText", "Hello World!");

        var controlMessage = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestReceiveMessageWithMessagePayloadResourceVariablesSupport()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(
            new FileResourcePayloadBuilder(
                "assembly://Agenix.Validation.Xml.Tests/Agenix.Validation.Xml.Tests.Resources.Actions/test-request-payload-with-variables.xml"));

        Context.SetVariable("myText", "Hello World!");

        var controlMessage =
            new DefaultMessage(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><TestRequest><Message>Hello World!</Message></TestRequest>");

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestReceiveMessageWithMessagePayloadResourceFunctionsSupport()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(
            new FileResourcePayloadBuilder(
                "assembly://Agenix.Validation.Xml.Tests/Agenix.Validation.Xml.Tests.Resources.Actions/test-request-payload-with-functions.xml"));

        var controlMessage =
            new DefaultMessage(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><TestRequest><Message>Hello World!</Message></TestRequest>");

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestReceiveMessageOverwriteMessageElementsXPath()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<?xml version=\"1.0\" encoding=\"utf-8\"?><TestRequest><Message>?</Message></TestRequest>"));

        var overwriteElements = new Dictionary<string, object> { { "/TestRequest/Message", "Hello World!" } };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(overwriteElements)
            .Build();

        var controlMessage =
            new DefaultMessage(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><TestRequest><Message>Hello World!</Message></TestRequest>");

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.XML)
            .Process(processor)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestReceiveMessageOverwriteMessageElementsDotNotation()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<?xml version=\"1.0\" encoding=\"utf-8\"?><TestRequest><Message>?</Message></TestRequest>"));

        var overwriteElements = new Dictionary<string, object> { { "TestRequest.Message", "Hello World!" } };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(overwriteElements)
            .Build();

        var controlMessage =
            new DefaultMessage(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><TestRequest><Message>Hello World!</Message></TestRequest>");

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.XML)
            .Process(processor)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestReceiveMessageOverwriteMessageElementsXPathWithNamespaces()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<?xml version=\"1.0\" encoding=\"utf-8\"?><ns0:TestRequest xmlns:ns0=\"http://agenix.org/unittest\">" +
            "<ns0:Message>?</ns0:Message></ns0:TestRequest>"));

        var overwriteElements = new Dictionary<string, object> { { "/ns0:TestRequest/ns0:Message", "Hello World!" } };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(overwriteElements)
            .Build();

        var controlMessage = new DefaultMessage(
            "<?xml version=\"1.0\" encoding=\"utf-8\"?><ns0:TestRequest xmlns:ns0=\"http://agenix.org/unittest\">" +
            "<ns0:Message>Hello World!</ns0:Message></ns0:TestRequest>");

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var validationContext = new XmlMessageValidationContext.Builder()
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.XML)
            .Validate(validationContext)
            .Process(processor)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestReceiveMessageOverwriteMessageElementsXPathWithNestedNamespaces()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<?xml version=\"1.0\" encoding=\"utf-8\"?><ns0:TestRequest xmlns:ns0=\"http://agenix.org/unittest\">" +
            "<ns1:Message xmlns:ns1=\"http://agenix.org/unittest/message\">?</ns1:Message></ns0:TestRequest>"));

        var overwriteElements = new Dictionary<string, object> { { "/ns0:TestRequest/ns1:Message", "Hello World!" } };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(overwriteElements)
            .Build();

        var controlMessage = new DefaultMessage(
            "<?xml version=\"1.0\" encoding=\"utf-8\"?><ns0:TestRequest xmlns:ns0=\"http://agenix.org/unittest\">" +
            "<ns1:Message xmlns:ns1=\"http://agenix.org/unittest/message\">Hello World!</ns1:Message></ns0:TestRequest>");

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var validationContext = new XmlMessageValidationContext.Builder()
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.XML)
            .Validate(validationContext)
            .Process(processor)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestReceiveMessageOverwriteMessageElementsXPathWithDefaultNamespaces()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<?xml version=\"1.0\" encoding=\"utf-8\"?><TestRequest xmlns=\"http://agenix.org/unittest\">" +
            "<Message>?</Message></TestRequest>"));

        var overwriteElements = new Dictionary<string, object>
        {
            { "//*[local-name()='TestRequest']/*[local-name()='Message']", "Hello World!" }
        };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(overwriteElements)
            .Build();

        var controlMessage =
            new DefaultMessage(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><TestRequest xmlns=\"http://agenix.org/unittest\"><Message>Hello World!</Message></TestRequest>");

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var validationContext = new XmlMessageValidationContext.Builder()
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.XML)
            .Validate(validationContext)
            .Process(processor)
            .Build();

        receiveAction.Execute(Context);
    }


    [Test]
    public void TestReceiveMessageWithMessageHeaders()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<?xml version=\"1.0\" encoding=\"utf-8\"?><TestRequest><Message>Hello World!</Message></TestRequest>"));

        var headers = new Dictionary<string, object> { { "Operation", "sayHello" } };
        controlMessageBuilder.AddHeaderBuilder(new DefaultHeaderBuilder(headers));

        var controlHeaders = new Dictionary<string, object> { { "Operation", "sayHello" } };
        var controlMessage =
            new DefaultMessage(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><TestRequest><Message>Hello World!</Message></TestRequest>",
                controlHeaders);

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.XML)
            .Build();

        receiveAction.Execute(Context);
    }


    [Test]
    public void TestReceiveMessageWithMessageHeadersVariablesSupport()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<?xml version=\"1.0\" encoding=\"utf-8\"?><TestRequest><Message>Hello World!</Message></TestRequest>"));

        Context.SetVariable("myOperation", "sayHello");

        var headers = new Dictionary<string, object> { { "Operation", "${myOperation}" } };
        controlMessageBuilder.AddHeaderBuilder(new DefaultHeaderBuilder(headers));

        var controlHeaders = new Dictionary<string, object> { { "Operation", "sayHello" } };
        var controlMessage =
            new DefaultMessage(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><TestRequest><Message>Hello World!</Message></TestRequest>",
                controlHeaders);

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.XML)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestReceiveMessageWithUnknownVariablesInMessageHeaders()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<?xml version=\"1.0\" encoding=\"utf-8\"?><TestRequest><Message>Hello World!</Message></TestRequest>"));

        var headers = new Dictionary<string, object> { { "Operation", "${myOperation}" } };
        controlMessageBuilder.AddHeaderBuilder(new DefaultHeaderBuilder(headers));

        var controlHeaders = new Dictionary<string, object> { { "Operation", "sayHello" } };
        var controlMessage =
            new DefaultMessage(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><TestRequest><Message>Hello World!</Message></TestRequest>",
                controlHeaders);

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.XML)
            .Build();

        var exception = Assert.Throws<AgenixSystemException>(() => receiveAction.Execute(Context));
        Assert.That("Unknown variable 'myOperation'", Is.EqualTo(exception.InnerException.Message));
    }

    [Test]
    public void TestReceiveMessageWithUnknownVariableInMessagePayload()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(
            new DefaultPayloadBuilder("<TestRequest><Message>${myText}</Message></TestRequest>"));

        var controlMessage = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        var exception = Assert.Throws<AgenixSystemException>(() => receiveAction.Execute(Context));
        Assert.That("Unknown variable 'myText'", Is.EqualTo(exception.Message));
    }

    [Test]
    public void TestReceiveMessageWithExtractVariablesFromHeaders()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(
            new DefaultPayloadBuilder("<TestRequest><Message>Hello World!</Message></TestRequest>"));

        var headers = new Dictionary<string, string> { { "Operation", "myOperation" } };

        var headerVariableExtractor = new MessageHeaderVariableExtractor.Builder()
            .Headers(headers)
            .Build();

        var controlHeaders = new Dictionary<string, object> { { "Operation", "sayHello" } };
        var controlMessage =
            new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>", controlHeaders);

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Process(headerVariableExtractor)
            .Build();

        receiveAction.Execute(Context);

        Assert.That(Context.GetVariable("myOperation"), Is.Not.Null);
        Assert.That("sayHello", Is.EqualTo(Context.GetVariable("myOperation")));
    }

    [Test]
    public void TestReceiveMessageWithValidateMessageElementsFromMessageXPath()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();

        var messageElements = new Dictionary<string, object> { { "/TestRequest/Message", "Hello World!" } };
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(messageElements)
            .Build();

        var controlMessage = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestReceiveMessageWithValidateMessageElementsXPathNamespaceSupport()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();

        var messageElements = new Dictionary<string, object> { { "/ns0:TestRequest/ns0:Message", "Hello World!" } };

        var controlMessage = new DefaultMessage("<ns0:TestRequest xmlns:ns0=\"http://agenix.org/unittest\">" +
                                                "<ns0:Message>Hello World!</ns0:Message></ns0:TestRequest>");

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(messageElements)
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestReceiveMessageWithValidateMessageElementsXPathNestedNamespaceSupport()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();

        var messageElements = new Dictionary<string, object> { { "/ns0:TestRequest/ns1:Message", "Hello World!" } };

        var controlMessage = new DefaultMessage("<ns0:TestRequest xmlns:ns0=\"http://agenix.org/unittest\">" +
                                                "<ns1:Message xmlns:ns1=\"http://agenix.org/unittest/message\">Hello World!</ns1:Message></ns0:TestRequest>");

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(messageElements)
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestReceiveMessageWithValidateMessageElementsXPathNamespaceBindings()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();

        var messageElements = new Dictionary<string, object> { { "/pfx:TestRequest/pfx:Message", "Hello World!" } };

        var namespaces = new Dictionary<string, string> { { "pfx", "http://agenix.org/unittest" } };

        var controlMessage = new DefaultMessage("<ns0:TestRequest xmlns:ns0=\"http://agenix.org/unittest\">" +
                                                "<ns0:Message>Hello World!</ns0:Message></ns0:TestRequest>");

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var validationContext = new XpathMessageValidationContext.Builder()
            .NamespaceContext(namespaces)
            .Expressions(messageElements)
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestReceiveMessageWithExtractVariablesFromMessageXPath()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(
            new DefaultPayloadBuilder("<TestRequest><Message>Hello World!</Message></TestRequest>"));

        var extractMessageElements = new Dictionary<string, object> { { "/TestRequest/Message", "messageVar" } };

        var variableExtractor = new XpathPayloadVariableExtractor.Builder()
            .Expressions(extractMessageElements)
            .Build();

        var controlMessage = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Process(variableExtractor)
            .Build();

        receiveAction.Execute(Context);

        Assert.That(Context.GetVariable("messageVar"), Is.Not.Null);
        Assert.That(Context.GetVariable("messageVar"), Is.EqualTo("Hello World!"));
    }

    [Test]
    public void TestReceiveMessageWithExtractVariablesFromMessageXPathNodeList()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<TestRequest>" +
                                                                          "<Message>Hello</Message>" +
                                                                          "<Message>ByeBye</Message>" +
                                                                          "</TestRequest>"));

        var extractMessageElements = new Dictionary<string, object>
        {
            { "node-set://TestRequest/Message", "messageVar" }
        };

        var variableExtractor = new XpathPayloadVariableExtractor.Builder()
            .Expressions(extractMessageElements)
            .Build();

        var controlMessage = new DefaultMessage("<TestRequest>" +
                                                "<Message>Hello</Message>" +
                                                "<Message>ByeBye</Message>" +
                                                "</TestRequest>");

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Process(variableExtractor)
            .Build();

        receiveAction.Execute(Context);

        Assert.That(Context.GetVariable("messageVar"), Is.Not.Null);
        Assert.That(Context.GetVariable("messageVar"), Is.EqualTo("Hello,ByeBye"));
    }

    [Test]
    public void TestReceiveMessageWithExtractVariablesFromMessageXPathNamespaceSupport()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<TestRequest xmlns=\"http://agenix.org/unittest\">" +
            "<Message>Hello World!</Message></TestRequest>"));

        var extractMessageElements = new Dictionary<string, object>
        {
            { "/ns0:TestRequest/ns0:Message", "messageVar" }
        };

        var variableExtractor = new XpathPayloadVariableExtractor.Builder()
            .Expressions(extractMessageElements)
            .Build();

        var controlMessage = new DefaultMessage("<ns0:TestRequest xmlns:ns0=\"http://agenix.org/unittest\">" +
                                                "<ns0:Message>Hello World!</ns0:Message></ns0:TestRequest>");

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var validationContext = new XmlMessageValidationContext.Builder()
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Process(variableExtractor)
            .Validate(validationContext)
            .Build();

        receiveAction.Execute(Context);

        Assert.That(Context.GetVariable("messageVar"), Is.Not.Null);
        Assert.That(Context.GetVariable("messageVar"), Is.EqualTo("Hello World!"));
    }

    [Test]
    public void TestReceiveMessageWithExtractVariablesFromMessageXPathNestedNamespaceSupport()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<TestRequest xmlns=\"http://agenix.org/unittest\" xmlns:ns1=\"http://agenix.org/unittest/message\">" +
            "<ns1:Message>Hello World!</ns1:Message></TestRequest>"));

        var extractMessageElements = new Dictionary<string, object>
        {
            { "/ns0:TestRequest/ns1:Message", "messageVar" }
        };

        var variableExtractor = new XpathPayloadVariableExtractor.Builder()
            .Expressions(extractMessageElements)
            .Build();

        var controlMessage = new DefaultMessage("<ns0:TestRequest xmlns:ns0=\"http://agenix.org/unittest\">" +
                                                "<ns1:Message xmlns:ns1=\"http://agenix.org/unittest/message\">Hello World!</ns1:Message></ns0:TestRequest>");

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var validationContext = new XmlMessageValidationContext.Builder()
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Process(variableExtractor)
            .Validate(validationContext)
            .Build();

        receiveAction.Execute(Context);

        Assert.That(Context.GetVariable("messageVar"), Is.Not.Null);
        Assert.That(Context.GetVariable("messageVar"), Is.EqualTo("Hello World!"));
    }

    [Test]
    public void TestReceiveMessageWithExtractVariablesFromMessageXPathNamespaceBindings()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<TestRequest xmlns=\"http://agenix.org/unittest\">" +
            "<Message>Hello World!</Message></TestRequest>"));

        var extractMessageElements = new Dictionary<string, object>
        {
            { "/pfx:TestRequest/pfx:Message", "messageVar" }
        };

        var namespaces = new Dictionary<string, string> { { "pfx", "http://agenix.org/unittest" } };

        var variableExtractor = new XpathPayloadVariableExtractor.Builder()
            .Expressions(extractMessageElements)
            .Namespaces(namespaces)
            .Build();

        var controlMessage = new DefaultMessage("<ns0:TestRequest xmlns:ns0=\"http://agenix.org/unittest\">" +
                                                "<ns0:Message>Hello World!</ns0:Message></ns0:TestRequest>");

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var validationContext = new XmlMessageValidationContext.Builder()
            .SchemaValidation(false)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Process(variableExtractor)
            .Validate(validationContext)
            .Build();

        receiveAction.Execute(Context);

        Assert.That(Context.GetVariable("messageVar"), Is.Not.Null);
        Assert.That(Context.GetVariable("messageVar"), Is.EqualTo("Hello World!"));
    }

    [Test]
    public void TestReceiveMessageWithTimeout()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(
            new DefaultPayloadBuilder("<TestRequest><Message>Hello World!</Message></TestRequest>"));

        var controlMessage = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(Context, 3000L))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Timeout(3000L)
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestReceiveSelectedWithMessageSelector()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(
            new DefaultPayloadBuilder("<TestRequest><Message>Hello World!</Message></TestRequest>"));

        var messageSelector = "Operation = 'sayHello'";

        var headers = new Dictionary<string, object> { { "Operation", "sayHello" } };
        var controlMessage = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>", headers);

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(messageSelector, Context, 5000L))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Selector(messageSelector)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestReceiveSelectedWithMessageSelectorAndTimeout()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(
            new DefaultPayloadBuilder("<TestRequest><Message>Hello World!</Message></TestRequest>"));

        var messageSelector = "Operation = 'sayHello'";

        var headers = new Dictionary<string, object> { { "Operation", "sayHello" } };
        var controlMessage = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>", headers);

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(messageSelector, Context, 5000L))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Timeout(5000L)
            .Selector(messageSelector)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestReceiveSelectedWithMessageSelectorMap()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(
            new DefaultPayloadBuilder("<TestRequest><Message>Hello World!</Message></TestRequest>"));

        IDictionary<string, object> messageSelector = new Dictionary<string, object> { { "Operation", "sayHello" } };

        var headers = new Dictionary<string, object> { { "Operation", "sayHello" } };
        var controlMessage = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>", headers);

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive("Operation = 'sayHello'", Context, 5000L))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Selector(messageSelector)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestReceiveSelectedWithMessageSelectorMapAndTimeout()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(
            new DefaultPayloadBuilder("<TestRequest><Message>Hello World!</Message></TestRequest>"));

        IDictionary<string, object> messageSelector = new Dictionary<string, object> { { "Operation", "sayHello" } };

        var headers = new Dictionary<string, object> { { "Operation", "sayHello" } };
        var controlMessage = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>", headers);

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive("Operation = 'sayHello'", Context, 5000L))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Timeout(5000L)
            .Message(controlMessageBuilder)
            .Selector(messageSelector)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestMessageTimeout()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(
            new DefaultPayloadBuilder("<TestRequest><Message>Hello World!</Message></TestRequest>"));

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns((IMessage)null!);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        var exception = Assert.Throws<AgenixSystemException>(() => receiveAction.Execute(Context));
        Assert.That(exception.Message, Is.EqualTo("Failed to receive message - message is not available"));
    }

    [Test]
    public void TestReceiveEmptyMessagePayloadAsExpected()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();

        var controlMessage = new DefaultMessage("");

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestReceiveEmptyMessagePayloadUnexpected()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(
            new DefaultPayloadBuilder("<TestRequest><Message>Hello World!</Message></TestRequest>"));

        var controlMessage = new DefaultMessage("");

        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        var exception = Assert.Throws<ValidationException>(() => receiveAction.Execute(Context));
        Assert.That(exception.Message, Is.EqualTo("Empty message validation failed - control message is not empty!"));
    }
}
