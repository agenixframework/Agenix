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
using System.IO;
using System.Linq;
using Agenix.Api.Endpoint;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Api.Report;
using Agenix.Api.Spi;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Api.Variable;
using Agenix.Core.Actions;
using Agenix.Core.Container;
using Agenix.Core.Dsl;
using Agenix.Core.Message;
using Agenix.Core.Message.Builder;
using Agenix.Core.Validation;
using Agenix.Core.Validation.Builder;
using Agenix.Core.Variable;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using IResource = Agenix.Api.IO.IResource;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.Tests.Actions.Dsl;

public class ReceiveMessageActionBuilderTest : AbstractNUnitSetUp
{
    private Mock<IEndpointConfiguration> _configuration;

    private TestContext _context;
    private Mock<IConsumer> _messageConsumer;
    private Mock<IEndpoint> _messageEndpoint;
    private Mock<IReferenceResolver> _referenceResolver;
    private Mock<IResource> _resource;

    [SetUp]
    public void PrepareTestContext()
    {
        // Initialize mocks
        _messageEndpoint = new Mock<IEndpoint>();
        _messageConsumer = new Mock<IConsumer>();
        _configuration = new Mock<IEndpointConfiguration>();
        _resource = new Mock<IResource>();
        _referenceResolver = new Mock<IReferenceResolver>();

        // Initialize the test context
        _context = Context;

        // Assuming DefaultTextEqualsMessageValidator is a custom validator you need to mock or initialize appropriately
        var validator = new DefaultTextEqualsMessageValidator();

        // Add validator to the context's message validator registry
        _context.MessageValidatorRegistry.AddMessageValidator("default", validator);
    }

    [Test]
    public void TestReceiveEmpty()
    {
        // Reset is not directly available in Moq, ensuring no stale setup
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Arrange
        _messageEndpoint.Setup(m => m.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);
        _messageConsumer.Setup(m => m.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage(""));

        // Assuming DefaultTestCaseRunner is a custom runner in your project
        var runner = new DefaultTestCaseRunner(_context);
        runner.Run(Receive(_messageEndpoint.Object));

        // Act
        var test = runner.GetTestCase();

        // Assert
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.AreEqual(typeof(ReceiveMessageAction), test.GetActions()[0].GetType());

        var action = (ReceiveMessageAction)test.GetActions()[0];
        ClassicAssert.AreEqual("receive", action.Name);

        ClassicAssert.AreEqual(nameof(MessageType.JSON), action.MessageType);
        ClassicAssert.AreEqual(_messageEndpoint.Object, action.Endpoint);
        ClassicAssert.AreEqual(1, action.ValidationContexts.Count);
        ClassicAssert.IsTrue(action.ValidationContexts.Any(vc => vc is HeaderValidationContext));
    }

    [Test]
    public void TestReceiveBuilder()
    {
        // Reset mocks to their default states
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Define mock behaviors
        _messageEndpoint.Setup(m => m.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);
        _messageConsumer.Setup(m => m.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("Foo").SetHeader("operation", "foo"));

        // Create a test case runner
        var runner = new DefaultTestCaseRunner(_context);
        runner.Run(new ReceiveMessageAction.Builder()
            .Endpoint(_messageEndpoint.Object)
            .Message(new DefaultMessage("Foo").SetHeader("operation", "foo"))
            .Type(MessageType.PLAINTEXT)
            .Build());

        // Get the executed test case
        var test = runner.GetTestCase();

        // Assertions to verify the behavior of the reception action
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.IsInstanceOf<ReceiveMessageAction>(test.GetActions()[0]);

        // Get the ReceiveMessageAction from the executed test case
        var action = (ReceiveMessageAction)test.GetActions()[0];

        // Action's name should be "receive"
        ClassicAssert.AreEqual("receive", action.Name);

        // Action's message type should match the one set in the builder
        ClassicAssert.AreEqual(nameof(MessageType.PLAINTEXT), action.MessageType);

        // Action's endpoint should match the provided endpoint
        ClassicAssert.AreEqual(_messageEndpoint.Object, action.Endpoint);

        // Action should have 3 validation contexts
        ClassicAssert.AreEqual(2, action.ValidationContexts.Count);

        // Verifying the presence of specific validation context instances
        ClassicAssert.IsTrue(action.ValidationContexts.Any(vc => vc is HeaderValidationContext));
        ClassicAssert.IsTrue(action.ValidationContexts.Any(vc => vc is DefaultValidationContext));

        // Verify the message builder and content
        ClassicAssert.IsInstanceOf<StaticMessageBuilder>(action.MessageBuilder);
        ClassicAssert.AreEqual("Foo", ((StaticMessageBuilder)action.MessageBuilder).GetMessage().Payload);
        ClassicAssert.IsNotNull(((StaticMessageBuilder)action.MessageBuilder).GetMessage().GetHeaders()["operation"]);
    }

