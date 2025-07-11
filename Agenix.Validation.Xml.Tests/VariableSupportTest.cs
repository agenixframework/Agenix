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

namespace Agenix.Validation.Xml.Tests;

public class VariableSupportTest : AbstractNUnitSetUp
{
    private readonly Mock<IConsumer> _consumer = new();
    private readonly Mock<IEndpoint> _endpoint = new();
    private readonly Mock<IEndpointConfiguration> _endpointConfiguration = new();

    [Test]
    public void TestValidateMessageElementsVariablesSupport()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                         "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                         "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                         "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                         "</element>" +
                                         "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        Context.GetVariables()["variable"] = "text-value";

        var validateMessageElements = new Dictionary<string, object>
        {
            { "//root/element/sub-elementA", "${variable}" }, { "//sub-elementB", "${variable}" }
        };

        // Act
        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Assert - No exceptions should be thrown, validation should pass
        Assert.That(() => receiveAction.Execute(Context), Throws.Nothing);
    }

    [Test]
    public void TestValidateMessageElementsFunctionSupport()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                         "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                         "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                         "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                         "</element>" +
                                         "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        Context.GetVariables()["variable"] = "text-value";
        Context.GetVariables()["text"] = "text";

        var validateMessageElements = new Dictionary<string, object>
        {
            { "//root/element/sub-elementA", "agenix:Concat('text', '-', 'value')" },
            { "//sub-elementB", "agenix:Concat(${text}, '-', 'value')" }
        };

        // Act
        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Assert - No exceptions should be thrown, validation should pass
        Assert.That(() => receiveAction.Execute(Context), Throws.Nothing);
    }

    [Test]
    public void TestValidateMessageElementsVariableSupportInExpression()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                         "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                         "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                         "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                         "</element>" +
                                         "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        Context.GetVariables()["expression"] = "//root/element/sub-elementA";

        var validateMessageElements = new Dictionary<string, object> { { "${expression}", "text-value" } };

        // Act
        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Assert - No exceptions should be thrown, validation should pass
        Assert.That(() => receiveAction.Execute(Context), Throws.Nothing);
    }

    [Test]
    public void TestValidateMessageElementsFunctionSupportInExpression()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                         "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                         "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                         "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                         "</element>" +
                                         "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        Context.GetVariables()["variable"] = "B";

        var validateMessageElements = new Dictionary<string, object>
        {
            { "agenix:Concat('//root/', 'element/sub-elementA')", "text-value" },
            { "agenix:Concat('//sub-element', ${variable})", "text-value" }
        };

        // Act
        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        // Assert - Function-based XPath expressions should resolve and validate correctly
        Assert.That(() => receiveAction.Execute(Context), Throws.Nothing);
    }

    [Test]
    public void TestValidateHeaderValuesVariablesSupport()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                         "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                         "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                         "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                         "</element>" +
                                         "</root>")
            .SetHeader("header-valueA", "A")
            .SetHeader("header-valueB", "B")
            .SetHeader("header-valueC", "C");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<root>" +
                                                                          "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                                                          "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                                                          "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                                                          "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                                                          "</element>" +
                                                                          "</root>"));

        // Set up variables for header validation
        Context.GetVariables()["variableA"] = "A";
        Context.GetVariables()["variableB"] = "B";
        Context.GetVariables()["variableC"] = "C";

        var validateHeaderValues = new Dictionary<string, object>
        {
            { "header-valueA", "${variableA}" },
            { "header-valueB", "${variableB}" },
            { "header-valueC", "${variableC}" }
        };

        controlMessageBuilder.AddHeaderBuilder(new DefaultHeaderBuilder(validateHeaderValues));

        // Act
        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        // Assert - Variables in header values should be resolved correctly
        Assert.That(() => receiveAction.Execute(Context), Throws.Nothing);
    }

    [Test]
    public void TestValidateHeaderValuesFunctionSupport()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                         "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                         "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                         "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                         "</element>" +
                                         "</root>")
            .SetHeader("header-valueA", "A")
            .SetHeader("header-valueB", "B")
            .SetHeader("header-valueC", "C");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<root>" +
                                                                          "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                                                          "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                                                          "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                                                          "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                                                          "</element>" +
                                                                          "</root>"));

        // Set up variable for function parameter
        Context.GetVariables()["variableC"] = "c";

        var validateHeaderValues = new Dictionary<string, object>
        {
            { "header-valueA", "agenix:UpperCase('a')" },
            { "header-valueB", "agenix:UpperCase('b')" },
            { "header-valueC", "agenix:UpperCase(${variableC})" }
        };

        controlMessageBuilder.AddHeaderBuilder(new DefaultHeaderBuilder(validateHeaderValues));

        // Act
        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        // Assert - Function expressions in header values should be resolved correctly
        Assert.That(() => receiveAction.Execute(Context), Throws.Nothing);
    }

    [Test]
    public void TestHeaderNameVariablesSupport()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                         "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                         "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                         "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                         "</element>" +
                                         "</root>")
            .SetHeader("header-valueA", "A")
            .SetHeader("header-valueB", "B")
            .SetHeader("header-valueC", "C");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<root>" +
                                                                          "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                                                          "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                                                          "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                                                          "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                                                          "</element>" +
                                                                          "</root>"));

        // Set up variables for header names
        Context.GetVariables()["variableA"] = "header-valueA";
        Context.GetVariables()["variableB"] = "header-valueB";
        Context.GetVariables()["variableC"] = "header-valueC";

        var validateHeaderValues = new Dictionary<string, object>
        {
            { "${variableA}", "A" }, { "${variableB}", "B" }, { "${variableC}", "C" }
        };

        controlMessageBuilder.AddHeaderBuilder(new DefaultHeaderBuilder(validateHeaderValues));

        // Act
        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        // Assert - Variables in header names should be resolved correctly
        Assert.That(() => receiveAction.Execute(Context), Throws.Nothing);
    }

    [Test]
    public void TestHeaderNameFunctionSupport()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                         "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                         "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                         "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                         "</element>" +
                                         "</root>")
            .SetHeader("header-valueA", "A")
            .SetHeader("header-valueB", "B")
            .SetHeader("header-valueC", "C");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<root>" +
                                                                          "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                                                          "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                                                          "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                                                          "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                                                          "</element>" +
                                                                          "</root>"));

        var validateHeaderValues = new Dictionary<string, object>
        {
            { "agenix:Concat('header', '-', 'valueA')", "A" },
            { "agenix:Concat('header', '-', 'valueB')", "B" },
            { "agenix:Concat('header', '-', 'valueC')", "C" }
        };

        controlMessageBuilder.AddHeaderBuilder(new DefaultHeaderBuilder(validateHeaderValues));

        // Act
        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        // Assert - Functions in header names should be resolved correctly
        Assert.That(() => receiveAction.Execute(Context), Throws.Nothing);
    }

    [Test]
    public void TestExtractMessageElementsVariablesSupport()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                         "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                         "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                         "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                         "</element>" +
                                         "</root>");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        Context.GetVariables()["variableA"] = "initial";
        Context.GetVariables()["variableB"] = "initial";

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<root>" +
                                                                          "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                                                          "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                                                          "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                                                          "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                                                          "</element>" +
                                                                          "</root>"));

        var extractMessageElements = new Dictionary<string, object>
        {
            { "//root/element/sub-elementA", "${variableA}" }, { "//root/element/sub-elementB", "${variableB}" }
        };

        var variableExtractor = new XpathPayloadVariableExtractor.Builder()
            .Expressions(extractMessageElements)
            .Build();

        // Act
        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Process(variableExtractor)
            .Build();

        receiveAction.Execute(Context);

        // Assert - Variables should be extracted from message elements
        Assert.That(Context.GetVariables().ContainsKey("variableA"), Is.True);
        Assert.That(Context.GetVariables()["variableA"], Is.EqualTo("text-value"));
        Assert.That(Context.GetVariables().ContainsKey("variableB"), Is.True);
        Assert.That(Context.GetVariables()["variableB"], Is.EqualTo("text-value"));
    }

    [Test]
    public void TestExtractHeaderValuesVariablesSupport()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<root>" +
                                         "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                         "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                         "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                         "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                         "</element>" +
                                         "</root>")
            .SetHeader("header-valueA", "A")
            .SetHeader("header-valueB", "B")
            .SetHeader("header-valueC", "C");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        Context.GetVariables()["variableA"] = "initial";
        Context.GetVariables()["variableB"] = "initial";

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<root>" +
                                                                          "<element attributeA='attribute-value' attributeB='attribute-value' >" +
                                                                          "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                                                                          "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                                                                          "<sub-elementC attribute='C'>text-value</sub-elementC>" +
                                                                          "</element>" +
                                                                          "</root>"));

        var extractHeaderValues = new Dictionary<string, string>
        {
            { "header-valueA", "${variableA}" }, { "header-valueB", "${variableB}" }
        };

        var variableExtractor = new MessageHeaderVariableExtractor.Builder()
            .Headers(extractHeaderValues)
            .Build();

        // Act
        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Process(variableExtractor)
            .Build();

        receiveAction.Execute(Context);

        // Assert - Variables should be extracted from message headers
        Assert.That(Context.GetVariables().ContainsKey("variableA"), Is.True);
        Assert.That(Context.GetVariables()["variableA"], Is.EqualTo("A"));
        Assert.That(Context.GetVariables().ContainsKey("variableB"), Is.True);
        Assert.That(Context.GetVariables()["variableB"], Is.EqualTo("B"));
    }

    [Test]
    public void TestExtractStandardMessageHeaders()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var correlationId = Guid.NewGuid().ToString();
        var messageId = Guid.NewGuid().ToString();

        var message = new DefaultMessage("<payload>Standard headers test</payload>")
            .SetHeader("agenix_message_timestamp", timestamp)
            .SetHeader("correlation_id", correlationId)
            .SetHeader("content_type", "application/xml")
            .SetHeader("message_type", "REQUEST");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        Context.GetVariables()["extractedMessageId"] = "initial";
        Context.GetVariables()["extractedTimestamp"] = "initial";
        Context.GetVariables()["extractedCorrelationId"] = "initial";
        Context.GetVariables()["extractedContentType"] = "initial";
        Context.GetVariables()["extractedMessageType"] = "initial";

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<payload>Standard headers test</payload>"));

        var extractHeaderValues = new Dictionary<string, string>
        {
            { "agenix_message_timestamp", "${extractedTimestamp}" },
            { "correlation_id", "${extractedCorrelationId}" },
            { "content_type", "${extractedContentType}" },
            { "message_type", "${extractedMessageType}" }
        };

        var variableExtractor = new MessageHeaderVariableExtractor.Builder()
            .Headers(extractHeaderValues)
            .Build();

        // Act
        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Process(variableExtractor)
            .Build();

        receiveAction.Execute(Context);

        // Assert - Standard message headers should be extracted
        Assert.That(Context.GetVariables()["extractedTimestamp"], Is.EqualTo(timestamp));
        Assert.That(Context.GetVariables()["extractedCorrelationId"], Is.EqualTo(correlationId));
        Assert.That(Context.GetVariables()["extractedContentType"], Is.EqualTo("application/xml"));
        Assert.That(Context.GetVariables()["extractedMessageType"], Is.EqualTo("REQUEST"));
    }

    [Test]
    public void TestExtractCustomApplicationHeaders()
    {
        // Arrange
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        var message = new DefaultMessage("<order><id>12345</id><amount>99.99</amount></order>")
            .SetHeader("x-api-version", "v2.1")
            .SetHeader("x-request-id", "req-abc-123")
            .SetHeader("x-user-id", "user789")
            .SetHeader("x-tenant-id", "tenant456")
            .SetHeader("x-transaction-id", "txn-def-456")
            .SetHeader("authorization", "Bearer token123")
            .SetHeader("x-source-system", "order-service")
            .SetHeader("x-request-timestamp", "2024-06-06T10:30:00Z");

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        Context.GetVariables()["apiVersion"] = "initial";
        Context.GetVariables()["requestId"] = "initial";
        Context.GetVariables()["userId"] = "initial";
        Context.GetVariables()["tenantId"] = "initial";
        Context.GetVariables()["transactionId"] = "initial";
        Context.GetVariables()["authToken"] = "initial";
        Context.GetVariables()["sourceSystem"] = "initial";
        Context.GetVariables()["requestTimestamp"] = "initial";

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(
            new DefaultPayloadBuilder("<order><id>12345</id><amount>99.99</amount></order>"));

        var extractHeaderValues = new Dictionary<string, string>
        {
            { "x-api-version", "${apiVersion}" },
            { "x-request-id", "${requestId}" },
            { "x-user-id", "${userId}" },
            { "x-tenant-id", "${tenantId}" },
            { "x-transaction-id", "${transactionId}" },
            { "authorization", "${authToken}" },
            { "x-source-system", "${sourceSystem}" },
            { "x-request-timestamp", "${requestTimestamp}" }
        };

        var variableExtractor = new MessageHeaderVariableExtractor.Builder()
            .Headers(extractHeaderValues)
            .Build();

        // Act
        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Process(variableExtractor)
            .Build();

        receiveAction.Execute(Context);

        // Assert - Custom application headers should be extracted
        Assert.That(Context.GetVariables()["apiVersion"], Is.EqualTo("v2.1"));
        Assert.That(Context.GetVariables()["requestId"], Is.EqualTo("req-abc-123"));
        Assert.That(Context.GetVariables()["userId"], Is.EqualTo("user789"));
        Assert.That(Context.GetVariables()["tenantId"], Is.EqualTo("tenant456"));
        Assert.That(Context.GetVariables()["transactionId"], Is.EqualTo("txn-def-456"));
        Assert.That(Context.GetVariables()["authToken"], Is.EqualTo("Bearer token123"));
        Assert.That(Context.GetVariables()["sourceSystem"], Is.EqualTo("order-service"));
        Assert.That(Context.GetVariables()["requestTimestamp"], Is.EqualTo("2024-06-06T10:30:00Z"));
    }
}
