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
using Agenix.Validation.Xml.Validation.Xml;
using Moq;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Xml.Tests;

public class MessageElementsTest : AbstractNUnitSetUp
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
    public void TestValidateMessageElements()
    {
        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>"
                                         + "<element attributeA='attribute-value' attributeB='attribute-value' >"
                                         + "<sub-elementA attribute='A'>text-value</sub-elementA>"
                                         + "<sub-elementB attribute='B'>text-value</sub-elementB>"
                                         + "<sub-elementC attribute='C'>text-value</sub-elementC>"
                                         + "</element>"
                                         + "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var validateMessageElements = new Dictionary<string, object>
        {
            ["//root/element/sub-elementA"] = "text-value",
            ["//sub-elementB"] = "text-value"
        };

        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Assert that no exception is thrown
        Assert.DoesNotThrow(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestValidateEmptyMessageElements()
    {
        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>"
                                         + "<element attributeA='attribute-value' attributeB='attribute-value' >"
                                         + "<sub-elementA attribute='A'></sub-elementA>"
                                         + "<sub-elementB attribute='B'></sub-elementB>"
                                         + "<sub-elementC attribute='C'>text-value</sub-elementC>"
                                         + "</element>"
                                         + "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var validateMessageElements = new Dictionary<string, object>
        {
            ["//root/element/sub-elementA"] = "",
            ["//sub-elementB"] = ""
        };

        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Assert that no exception is thrown
        Assert.DoesNotThrow(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestValidateEmptyMessageAttributes()
    {
        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>"
                                         + "<element attributeA='attribute-value' attributeB='attribute-value' >"
                                         + "<sub-elementA attribute=''>text-value</sub-elementA>"
                                         + "<sub-elementB attribute=''>text-value</sub-elementB>"
                                         + "<sub-elementC attribute=''>text-value</sub-elementC>"
                                         + "</element>"
                                         + "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var validateMessageElements = new Dictionary<string, object>
        {
            ["//root/element/sub-elementA/@attribute"] = "",
            ["//root/element/sub-elementB/@attribute"] = "",
            ["//root/element/sub-elementC/@attribute"] = ""
        };

        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Assert that no exception is thrown
        Assert.DoesNotThrow(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestValidateNullElements()
    {
        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>"
                                         + "<element attributeA='attribute-value' attributeB='attribute-value' >"
                                         + "<sub-elementA attribute='A'></sub-elementA>"
                                         + "<sub-elementB attribute='B'></sub-elementB>"
                                         + "<sub-elementC attribute='C'>text-value</sub-elementC>"
                                         + "</element>"
                                         + "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var validateMessageElements = new Dictionary<string, object>
        {
            ["//root/element/sub-elementA"] = "null",
            ["//sub-elementB"] = "null"
        };

        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Assert that ValidationException is thrown
        Assert.Throws<ValidationException>(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestValidateMessageElementAttributes()
    {
        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>"
                                         + "<element attributeA='attribute-value' attributeB='attribute-value' >"
                                         + "<sub-elementA attribute='A'>text-value</sub-elementA>"
                                         + "<sub-elementB attribute='B'>text-value</sub-elementB>"
                                         + "<sub-elementC attribute='C'>text-value</sub-elementC>"
                                         + "</element>"
                                         + "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var validateMessageElements = new Dictionary<string, object>
        {
            ["//root/element/sub-elementA/@attribute"] = "A",
            ["//sub-elementB/@attribute"] = "B"
        };

        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Assert that no exception is thrown
        Assert.DoesNotThrow(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestValidateMessageElementsWrongExpectedElement()
    {
        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>"
                                         + "<element attributeA='attribute-value' attributeB='attribute-value' >"
                                         + "<sub-elementA attribute='A'>text-value</sub-elementA>"
                                         + "<sub-elementB attribute='B'>text-value</sub-elementB>"
                                         + "<sub-elementC attribute='C'>text-value</sub-elementC>"
                                         + "</element>"
                                         + "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var validateMessageElements = new Dictionary<string, object>
        {
            ["//root/element/sub-element-wrong"] = "text-value",
            ["//sub-element-wrong"] = "text-value"
        };

        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Assert that AgenixSystemException is thrown
        Assert.Throws<AgenixSystemException>(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestValidateMessageElementsWrongExpectedValue()
    {
        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>"
                                         + "<element attributeA='attribute-value' attributeB='attribute-value' >"
                                         + "<sub-elementA attribute='A'>text-value</sub-elementA>"
                                         + "<sub-elementB attribute='B'>text-value</sub-elementB>"
                                         + "<sub-elementC attribute='C'>text-value</sub-elementC>"
                                         + "</element>"
                                         + "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var validateMessageElements = new Dictionary<string, object>
        {
            ["//root/element/sub-elementA"] = "text-value-wrong",
            ["//sub-elementB"] = "text-value-wrong"
        };

        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Assert that ValidationException is thrown
        Assert.Throws<ValidationException>(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestValidateMessageElementAttributesWrongExpectedValue()
    {
        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>"
                                         + "<element attributeA='attribute-value' attributeB='attribute-value' >"
                                         + "<sub-elementA attribute='A'>text-value</sub-elementA>"
                                         + "<sub-elementB attribute='B'>text-value</sub-elementB>"
                                         + "<sub-elementC attribute='C'>text-value</sub-elementC>"
                                         + "</element>"
                                         + "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var validateMessageElements = new Dictionary<string, object>
        {
            ["//root/element/sub-elementA/@attribute"] = "wrong-value",
            ["//sub-elementB/@attribute"] = "wrong-value"
        };

        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Assert that ValidationException is thrown
        Assert.Throws<ValidationException>(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestValidateMessageElementAttributesWrongExpectedAttribute()
    {
        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>"
                                         + "<element attributeA='attribute-value' attributeB='attribute-value' >"
                                         + "<sub-elementA attribute='A'>text-value</sub-elementA>"
                                         + "<sub-elementB attribute='B'>text-value</sub-elementB>"
                                         + "<sub-elementC attribute='C'>text-value</sub-elementC>"
                                         + "</element>"
                                         + "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var validateMessageElements = new Dictionary<string, object>
        {
            ["//root/element/sub-elementA/@attribute-wrong"] = "A",
            ["//sub-elementB/@attribute-wrong"] = "B"
        };

        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Assert that AgenixSystemException is thrown
        Assert.Throws<AgenixSystemException>(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestSetMessageElements()
    {
        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>"
                                         + "<element attributeA='attribute-value' attributeB='attribute-value' >"
                                         + "<sub-elementA attribute='A'>text-value</sub-elementA>"
                                         + "<sub-elementB attribute='B'>text-value</sub-elementB>"
                                         + "<sub-elementC attribute='C'>text-value</sub-elementC>"
                                         + "</element>"
                                         + "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>"
            + "<element attributeA='attribute-value' attributeB='attribute-value' >"
            + "<sub-elementA attribute='A'>to be overwritten</sub-elementA>"
            + "<sub-elementB attribute='B'>to be overwritten</sub-elementB>"
            + "<sub-elementC attribute='C'>text-value</sub-elementC>"
            + "</element>"
            + "</root>"));

        var messageElements = new Dictionary<string, object>
        {
            ["//root/element/sub-elementA"] = "text-value",
            ["//sub-elementB"] = "text-value"
        };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(messageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.XML)
            .Process(processor)
            .Build();

        // Assert that no exception is thrown
        Assert.DoesNotThrow(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestSetMessageElementsUsingEmptyString()
    {
        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>"
                                         + "<element attributeA='attribute-value' attributeB='attribute-value' >"
                                         + "<sub-elementA attribute='A'></sub-elementA>"
                                         + "<sub-elementB attribute='B'></sub-elementB>"
                                         + "<sub-elementC attribute='C'>text-value</sub-elementC>"
                                         + "</element>"
                                         + "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>"
            + "<element attributeA='attribute-value' attributeB='attribute-value' >"
            + "<sub-elementA attribute='A'>to be overwritten</sub-elementA>"
            + "<sub-elementB attribute='B'>to be overwritten</sub-elementB>"
            + "<sub-elementC attribute='C'>text-value</sub-elementC>"
            + "</element>"
            + "</root>"));

        var messageElements = new Dictionary<string, object>
        {
            ["//root/element/sub-elementA"] = "",
            ["//sub-elementB"] = ""
        };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(messageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Process(processor)
            .Build();

        // Assert that no exception is thrown
        Assert.DoesNotThrow(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestSetMessageElementsAndValidate()
    {
        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>"
                                         + "<element attributeA='attribute-value' attributeB='attribute-value' >"
                                         + "<sub-elementA attribute='A'>text-value</sub-elementA>"
                                         + "<sub-elementB attribute='B'>text-value</sub-elementB>"
                                         + "<sub-elementC attribute='C'>text-value</sub-elementC>"
                                         + "</element>"
                                         + "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>"
            + "<element attributeA='attribute-value' attributeB='attribute-value' >"
            + "<sub-elementA attribute='A'>to be overwritten</sub-elementA>"
            + "<sub-elementB attribute='B'>to be overwritten</sub-elementB>"
            + "<sub-elementC attribute='C'>text-value</sub-elementC>"
            + "</element>"
            + "</root>"));

        var messageElements = new Dictionary<string, object>
        {
            ["//root/element/sub-elementA"] = "text-value",
            ["//sub-elementB"] = "text-value"
        };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(messageElements)
            .Build();

        var validateElements = new Dictionary<string, object>
        {
            ["//root/element/sub-elementA"] = "text-value",
            ["//sub-elementB"] = "text-value"
        };

        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.XML)
            .Validate(validationContext)
            .Process(processor)
            .Build();

        // Assert that no exception is thrown
        Assert.DoesNotThrow(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestSetMessageElementAttributes()
    {
        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>"
                                         + "<element attributeA='attribute-value' attributeB='attribute-value' >"
                                         + "<sub-elementA attribute='A'>text-value</sub-elementA>"
                                         + "<sub-elementB attribute='B'>text-value</sub-elementB>"
                                         + "<sub-elementC attribute='C'>text-value</sub-elementC>"
                                         + "</element>"
                                         + "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>"
            + "<element attributeA='attribute-value' attributeB='attribute-value' >"
            + "<sub-elementA attribute='to be overwritten'>text-value</sub-elementA>"
            + "<sub-elementB attribute='to be overwritten'>text-value</sub-elementB>"
            + "<sub-elementC attribute='C'>text-value</sub-elementC>"
            + "</element>"
            + "</root>"));

        var messageElements = new Dictionary<string, object>
        {
            ["//root/element/sub-elementA/@attribute"] = "A",
            ["//sub-elementB/@attribute"] = "B"
        };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(messageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.XML)
            .Process(processor)
            .Build();

        // Assert that no exception is thrown
        Assert.DoesNotThrow(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestSetMessageElementsError()
    {
        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>"
                                         + "<element attributeA='attribute-value' attributeB='attribute-value' >"
                                         + "<sub-elementA attribute='A'>text-value</sub-elementA>"
                                         + "<sub-elementB attribute='B'>text-value</sub-elementB>"
                                         + "<sub-elementC attribute='C'>text-value</sub-elementC>"
                                         + "</element>"
                                         + "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>"
            + "<element attributeA='attribute-value' attributeB='attribute-value' >"
            + "<sub-elementA attribute='A'>to be overwritten</sub-elementA>"
            + "<sub-elementB attribute='B'>to be overwritten</sub-elementB>"
            + "<sub-elementC attribute='C'>text-value</sub-elementC>"
            + "</element>"
            + "</root>"));

        var messageElements = new Dictionary<string, object>
        {
            ["//root/element/sub-element-wrong"] = "text-value",
            ["//sub-element-wrong"] = "text-value"
        };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(messageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.XML)
            .Process(processor)
            .Build();

        // Assert that AgenixSystemException is thrown
        Assert.Throws<AgenixSystemException>(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestSetMessageElementAttributesError()
    {
        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>"
                                         + "<element attributeA='attribute-value' attributeB='attribute-value' >"
                                         + "<sub-elementA attribute='A'>text-value</sub-elementA>"
                                         + "<sub-elementB attribute='B'>text-value</sub-elementB>"
                                         + "<sub-elementC attribute='C'>text-value</sub-elementC>"
                                         + "</element>"
                                         + "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>"
            + "<element attributeA='attribute-value' attributeB='attribute-value' >"
            + "<sub-elementA attribute='to be overwritten'>text-value</sub-elementA>"
            + "<sub-elementB attribute='to be overwritten'>text-value</sub-elementB>"
            + "<sub-elementC attribute='C'>text-value</sub-elementC>"
            + "</element>"
            + "</root>"));

        var messageElements = new Dictionary<string, object>
        {
            ["//root/element/sub-elementA/@attribute-wrong"] = "A",
            ["//sub-elementB/@attribute-wrong"] = "B"
        };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(messageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.XML)
            .Process(processor)
            .Build();

        // Assert that AgenixSystemException is thrown
        Assert.Throws<AgenixSystemException>(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestSetMessageElementAttributesErrorWrongElement()
    {
        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>"
                                         + "<element attributeA='attribute-value' attributeB='attribute-value' >"
                                         + "<sub-elementA attribute='A'>text-value</sub-elementA>"
                                         + "<sub-elementB attribute='B'>text-value</sub-elementB>"
                                         + "<sub-elementC attribute='C'>text-value</sub-elementC>"
                                         + "</element>"
                                         + "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>"
            + "<element attributeA='attribute-value' attributeB='attribute-value' >"
            + "<sub-elementA attribute='to be overwritten'>text-value</sub-elementA>"
            + "<sub-elementB attribute='to be overwritten'>text-value</sub-elementB>"
            + "<sub-elementC attribute='C'>text-value</sub-elementC>"
            + "</element>"
            + "</root>"));

        var messageElements = new Dictionary<string, object>
        {
            ["//root/element/sub-elementA-wrong/@attribute"] = "A",
            ["//sub-elementB-wrong/@attribute"] = "B"
        };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(messageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.XML)
            .Process(processor)
            .Build();

        // Assert that AgenixSystemException is thrown
        Assert.Throws<AgenixSystemException>(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestExtractMessageElements()
    {
        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>"
                                         + "<element attributeA='attribute-value' attributeB='attribute-value' >"
                                         + "<sub-elementA attribute='A'>text-value</sub-elementA>"
                                         + "<sub-elementB attribute='B'>text-value</sub-elementB>"
                                         + "<sub-elementC attribute='C'>text-value</sub-elementC>"
                                         + "</element>"
                                         + "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<root>"
                                                                          + "<element attributeA='attribute-value' attributeB='attribute-value' >"
                                                                          + "<sub-elementA attribute='A'>text-value</sub-elementA>"
                                                                          + "<sub-elementB attribute='B'>text-value</sub-elementB>"
                                                                          + "<sub-elementC attribute='C'>text-value</sub-elementC>"
                                                                          + "</element>"
                                                                          + "</root>"));

        var extractMessageElements = new Dictionary<string, object>
        {
            ["//root/element/sub-elementA"] = "${valueA}",
            ["//root/element/sub-elementB"] = "${valueB}"
        };

        var variableExtractor = new XpathPayloadVariableExtractor.Builder()
            .Expressions(extractMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Process(variableExtractor)
            .Build();

        receiveAction.Execute(Context);

        Assert.That(Context.GetVariables().ContainsKey("valueA"), Is.True);
        Assert.That(Context.GetVariables()["valueA"], Is.EqualTo("text-value"));
        Assert.That(Context.GetVariables().ContainsKey("valueB"), Is.True);
        Assert.That(Context.GetVariables()["valueB"], Is.EqualTo("text-value"));
    }

    [Test]
    public void TestExtractMessageAttributes()
    {
        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>"
                                         + "<element attributeA='attribute-value' attributeB='attribute-value' >"
                                         + "<sub-elementA attribute='A'>text-value</sub-elementA>"
                                         + "<sub-elementB attribute='B'>text-value</sub-elementB>"
                                         + "<sub-elementC attribute='C'>text-value</sub-elementC>"
                                         + "</element>"
                                         + "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<root>"
                                                                          + "<element attributeA='attribute-value' attributeB='attribute-value' >"
                                                                          + "<sub-elementA attribute='A'>text-value</sub-elementA>"
                                                                          + "<sub-elementB attribute='B'>text-value</sub-elementB>"
                                                                          + "<sub-elementC attribute='C'>text-value</sub-elementC>"
                                                                          + "</element>"
                                                                          + "</root>"));

        var extractMessageElements = new Dictionary<string, object>
        {
            ["//root/element/sub-elementA/@attribute"] = "${valueA}",
            ["//root/element/sub-elementB/@attribute"] = "${valueB}"
        };

        var variableExtractor = new XpathPayloadVariableExtractor.Builder()
            .Expressions(extractMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Process(variableExtractor)
            .Build();

        receiveAction.Execute(Context);

        Assert.That(Context.GetVariables().ContainsKey("valueA"), Is.True);
        Assert.That(Context.GetVariables()["valueA"], Is.EqualTo("A"));
        Assert.That(Context.GetVariables().ContainsKey("valueB"), Is.True);
        Assert.That(Context.GetVariables()["valueB"], Is.EqualTo("B"));
    }

    [Test]
    public void TestExtractMessageElementsForWrongElement()
    {
        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>"
                                         + "<element attributeA='attribute-value' attributeB='attribute-value' >"
                                         + "<sub-elementA attribute='A'>text-value</sub-elementA>"
                                         + "<sub-elementB attribute='B'>text-value</sub-elementB>"
                                         + "<sub-elementC attribute='C'>text-value</sub-elementC>"
                                         + "</element>"
                                         + "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<root>"
                                                                          + "<element attributeA='attribute-value' attributeB='attribute-value' >"
                                                                          + "<sub-elementA attribute='A'>text-value</sub-elementA>"
                                                                          + "<sub-elementB attribute='B'>text-value</sub-elementB>"
                                                                          + "<sub-elementC attribute='C'>text-value</sub-elementC>"
                                                                          + "</element>"
                                                                          + "</root>"));

        var extractMessageElements = new Dictionary<string, object>
        {
            ["//root/element/sub-element-wrong"] = "${valueA}",
            ["//element/sub-element-wrong"] = "${valueB}"
        };

        var variableExtractor = new XpathPayloadVariableExtractor.Builder()
            .Expressions(extractMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Process(variableExtractor)
            .Build();

        Assert.Throws<AgenixSystemException>(() => receiveAction.Execute(Context));

        Assert.That(Context.GetVariables().ContainsKey("valueA"), Is.False);
        Assert.That(Context.GetVariables().ContainsKey("valueB"), Is.False);
    }

    [Test]
    public void TestExtractMessageElementsForWrongAttribute()
    {
        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>"
                                         + "<element attributeA='attribute-value' attributeB='attribute-value' >"
                                         + "<sub-elementA attribute='A'>text-value</sub-elementA>"
                                         + "<sub-elementB attribute='B'>text-value</sub-elementB>"
                                         + "<sub-elementC attribute='C'>text-value</sub-elementC>"
                                         + "</element>"
                                         + "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<root>"
                                                                          + "<element attributeA='attribute-value' attributeB='attribute-value' >"
                                                                          + "<sub-elementA attribute='A'>text-value</sub-elementA>"
                                                                          + "<sub-elementB attribute='B'>text-value</sub-elementB>"
                                                                          + "<sub-elementC attribute='C'>text-value</sub-elementC>"
                                                                          + "</element>"
                                                                          + "</root>"));

        var extractMessageElements = new Dictionary<string, object>
        {
            ["//root/element/sub-elementA/@attribute-wrong"] = "${attributeA}"
        };

        var variableExtractor = new XpathPayloadVariableExtractor.Builder()
            .Expressions(extractMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Process(variableExtractor)
            .Build();

        Assert.Throws<AgenixSystemException>(() => receiveAction.Execute(Context));

        Assert.That(Context.GetVariables().ContainsKey("attributeA"), Is.False);
    }
}