    [Test]
    public void TestReceiveBuilderWithPayloadBuilder()
    {
        // Reset mocks to their default states
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Define mock behaviors
        _messageEndpoint.Setup(m => m.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);
        _messageConsumer.Setup(m => m.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello Agenix!</Message></TestRequest>")
                .SetHeader("operation", "foo"));

        _referenceResolver.Setup(r => r.Resolve<TestContext>()).Returns(_context);
        _referenceResolver.Setup(r => r.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(r => r.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(r => r.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());

        _context.SetReferenceResolver(_referenceResolver.Object);

        // Create test case runner
        var runner = new DefaultTestCaseRunner(_context);
        runner.Run(new ReceiveMessageAction.Builder()
            .Endpoint(_messageEndpoint.Object)
            .Message()
            .Body(ctx => "<TestRequest><Message>Hello Agenix!</Message></TestRequest>")
            .Build());

        // Get the executed test case
        var test = runner.GetTestCase();

        // Assertions to verify the behavior of the receive action
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.IsInstanceOf<ReceiveMessageAction>(test.GetActions()[0]);

        // Get the ReceiveMessageAction from the executed test case
        var action = (ReceiveMessageAction)test.GetActions()[0];

        // Action's name should be "receive"
        ClassicAssert.AreEqual("receive", action.Name);

        // Action's message type should match the one set in the builder
        ClassicAssert.AreEqual(nameof(MessageType.XML), action.MessageType);

        // Action's endpoint should match the provided endpoint
        ClassicAssert.AreEqual(_messageEndpoint.Object, action.Endpoint);

        // Action should have 3 validation contexts
        ClassicAssert.AreEqual(2, action.ValidationContexts.Count);

        // Verifying the presence of specific validation context instances
        ClassicAssert.IsTrue(action.ValidationContexts.Any(vc => vc is HeaderValidationContext));
        ClassicAssert.IsTrue(action.ValidationContexts.Any(vc => vc is DefaultValidationContext));

        // Verify the message builder and content
        ClassicAssert.IsInstanceOf<DefaultMessageBuilder>(action.MessageBuilder);
        ClassicAssert.AreEqual("<TestRequest><Message>Hello Agenix!</Message></TestRequest>",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessagePayload(_context, action.MessageType));
    }

    [Test]
    public void TestReceiveBuilderWithPayloadString()
    {
        // Reset mocks to their default states
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Define mock behaviors
        _messageEndpoint.Setup(m => m.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);
        _messageConsumer.Setup(m => m.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "foo"));

        // Create test case runner
        var runner = new DefaultTestCaseRunner(_context);
        runner.Run(new ReceiveMessageAction.Builder()
            .Endpoint(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>")
            .Build());

        // Get the executed test case
        var test = runner.GetTestCase();

        // Assertions to verify the behavior of the receive action
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.IsInstanceOf<ReceiveMessageAction>(test.GetActions()[0]);

        // Get the ReceiveMessageAction from the executed test case
        var action = (ReceiveMessageAction)test.GetActions()[0];

        // Action's name should be "receive"
        ClassicAssert.AreEqual("receive", action.Name);

        // Action's message type should match the one set in the builder
        ClassicAssert.AreEqual(nameof(MessageType.XML), action.MessageType);

        // Action's endpoint should match the provided endpoint
        ClassicAssert.AreEqual(_messageEndpoint.Object, action.Endpoint);

        // Action should have 3 validation contexts
        ClassicAssert.AreEqual(2, action.ValidationContexts.Count);

        // Verifying the presence of specific validation context instances
        ClassicAssert.IsTrue(action.ValidationContexts.Any(vc => vc is HeaderValidationContext));
        ClassicAssert.IsTrue(action.ValidationContexts.Any(vc => vc is DefaultValidationContext));

        // Verify the message builder and content
        ClassicAssert.IsInstanceOf<DefaultMessageBuilder>(action.MessageBuilder);
        ClassicAssert.AreEqual("<TestRequest><Message>Hello World!</Message></TestRequest>",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessagePayload(_context, action.MessageType));
    }

    [Test]
    public void TestReceiveBuilderWithPayloadResource()
    {
        // Reset mocks to their default states
        _resource.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Define mock behaviors
        _messageEndpoint.Setup(m => m.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);
        _messageConsumer.Setup(m => m.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "foo"));

        _resource.Setup(r => r.Exists).Returns(true);
        _resource.Setup(r => r.InputStream)
            .Returns(new MemoryStream("<TestRequest><Message>Hello World!</Message></TestRequest>"u8.ToArray()));

        // Create test case runner
        var runner = new DefaultTestCaseRunner(_context);
        runner.Run(new ReceiveMessageAction.Builder()
            .Endpoint(_messageEndpoint.Object)
            .Message()
            .Body(_resource.Object)
            .Build());

        // Get the executed test case
        var test = runner.GetTestCase();

        // Assertions to verify the behavior of the receive action
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.IsInstanceOf<ReceiveMessageAction>(test.GetActions()[0]);

        // Get the ReceiveMessageAction from the executed test case
        var action = (ReceiveMessageAction)test.GetActions()[0];

        // Action's name should be "receive"
        ClassicAssert.AreEqual("receive", action.Name);

        // Action's message type should match the one set in the builder
        ClassicAssert.AreEqual(nameof(MessageType.XML), action.MessageType);

        // Action's endpoint should match the provided endpoint
        ClassicAssert.AreEqual(_messageEndpoint.Object, action.Endpoint);

        // Action should have 3 validation contexts
        ClassicAssert.AreEqual(2, action.ValidationContexts.Count);

        // Verifying the presence of specific validation context instances
        ClassicAssert.IsTrue(action.ValidationContexts.Any(vc => vc is HeaderValidationContext));
        ClassicAssert.IsTrue(action.ValidationContexts.Any(vc => vc is DefaultValidationContext));

        // Verify the message builder and content
        ClassicAssert.IsInstanceOf<DefaultMessageBuilder>(action.MessageBuilder);
        ClassicAssert.AreEqual("<TestRequest><Message>Hello World!</Message></TestRequest>",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessagePayload(_context, action.MessageType));
    }

    [Test]
    public void TestReceiveBuilderWithEndpointName()
    {
        // Reset mocks to their default states
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Define mock behaviors
        _messageEndpoint.Setup(m => m.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);
        _messageConsumer.Setup(m => m.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "foo"));

        _referenceResolver.Setup(r => r.Resolve<TestContext>()).Returns(_context);
        _referenceResolver.Setup(r => r.Resolve<IEndpoint>("fooMessageEndpoint")).Returns(_messageEndpoint.Object);
        _referenceResolver.Setup(r => r.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(r => r.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(r => r.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());

        _context.SetReferenceResolver(_referenceResolver.Object);

        // Create a test case runner
        var runner = new DefaultTestCaseRunner(_context);
        runner.Run(Receive("fooMessageEndpoint")
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>"));

        // Get the executed test case
        var test = runner.GetTestCase();

        // Assertions to verify the behavior of the receive action
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.IsInstanceOf<ReceiveMessageAction>(test.GetActions()[0]);

        // Get the ReceiveMessageAction from the executed test case
        var action = (ReceiveMessageAction)test.GetActions()[0];

        // Action's name should be "receive"
        ClassicAssert.AreEqual("receive", action.Name);

        // Action's endpoint URI should match the one set in the builder
        ClassicAssert.AreEqual("fooMessageEndpoint", action.EndpointUri);

        // Action's message type should match the one set in the builder
        ClassicAssert.AreEqual(nameof(MessageType.XML), action.MessageType);
    }

    [Test]
    public void TestReceiveBuilderWithTimeout()
    {
        // Reset mocks to their default states
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Define mock behaviors
        _messageEndpoint.Setup(m => m.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageConsumer.Setup(m => m.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "foo"));

        // Create test case runner
        var runner = new DefaultTestCaseRunner(_context);
        runner.Run(new ReceiveMessageAction.Builder()
            .Endpoint(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>")
            .Timeout(1000L)
            .Build());

        // Get the executed test case
        var test = runner.GetTestCase();

        // Assertions to verify the behavior of the receive action
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.IsInstanceOf<ReceiveMessageAction>(test.GetActions()[0]);

        // Get the ReceiveMessageAction from the executed test case
        var action = (ReceiveMessageAction)test.GetActions()[0];

        // Action's name should be "receive"
        ClassicAssert.AreEqual("receive", action.Name);

        // Action's endpoint should match the provided endpoint
        ClassicAssert.AreEqual(_messageEndpoint.Object, action.Endpoint);

        // Action's receive timeout should match the one set in the builder
        ClassicAssert.AreEqual(1000L, action.ReceiveTimeout);
    }

    [Test]
    public void TestReceiveBuilderWithHeaders()
    {
        // Reset mocks to their default states
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Define mock behaviors
        _messageEndpoint.Setup(m => m.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);
        _messageConsumer.Setup(m => m.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("some", "value")
                .SetHeader("operation", "sayHello")
                .SetHeader("foo", "bar"));

        // Create the first test case runner
        var runner = new DefaultTestCaseRunner(_context);
        runner.Run(new ReceiveMessageAction.Builder()
            .Endpoint(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>")
            .Headers(new Dictionary<string, object> { { "some", "value" } })
            .Header("operation", "sayHello")
            .Header("foo", "bar")
            .Build());

        // Create the second test case runner
        runner.Run(new ReceiveMessageAction.Builder()
            .Endpoint(_messageEndpoint.Object)
            .Message()
            .Header("operation", "sayHello")
            .Header("foo", "bar")
            .Headers(new Dictionary<string, object> { { "some", "value" } })
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>")
            .Build());

        // Get the executed test case
        var test = runner.GetTestCase();

        // Assertions to verify the behavior of the receive actions
        ClassicAssert.AreEqual(2, test.GetActionCount());
        ClassicAssert.IsInstanceOf<ReceiveMessageAction>(test.GetActions()[0]);
        ClassicAssert.IsInstanceOf<ReceiveMessageAction>(test.GetActions()[1]);

        // Get the first ReceiveMessageAction from the executed test case
        var action = (ReceiveMessageAction)test.GetActions()[0];

        // Validate the properties of the first action
        ClassicAssert.AreEqual("receive", action.Name);
        ClassicAssert.AreEqual(_messageEndpoint.Object, action.Endpoint);
        ClassicAssert.AreEqual(nameof(MessageType.XML), action.MessageType);

        ClassicAssert.IsInstanceOf<DefaultMessageBuilder>(action.MessageBuilder);
        ClassicAssert.AreEqual("<TestRequest><Message>Hello World!</Message></TestRequest>",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessagePayload(_context, action.MessageType));
        var headers = ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaders(_context);
        ClassicAssert.IsTrue(headers.ContainsKey("some"));
        ClassicAssert.IsTrue(headers.ContainsKey("operation"));
        ClassicAssert.IsTrue(headers.ContainsKey("foo"));

        // Get the second ReceiveMessageAction from the executed test case
        action = (ReceiveMessageAction)test.GetActions()[1];

        // Validate the properties of the second action
        ClassicAssert.AreEqual("receive", action.Name);
        ClassicAssert.AreEqual(_messageEndpoint.Object, action.Endpoint);
        ClassicAssert.AreEqual(nameof(MessageType.XML), action.MessageType);

        ClassicAssert.IsInstanceOf<DefaultMessageBuilder>(action.MessageBuilder);
        ClassicAssert.AreEqual("<TestRequest><Message>Hello World!</Message></TestRequest>",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessagePayload(_context, action.MessageType));
        headers = ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaders(_context);
        ClassicAssert.IsTrue(headers.ContainsKey("some"));
        ClassicAssert.IsTrue(headers.ContainsKey("operation"));
        ClassicAssert.IsTrue(headers.ContainsKey("foo"));
    }

    [Test]
    public void TestReceiveBuilderWithHeaderData()
    {
        // Reset mocks
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Define mock behaviors
        _messageEndpoint.Setup(m => m.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);
        _messageConsumer.Setup(m => m.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "foo")
                .AddHeaderData("<Header><Name>operation</Name><Value>foo</Value></Header>"));

        var runner = new DefaultTestCaseRunner(_context);

        // First test run
        runner.Run(new ReceiveMessageAction.Builder()
            .Endpoint(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>")
            .Header("<Header><Name>operation</Name><Value>foo</Value></Header>")
            .Build());

        // Second test run
        runner.Run(new ReceiveMessageAction.Builder()
            .Endpoint(_messageEndpoint.Object)
            .Message(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>"))
            .Header("<Header><Name>operation</Name><Value>foo</Value></Header>")
            .Build());

        // Get the executed test case
        var test = runner.GetTestCase();

        // Assertions to verify the behavior of the receive action
        ClassicAssert.AreEqual(2, test.GetActionCount());
        ClassicAssert.IsInstanceOf<ReceiveMessageAction>(test.GetActions()[0]);
        ClassicAssert.IsInstanceOf<ReceiveMessageAction>(test.GetActions()[1]);

        // Get the first ReceiveMessageAction from the executed test case
        var action = (ReceiveMessageAction)test.GetActions()[0];

        // Validate the properties of the first action
        ClassicAssert.AreEqual("receive", action.Name);
        ClassicAssert.AreEqual(_messageEndpoint.Object, action.Endpoint);
        ClassicAssert.AreEqual(nameof(MessageType.XML), action.MessageType);
        ClassicAssert.IsInstanceOf<DefaultMessageBuilder>(action.MessageBuilder);
        ClassicAssert.AreEqual("<TestRequest><Message>Hello World!</Message></TestRequest>",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessagePayload(_context, action.MessageType));
        ClassicAssert.AreEqual(1,
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(_context).Count);
        ClassicAssert.AreEqual("<Header><Name>operation</Name><Value>foo</Value></Header>",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(_context)[0]);

        // Get the second ReceiveMessageAction from the executed test case
        action = (ReceiveMessageAction)test.GetActions()[1];

        // Validate the properties of the second action
        ClassicAssert.AreEqual("receive", action.Name);
        ClassicAssert.AreEqual(_messageEndpoint.Object, action.Endpoint);
        ClassicAssert.AreEqual(nameof(MessageType.XML), action.MessageType);
        ClassicAssert.IsInstanceOf<StaticMessageBuilder>(action.MessageBuilder);
        ClassicAssert.AreEqual("<TestRequest><Message>Hello World!</Message></TestRequest>",
            ((StaticMessageBuilder)action.MessageBuilder).GetMessage().Payload);
        ClassicAssert.AreEqual(1,
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(_context).Count);
        ClassicAssert.AreEqual("<Header><Name>operation</Name><Value>foo</Value></Header>",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(_context)[0]);
    }

    [Test]
    public void TestReceiveBuilderWithMultipleHeaderData()
    {
        // Reset mocks
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Define mock behaviors
        _messageEndpoint.Setup(m => m.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);
        _messageConsumer.Setup(m => m.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "foo")
                .AddHeaderData("<Header><Name>operation</Name><Value>foo1</Value></Header>")
                .AddHeaderData("<Header><Name>operation</Name><Value>foo2</Value></Header>"));

        var runner = new DefaultTestCaseRunner(_context);

        // First test run
        runner.Run(new ReceiveMessageAction.Builder()
            .Endpoint(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>")
            .Header("<Header><Name>operation</Name><Value>foo1</Value></Header>")
            .Header("<Header><Name>operation</Name><Value>foo2</Value></Header>")
            .Build());

        // Second test run
        runner.Run(new ReceiveMessageAction.Builder()
            .Endpoint(_messageEndpoint.Object)
            .Message(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>"))
            .Header("<Header><Name>operation</Name><Value>foo1</Value></Header>")
            .Header("<Header><Name>operation</Name><Value>foo2</Value></Header>")
            .Build());

        // Get the executed test case
        var test = runner.GetTestCase();

        // Assertions to verify the behavior of the receive action
        ClassicAssert.AreEqual(2, test.GetActionCount());
        ClassicAssert.IsInstanceOf<ReceiveMessageAction>(test.GetActions()[0]);
        ClassicAssert.IsInstanceOf<ReceiveMessageAction>(test.GetActions()[1]);

        // Get the first ReceiveMessageAction from the executed test case
        var action = (ReceiveMessageAction)test.GetActions()[0];

        // Validate the properties of the first action
        ClassicAssert.AreEqual("receive", action.Name);
        ClassicAssert.AreEqual(_messageEndpoint.Object, action.Endpoint);
        ClassicAssert.AreEqual(nameof(MessageType.XML), action.MessageType);
        ClassicAssert.IsInstanceOf<DefaultMessageBuilder>(action.MessageBuilder);
        ClassicAssert.AreEqual("<TestRequest><Message>Hello World!</Message></TestRequest>",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessagePayload(_context, action.MessageType));
        ClassicAssert.AreEqual(2,
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(_context).Count);
        ClassicAssert.AreEqual("<Header><Name>operation</Name><Value>foo1</Value></Header>",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(_context)[0]);
        ClassicAssert.AreEqual("<Header><Name>operation</Name><Value>foo2</Value></Header>",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(_context)[1]);

        // Get the second ReceiveMessageAction from the executed test case
        action = (ReceiveMessageAction)test.GetActions()[1];

        // Validate the properties of the second action
        ClassicAssert.AreEqual("receive", action.Name);
        ClassicAssert.AreEqual(_messageEndpoint.Object, action.Endpoint);
        ClassicAssert.AreEqual(MessageType.XML.ToString(), action.MessageType);
        ClassicAssert.IsInstanceOf<StaticMessageBuilder>(action.MessageBuilder);
        ClassicAssert.AreEqual("<TestRequest><Message>Hello World!</Message></TestRequest>",
            ((StaticMessageBuilder)action.MessageBuilder).GetMessage().Payload);
        ClassicAssert.AreEqual(2,
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(_context).Count);
        ClassicAssert.AreEqual("<Header><Name>operation</Name><Value>foo1</Value></Header>",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(_context)[0]);
        ClassicAssert.AreEqual("<Header><Name>operation</Name><Value>foo2</Value></Header>",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(_context)[1]);
    }

    [Test]
    public void TestReceiveBuilderWithHeaderDataBuilder()
    {
        // Reset mocks
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Define mock behaviors
        _messageEndpoint.Setup(m => m.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);
        _messageConsumer.Setup(m => m.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage()
                .AddHeaderData("<TestRequest><Message>Hello Agenix!</Message></TestRequest>")
                .SetHeader("operation", "foo"));

        _referenceResolver.Setup(r => r.Resolve<TestContext>()).Returns(_context);
        _referenceResolver.Setup(r => r.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(r => r.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(r => r.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());

        _context.SetReferenceResolver(_referenceResolver.Object);

        var runner = new DefaultTestCaseRunner(_context);
        runner.Run(new ReceiveMessageAction.Builder()
            .Endpoint(_messageEndpoint.Object)
            .Message()
            .Header(_ => "<TestRequest><Message>Hello Agenix!</Message></TestRequest>")
            .Build());

        // Get the executed test case
        var test = runner.GetTestCase();

        // Assertions to verify the behavior of the receive action
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.IsInstanceOf<ReceiveMessageAction>(test.GetActions()[0]);

        // Get the ReceiveMessageAction from the executed test case
        var action = (ReceiveMessageAction)test.GetActions()[0];

        // Validate the properties of the action
        ClassicAssert.AreEqual("receive", action.Name);
        ClassicAssert.AreEqual(nameof(MessageType.JSON), action.MessageType);
        ClassicAssert.AreEqual(_messageEndpoint.Object, action.Endpoint);
        ClassicAssert.AreEqual(1, action.ValidationContexts.Count);
        ClassicAssert.IsTrue(action.ValidationContexts.Any(vc => vc is HeaderValidationContext));

        ClassicAssert.IsInstanceOf<DefaultMessageBuilder>(action.MessageBuilder);
        ClassicAssert.AreEqual(1,
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(_context).Count);
        ClassicAssert.AreEqual("<TestRequest><Message>Hello Agenix!</Message></TestRequest>",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(_context)[0]);
    }

    [Test]
    public void TestReceiveBuilderWithHeaderResource()
    {
        // Reset mocks
        _resource.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Define mock behaviors
        _messageEndpoint.Setup(m => m.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);

        _messageConsumer.SetupSequence(m => m.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "foo")
                .AddHeaderData("<Header><Name>operation</Name><Value>foo</Value></Header>"))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "bar")
                .AddHeaderData("<Header><Name>operation</Name><Value>bar</Value></Header>"));

        _resource.SetupSequence(r => r.Exists)
            .Returns(true)
            .Returns(true);

        _resource.SetupSequence(r => r.InputStream)
            .Returns(new MemoryStream("<Header><Name>operation</Name><Value>foo</Value></Header>"u8.ToArray()))
            .Returns(new MemoryStream("<Header><Name>operation</Name><Value>bar</Value></Header>"u8.ToArray()));

        var runner = new DefaultTestCaseRunner(_context);

        // First test run
        runner.Run(new ReceiveMessageAction.Builder()
            .Endpoint(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>")
            .Header(_resource.Object)
            .Build());

        // Second test run
        runner.Run(new ReceiveMessageAction.Builder()
            .Endpoint(_messageEndpoint.Object)
            .Message(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>"))
            .Header(_resource.Object)
            .Build());

        // Get the executed test case
        var test = runner.GetTestCase();

        // Assertions to verify the behavior of the receive action
        ClassicAssert.AreEqual(2, test.GetActionCount());
        ClassicAssert.IsInstanceOf<ReceiveMessageAction>(test.GetActions()[0]);
        ClassicAssert.IsInstanceOf<ReceiveMessageAction>(test.GetActions()[1]);

        // Get the first ReceiveMessageAction from the executed test case
        var action = (ReceiveMessageAction)test.GetActions()[0];

        // Validate the properties of the first action
        ClassicAssert.AreEqual("receive", action.Name);
        ClassicAssert.AreEqual(nameof(MessageType.XML), action.MessageType);
        ClassicAssert.AreEqual(_messageEndpoint.Object, action.Endpoint);
        ClassicAssert.IsInstanceOf<DefaultMessageBuilder>(action.MessageBuilder);
        ClassicAssert.AreEqual("<TestRequest><Message>Hello World!</Message></TestRequest>",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessagePayload(_context, action.MessageType));
        ClassicAssert.AreEqual(1,
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(_context).Count);
        ClassicAssert.AreEqual("<Header><Name>operation</Name><Value>foo</Value></Header>",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(_context)[0]);

        // Get the second ReceiveMessageAction from the executed test case
        action = (ReceiveMessageAction)test.GetActions()[1];

        // Validate the properties of the second action
        ClassicAssert.AreEqual("receive", action.Name);
        ClassicAssert.AreEqual(nameof(MessageType.XML), action.MessageType);
        ClassicAssert.AreEqual(_messageEndpoint.Object, action.Endpoint);
        ClassicAssert.IsInstanceOf<StaticMessageBuilder>(action.MessageBuilder);
        ClassicAssert.AreEqual("<TestRequest><Message>Hello World!</Message></TestRequest>",
            ((StaticMessageBuilder)action.MessageBuilder).GetMessage().Payload);
        ClassicAssert.AreEqual(1,
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(_context).Count);
        ClassicAssert.AreEqual("<Header><Name>operation</Name><Value>bar</Value></Header>",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(_context)[0]);
    }

    [Test]
    public void TestReceiveBuilderWithMultipleHeaderResource()
    {
        // Reset mocks
        _resource.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Define mock behaviors
        _messageEndpoint.Setup(m => m.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);

        _messageConsumer.Setup(m => m.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "foo")
                .AddHeaderData("<Header><Name>operation</Name><Value>sayHello</Value></Header>")
                .AddHeaderData("<Header><Name>operation</Name><Value>foo</Value></Header>")
                .AddHeaderData("<Header><Name>operation</Name><Value>bar</Value></Header>"));

        _resource.SetupSequence(r => r.Exists).Returns(true).Returns(true).Returns(true).Returns(true);
        _resource.SetupSequence(r => r.InputStream)
            .Returns(new MemoryStream("<Header><Name>operation</Name><Value>foo</Value></Header>"u8.ToArray()))
            .Returns(new MemoryStream("<Header><Name>operation</Name><Value>bar</Value></Header>"u8.ToArray()))
            .Returns(new MemoryStream("<Header><Name>operation</Name><Value>foo</Value></Header>"u8.ToArray()))
            .Returns(new MemoryStream("<Header><Name>operation</Name><Value>bar</Value></Header>"u8.ToArray()));

        var runner = new DefaultTestCaseRunner(_context);

        // First test run
        runner.Run(new ReceiveMessageAction.Builder()
            .Endpoint(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>")
            .Header("<Header><Name>operation</Name><Value>sayHello</Value></Header>")
            .Header(_resource.Object)
            .Header(_resource.Object)
            .Build());

        // Second test run
        runner.Run(new ReceiveMessageAction.Builder()
            .Endpoint(_messageEndpoint.Object)
            .Message(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>"))
            .Header("<Header><Name>operation</Name><Value>sayHello</Value></Header>")
            .Header(_resource.Object)
            .Header(_resource.Object)
            .Build());

        // Get the executed test case
        var test = runner.GetTestCase();

        // Assertions to verify the behavior of the receive action
        ClassicAssert.AreEqual(2, test.GetActionCount());
        ClassicAssert.IsInstanceOf<ReceiveMessageAction>(test.GetActions()[0]);
        ClassicAssert.IsInstanceOf<ReceiveMessageAction>(test.GetActions()[1]);

        // Get the first ReceiveMessageAction from the executed test case
        var action = (ReceiveMessageAction)test.GetActions()[0];

        // Validate the properties of the first action
        ClassicAssert.AreEqual("receive", action.Name);
        ClassicAssert.AreEqual(nameof(MessageType.XML), action.MessageType);
        ClassicAssert.AreEqual(_messageEndpoint.Object, action.Endpoint);
        ClassicAssert.IsInstanceOf<DefaultMessageBuilder>(action.MessageBuilder);
        ClassicAssert.AreEqual("<TestRequest><Message>Hello World!</Message></TestRequest>",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessagePayload(_context, action.MessageType));
        ClassicAssert.AreEqual(3,
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(_context).Count);
        ClassicAssert.AreEqual("<Header><Name>operation</Name><Value>sayHello</Value></Header>",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(_context)[0]);
        ClassicAssert.AreEqual("<Header><Name>operation</Name><Value>foo</Value></Header>",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(_context)[1]);
        ClassicAssert.AreEqual("<Header><Name>operation</Name><Value>bar</Value></Header>",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(_context)[2]);

        // Get the second ReceiveMessageAction from the executed test case
        action = (ReceiveMessageAction)test.GetActions()[1];

        // Validate the properties of the second action
        ClassicAssert.AreEqual("receive", action.Name);
        ClassicAssert.AreEqual(MessageType.XML.ToString(), action.MessageType);
        ClassicAssert.AreEqual(_messageEndpoint.Object, action.Endpoint);
        ClassicAssert.IsInstanceOf<StaticMessageBuilder>(action.MessageBuilder);
        ClassicAssert.AreEqual("<TestRequest><Message>Hello World!</Message></TestRequest>",
            ((StaticMessageBuilder)action.MessageBuilder).GetMessage().Payload);
        ClassicAssert.AreEqual(3,
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(_context).Count);
        ClassicAssert.AreEqual("<Header><Name>operation</Name><Value>sayHello</Value></Header>",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(_context)[0]);
        ClassicAssert.AreEqual("<Header><Name>operation</Name><Value>foo</Value></Header>",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(_context)[1]);
        ClassicAssert.AreEqual("<Header><Name>operation</Name><Value>bar</Value></Header>",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(_context)[2]);
    }

    [Test]
    public void TestReceiveBuilderWithValidator()
    {
        // Reset mocks
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Define mock behaviors
        _messageEndpoint.Setup(m => m.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);

        _messageConsumer.Setup(m => m.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("TestMessage").SetHeader("operation", "sayHello"));

        var validator = new DefaultTextEqualsMessageValidator();

        var runner = new DefaultTestCaseRunner(_context);

        runner.Run(new ReceiveMessageAction.Builder()
            .Endpoint(_messageEndpoint.Object)
            .Message()
            .Type(MessageType.PLAINTEXT)
            .Body("TestMessage")
            .Header("operation", "sayHello")
            .Validator(validator)
            .Build());

        // Get the executed test case
        var test = runner.GetTestCase();

        // Assertions to verify the behavior of the receive action
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.IsInstanceOf<ReceiveMessageAction>(test.GetActions()[0]);

        // Get the ReceiveMessageAction from the executed test case
        var action = (ReceiveMessageAction)test.GetActions()[0];

        // Validate the properties of the action
        ClassicAssert.AreEqual("receive", action.Name);
        ClassicAssert.AreEqual(_messageEndpoint.Object, action.Endpoint);
        ClassicAssert.AreEqual(MessageType.PLAINTEXT.ToString(), action.MessageType);
        ClassicAssert.AreEqual(1, action.Validators.Count);
        ClassicAssert.AreEqual(validator, action.Validators[0]);

        ClassicAssert.IsInstanceOf<DefaultMessageBuilder>(action.MessageBuilder);
        ClassicAssert.AreEqual("TestMessage",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessagePayload(_context, action.MessageType));
        ClassicAssert.IsTrue(((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaders(_context)
            .ContainsKey("operation"));
    }

    [Test]
    public void TestReceiveBuilderWithValidatorName()
    {
        var validator = new DefaultTextEqualsMessageValidator();

        // Reset mocks
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Set up mock behaviors
        _messageEndpoint.Setup(m => m.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);

        _messageConsumer.Setup(m => m.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("TestMessage").SetHeader("operation", "sayHello"));

        _referenceResolver.Setup(r => r.Resolve<TestContext>()).Returns(_context);
        _referenceResolver.Setup(r => r.Resolve("plainTextValidator")).Returns(validator);
        _referenceResolver.Setup(r => r.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(r => r.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(r => r.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());

        _context.SetReferenceResolver(_referenceResolver.Object);

        var runner = new DefaultTestCaseRunner(_context);

        runner.Run(Receive(_messageEndpoint.Object)
            .Message()
            .Type(MessageType.PLAINTEXT)
            .Body("TestMessage")
            .Header("operation", "sayHello")
            .Validator("plainTextValidator"));

        // Get the executed test case
        var test = runner.GetTestCase();

        // Assertions to verify the behavior of the receive action
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.IsInstanceOf<ReceiveMessageAction>(test.GetActions()[0]);

        // Get the ReceiveMessageAction from the executed test case
        var action = (ReceiveMessageAction)test.GetActions()[0];

        // Validate the properties of the action
        ClassicAssert.AreEqual("receive", action.Name);
        ClassicAssert.AreEqual(_messageEndpoint.Object, action.Endpoint);
        ClassicAssert.AreEqual(MessageType.PLAINTEXT.ToString(), action.MessageType);
        ClassicAssert.AreEqual(1, action.Validators.Count);
        ClassicAssert.AreEqual(validator, action.Validators[0]);

        ClassicAssert.IsInstanceOf<DefaultMessageBuilder>(action.MessageBuilder);
        ClassicAssert.AreEqual("TestMessage",
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessagePayload(_context, action.MessageType));
        ClassicAssert.IsTrue(((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaders(_context)
            .ContainsKey("operation"));
    }

    [Test]
    public void TestReceiveBuilderWithSelector()
    {
        var selectiveConsumer = new Mock<ISelectiveConsumer>();

        // Reset mocks
        _messageEndpoint.Reset();
        _configuration.Reset();
        selectiveConsumer.Reset();

        // Define mock behaviors
        _messageEndpoint.Setup(m => m.CreateConsumer()).Returns(selectiveConsumer.Object);
        _messageEndpoint.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);

        selectiveConsumer.Setup(m => m.Receive(It.Is<string>(s => s == "operation = 'sayHello'"),
                It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "sayHello"));

        var messageSelector = new Dictionary<string, object> { { "operation", "sayHello" } };

        var runner = new DefaultTestCaseRunner(_context);

        runner.Run(Receive(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>")
            .Selector(messageSelector));

        // Get the executed test case
        var test = runner.GetTestCase();

        // Assertions to verify the behavior of the receive action
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.IsInstanceOf<ReceiveMessageAction>(test.GetActions()[0]);

        // Get the ReceiveMessageAction from the executed test case
        var action = (ReceiveMessageAction)test.GetActions()[0];

        // Validate the properties of the action
        ClassicAssert.AreEqual("receive", action.Name);
        ClassicAssert.AreEqual(nameof(MessageType.XML), action.MessageType);
        ClassicAssert.AreEqual(_messageEndpoint.Object, action.Endpoint);

        // Verify message selector map
        ClassicAssert.AreEqual(messageSelector, action.MessageSelectors);
    }

    [Test]
    public void TestReceiveBuilderWithSelectorExpression()
    {
        var selectiveConsumer = new Mock<ISelectiveConsumer>();

        // Reset mocks
        _messageEndpoint.Reset();
        _configuration.Reset();
        selectiveConsumer.Reset();

        // Define mock behaviors
        _messageEndpoint.Setup(m => m.CreateConsumer()).Returns(selectiveConsumer.Object);
        _messageEndpoint.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);

        selectiveConsumer.Setup(m => m.Receive(It.Is<string>(s => s == "operation = 'sayHello'"),
                It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "sayHello"));

        var runner = new DefaultTestCaseRunner(_context);

        runner.Run(Receive(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>")
            .Selector("operation = 'sayHello'")
            .Build());

        // Get the executed test case
        var test = runner.GetTestCase();

        // Assertions to verify the behavior of the receive action
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.IsInstanceOf<ReceiveMessageAction>(test.GetActions()[0]);

        // Get the ReceiveMessageAction from the executed test case
        var action = (ReceiveMessageAction)test.GetActions()[0];

        // Validate the properties of the action
        ClassicAssert.AreEqual("receive", action.Name);
        ClassicAssert.AreEqual(nameof(MessageType.XML), action.MessageType);
        ClassicAssert.AreEqual(_messageEndpoint.Object, action.Endpoint);

        // Verify message selector and ensure map is empty
        ClassicAssert.IsTrue(action.MessageSelectors.Count == 0);
        ClassicAssert.AreEqual("operation = 'sayHello'", action.Selector);
    }

    [Test]
    public void TestReceiveBuilderExtractor()
    {
        void Extractor(IMessage message, TestContext context)
        {
            context.SetVariable("messageId", message.Id);
        }

        // Create the received message
        var received = new DefaultMessage("<TestRequest><Message lang=\"ENG\">Hello World!</Message></TestRequest>")
            .SetHeader("operation", "sayHello");

        // Reset mocks
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Define mock behaviors
        _messageEndpoint.Setup(m => m.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);

        _messageConsumer.Setup(m => m.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(received);

        _referenceResolver.Setup(r => r.Resolve<TestContext>()).Returns(_context);
        _referenceResolver.Setup(r => r.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(r => r.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(r => r.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());

        _context.SetReferenceResolver(_referenceResolver.Object);

        var runner = new DefaultTestCaseRunner(_context);

        runner.Run(Receive(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message lang=\"ENG\">Hello World!</Message></TestRequest>")
            .Process((VariableExtractor)Extractor));

        // Validate if the variable was set correctly
        ClassicAssert.IsNotNull(_context.GetVariable("messageId"));
        ClassicAssert.AreEqual(_context.GetVariable("messageId"), received.Id);

        // Get the executed test case
        var test = runner.GetTestCase();

        // Assertions to verify the behavior of the receive action
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.IsInstanceOf<ReceiveMessageAction>(test.GetActions()[0]);

        // Get the ReceiveMessageAction from the executed test case
        var action = (ReceiveMessageAction)test.GetActions()[0];

        // Validate the properties of the action
        ClassicAssert.AreEqual("receive", action.Name);
        ClassicAssert.AreEqual(nameof(MessageType.XML), action.MessageType);
        ClassicAssert.AreEqual(_messageEndpoint.Object, action.Endpoint);

        // Verify variable extractor
        ClassicAssert.AreEqual(1, action.VariableExtractors.Count);
        ClassicAssert.AreEqual(typeof(DelegatingVariableExtractor), action.VariableExtractors[0].GetType());
    }

    [Test]
    public void TestReceiveBuilderExtractFromHeader()
    {
        VariableExtractor extractor = (message, context) => context.SetVariable("messageId", message.Id);

        // Setup mock behaviors
        _messageEndpoint.Setup(m => m.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);

        var received = new DefaultMessage("<TestRequest><Message lang=\"ENG\">Hello World!</Message></TestRequest>")
            .SetHeader("operation", "sayHello")
            .SetHeader("requestId", "123456");
        _messageConsumer.Setup(m => m.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(received);

        // Run test case
        var runner = new DefaultTestCaseRunner(_context);

        runner.Run(Receive(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message lang=\"ENG\">Hello World!</Message></TestRequest>")
            .Extract(MessageSupport.Message()
                .Headers()
                .Header("operation", "operationHeader")
                .Header("requestId", "id"))
            .Process(extractor));

        // Validate variable extraction
        ClassicAssert.IsNotNull(_context.GetVariable("operationHeader"));
        ClassicAssert.IsNotNull(_context.GetVariable("id"));
        ClassicAssert.AreEqual("sayHello", _context.GetVariable("operationHeader"));
        ClassicAssert.AreEqual("123456", _context.GetVariable("id"));

        ClassicAssert.IsNotNull(_context.GetVariable("messageId"));
        ClassicAssert.AreEqual(received.Id, _context.GetVariable("messageId"));

        // Get and validate an executed test case
        var test = runner.GetTestCase();
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.IsInstanceOf<ReceiveMessageAction>(test.GetActions()[0]);

        var action = (ReceiveMessageAction)test.GetActions()[0];
        ClassicAssert.AreEqual("receive", action.Name);
        ClassicAssert.AreEqual(nameof(MessageType.XML), action.MessageType);
        ClassicAssert.AreEqual(_messageEndpoint.Object, action.Endpoint);

        // Validate variable extractor
        ClassicAssert.AreEqual(2, action.VariableExtractors.Count);
        ClassicAssert.IsInstanceOf<MessageHeaderVariableExtractor>(action.VariableExtractors[0]);
        var ext1 = (MessageHeaderVariableExtractor)action.VariableExtractors[0];
        ClassicAssert.IsTrue(ext1.GetHeaderMappings().ContainsKey("operation"));
        ClassicAssert.IsTrue(ext1.GetHeaderMappings().ContainsKey("requestId"));

        ClassicAssert.IsInstanceOf<DelegatingVariableExtractor>(action.VariableExtractors[1]);
    }

    [Test]
    public void TestReceiveBuilderWithValidationProcessor()
    {
        var callback = new Mock<AbstractValidationProcessor<dynamic>>();

        // Setup mock behaviors
        _messageEndpoint.Setup(m => m.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);

        _messageConsumer.Setup(m => m.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("TestMessage").SetHeader("operation", "sayHello"));


        callback.Setup(c => c.Validate(It.IsAny<IMessage>(), It.IsAny<TestContext>()));

        // Run test case
        var runner = new DefaultTestCaseRunner(_context);

        runner.Run(Receive(_messageEndpoint.Object)
            .Message()
            .Type(MessageType.PLAINTEXT)
            .Body("TestMessage")
            .Header("operation", "sayHello")
            .Validate(callback.Object));

        // Get and validate executed test case
        var test = runner.GetTestCase();
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.IsInstanceOf<ReceiveMessageAction>(test.GetActions()[0]);

        var action = (ReceiveMessageAction)test.GetActions()[0];
        ClassicAssert.AreEqual("receive", action.Name);
        ClassicAssert.AreEqual(MessageType.PLAINTEXT.ToString(), action.MessageType);
        ClassicAssert.AreEqual(_messageEndpoint.Object, action.Endpoint);
        ClassicAssert.AreEqual(callback.Object, action.Processor);

        ClassicAssert.IsInstanceOf<DefaultMessageBuilder>(action.MessageBuilder);
        var messageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        ClassicAssert.AreEqual("TestMessage", messageBuilder.BuildMessagePayload(_context, action.MessageType));
        ClassicAssert.IsTrue(messageBuilder.BuildMessageHeaders(_context).ContainsKey("operation"));

        // Verify callback interactions
        callback.Verify(c => c.Validate(It.IsAny<IMessage>(), It.IsAny<TestContext>()), Times.Once);
    }
}
