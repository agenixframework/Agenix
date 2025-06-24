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
using Agenix.Api.IO;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Api.Report;
using Agenix.Api.Spi;
using Agenix.Api.Xml;
using Agenix.Core;
using Agenix.Core.Actions;
using Agenix.Core.Container;
using Agenix.Core.Validation.Builder;
using Agenix.Core.Xml;
using Agenix.Validation.Xml.Dsl;
using Agenix.Validation.Xml.Message.Builder;
using Agenix.Validation.Xml.Tests.Message;
using Agenix.Validation.Xml.Validation.Xml;
using Moq;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Xml.Tests.Actions.Dsl;

public class SendMessageActionBuilderTest : AbstractNUnitSetUp
{
    private readonly Xml2Marshaller _marshaller = new(typeof(TestRequest));
    private readonly Mock<IEndpoint> _messageEndpoint = new();
    private readonly Mock<IProducer> _messageProducer = new();
    private readonly Mock<IReferenceResolver> _referenceResolver = new();
    private readonly Mock<IResource> _resource = new();

    [SetUp]
    public void SetUp()
    {
        _marshaller.SetProperty("omitxmldeclaration", "true");
        _marshaller.SetProperty("stripnamespaces", "true");
        _marshaller.SetProperty("removeemptylines", "true");
    }

    [Test]
    public void TestSendBuilderWithPayloadModel()
    {
        // Setup mocks
        _messageEndpoint.Setup(x => x.CreateProducer()).Returns(_messageProducer.Object);

        _messageProducer.Setup(x => x.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, ctx) =>
            {
                Assert.That(message.GetPayload<string>(),
                    Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
            });

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());
        _referenceResolver.Setup(x => x.ResolveAll<IMarshaller>())
            .Returns(new Dictionary<string, IMarshaller> { { "marshaller", _marshaller } });
        _referenceResolver.Setup(x => x.Resolve<IMarshaller>()).Returns(_marshaller);

        Context.SetReferenceResolver(_referenceResolver.Object);
        var runner = new DefaultTestCaseRunner(Context);

        runner.Run(SendMessageAction.Builder.Send(_messageEndpoint.Object)
            .Message()
            .Type(MessageType.XML)
            .Body(new MarshallingPayloadBuilder(new TestRequest("Hello Agenix!"))));

        var testCase = runner.GetTestCase();
        Assert.That(testCase.GetActionCount(), Is.EqualTo(1));
        Assert.That(testCase.GetActions()[0], Is.TypeOf<SendMessageAction>());

        var action = (SendMessageAction)testCase.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("send"));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.MessageBuilder, Is.TypeOf<DefaultMessageBuilder>());

        var messageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        Assert.That(messageBuilder.BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
        Assert.That(messageBuilder.BuildMessageHeaders(Context).Count, Is.EqualTo(0));
    }

    [Test]
    public void TestSendBuilderWithPayloadModelExplicitMarshaller()
    {
        // Setup mocks
        _messageEndpoint.Setup(x => x.CreateProducer()).Returns(_messageProducer.Object);

        _messageProducer.Setup(x => x.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, ctx) =>
            {
                Assert.That(message.GetPayload<string>(),
                    Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
            });

        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(SendMessageAction.Builder.Send(_messageEndpoint.Object)
            .Message()
            .Body(new MarshallingPayloadBuilder(new TestRequest("Hello Agenix!"), _marshaller)));

        var testCase = runner.GetTestCase();
        Assert.That(testCase.GetActionCount(), Is.EqualTo(1));
        Assert.That(testCase.GetActions()[0], Is.TypeOf<SendMessageAction>());

        var action = (SendMessageAction)testCase.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("send"));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.MessageBuilder, Is.TypeOf<DefaultMessageBuilder>());

        var messageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        Assert.That(messageBuilder.BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
        Assert.That(messageBuilder.BuildMessageHeaders(Context).Count, Is.EqualTo(0));
    }

    [Test]
    public void TestSendBuilderWithPayloadModelExplicitMarshallerName()
    {
        // Setup mocks
        _messageEndpoint.Setup(x => x.CreateProducer()).Returns(_messageProducer.Object);

        _messageProducer.Setup(x => x.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, ctx) =>
            {
                Assert.That(message.GetPayload<string>(),
                    Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
            });

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());
        _referenceResolver.Setup(x => x.IsResolvable("myMarshaller")).Returns(true);
        _referenceResolver.Setup(x => x.Resolve<IMarshaller>("myMarshaller")).Returns(_marshaller);

        Context.SetReferenceResolver(_referenceResolver.Object);
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(SendMessageAction.Builder.Send(_messageEndpoint.Object)
            .Message()
            .Body(new MarshallingPayloadBuilder(new TestRequest("Hello Agenix!"), "myMarshaller")));

        var testCase = runner.GetTestCase();
        Assert.That(testCase.GetActionCount(), Is.EqualTo(1));
        Assert.That(testCase.GetActions()[0], Is.TypeOf<SendMessageAction>());

        var action = (SendMessageAction)testCase.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("send"));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.MessageBuilder, Is.TypeOf<DefaultMessageBuilder>());

        var messageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        Assert.That(messageBuilder.BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
        Assert.That(messageBuilder.BuildMessageHeaders(Context).Count, Is.EqualTo(0));
    }

    [Test]
    public void TestSendBuilderExtractFromPayload()
    {
        // Setup mocks
        _messageEndpoint.Setup(x => x.CreateProducer()).Returns(_messageProducer.Object);

        _messageProducer.Setup(x => x.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, ctx) =>
            {
                Assert.That(message.GetPayload<string>(),
                    Is.EqualTo("<TestRequest><Message lang=\"ENG\">Hello World!</Message></TestRequest>"));
            });

        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(SendMessageAction.Builder.Send(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message lang=\"ENG\">Hello World!</Message></TestRequest>")
            .Extract(XpathSupport.Xpath()
                .Expression("/TestRequest/Message", "text")
                .Expression("/TestRequest/Message/@lang", "language")
            ));

        Assert.That(Context.GetVariable("text"), Is.Not.Null);
        Assert.That(Context.GetVariable("language"), Is.Not.Null);
        Assert.That(Context.GetVariable("text"), Is.EqualTo("Hello World!"));
        Assert.That(Context.GetVariable("language"), Is.EqualTo("ENG"));

        var testCase = runner.GetTestCase();
        Assert.That(testCase.GetActionCount(), Is.EqualTo(1));
        Assert.That(testCase.GetActions()[0], Is.TypeOf<SendMessageAction>());

        var action = (SendMessageAction)testCase.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("send"));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));

        Assert.That(action.VariableExtractors.Count, Is.EqualTo(1));
        Assert.That(action.VariableExtractors[0], Is.TypeOf<XpathPayloadVariableExtractor>());

        var extractor = (XpathPayloadVariableExtractor)action.VariableExtractors[0];
        Assert.That(extractor.Expressions.ContainsKey("/TestRequest/Message"), Is.True);
        Assert.That(extractor.Expressions.ContainsKey("/TestRequest/Message/@lang"), Is.True);
    }
}
