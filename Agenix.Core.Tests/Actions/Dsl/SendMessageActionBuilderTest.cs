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

using System.Collections.Concurrent;
using System.Threading.Tasks;
using Agenix.Api.Endpoint;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Api.Report;
using Agenix.Api.Spi;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Core.Actions;
using Agenix.Core.Container;
using Agenix.Core.Message;
using Agenix.Core.Message.Builder;
using Agenix.Core.Validation.Builder;
using Moq;
using NUnit.Framework;
using static Agenix.Core.Actions.SendMessageAction.Builder;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.Tests.Actions.Dsl;

public class SendMessageActionBuilderTest : AbstractNUnitSetUp
{
    private readonly IEndpoint _messageEndpoint = new Mock<IEndpoint>().Object;
    private readonly IProducer _messageProducer = new Mock<IProducer>().Object;
    private readonly Mock<IReferenceResolver> _referenceResolver = new();

    private readonly IMessageValidator<IValidationContext> _validator =
        new Mock<IMessageValidator<IValidationContext>>().Object;

    protected override TestContextFactory CreateTestContextFactory()
    {
        Mock.Get(_validator)
            .Setup(v => v.SupportsMessageType(It.IsAny<string>(), It.IsAny<IMessage>()))
            .Returns(true);

        var factory = base.CreateTestContextFactory();
        factory.MessageValidatorRegistry.AddMessageValidator("validator", _validator);

        return factory;
    }

    [Test]
    public async Task TestSendBuilderWithMessageInstance()
    {
        Mock.Get(_messageEndpoint).Reset();
        Mock.Get(_messageProducer).Reset();

        Mock.Get(_messageEndpoint).Setup(e => e.CreateProducer()).Returns(_messageProducer);
        Mock.Get(_messageProducer).Setup(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback((IMessage message, TestContext _) =>
            {
                Assert.That(message.GetPayload<string>(), Is.EqualTo("Foo"));
                Assert.That(message.GetHeader("operation"), Is.Not.Null);
                Assert.That(message.GetHeader("operation"), Is.EqualTo("foo"));
            });

        var runner = new DefaultTestCaseRunner(Context);
        await runner.Run(Send(_messageEndpoint)
            .Message(new DefaultMessage("Foo").SetHeader("operation", "foo"))
            .Header("additional", "additionalValue"));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0], Is.TypeOf<SendMessageAction>());

        var action = (SendMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("send"));
        Assert.That(action.Endpoint, Is.SameAs(_messageEndpoint));
        Assert.That(action.MessageBuilder, Is.TypeOf<StaticMessageBuilder>());

        var messageBuilder = (StaticMessageBuilder)action.MessageBuilder;
        Assert.That(messageBuilder.BuildMessagePayload(Context, action.MessageType), Is.EqualTo("Foo"));
        Assert.That(messageBuilder.BuildMessageHeaders(Context)["operation"], Is.EqualTo("foo"));
        Assert.That(messageBuilder.BuildMessageHeaders(Context)["additional"], Is.EqualTo("additionalValue"));
    }

    [Test]
    public async Task TestSendBuilderWithObjectMessageInstance()
    {
        Mock.Get(_messageEndpoint).Reset();
        Mock.Get(_messageProducer).Reset();

        Mock.Get(_messageEndpoint).Setup(e => e.CreateProducer()).Returns(_messageProducer);

        Mock.Get(_messageProducer)
            .Setup(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback((IMessage message, TestContext _) =>
            {
                Assert.That(message.GetPayload<int>(), Is.EqualTo(10));
                Assert.That(message.GetHeader("operation"), Is.Not.Null);
                Assert.That(message.GetHeader("operation"), Is.EqualTo("foo"));
            });

        var message = new DefaultMessage(10).SetHeader("operation", "foo");
        var runner = new DefaultTestCaseRunner(Context);
        await runner.Run(Send(_messageEndpoint).Message(message));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0], Is.TypeOf<SendMessageAction>());

        var action = (SendMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("send"));
        Assert.That(action.Endpoint, Is.SameAs(_messageEndpoint));
        Assert.That(action.MessageBuilder, Is.TypeOf<StaticMessageBuilder>());

        var messageBuilder = (StaticMessageBuilder)action.MessageBuilder;
        Assert.That(messageBuilder.BuildMessagePayload(Context, action.MessageType), Is.EqualTo(message.Payload));
        Assert.That(messageBuilder.BuildMessageHeaders(Context).Count, Is.EqualTo(1));
        Assert.That(messageBuilder.BuildMessageHeaders(Context)["operation"], Is.EqualTo("foo"));
        Assert.That(messageBuilder.GetMessage().GetHeader(MessageHeaders.Id),
            Is.EqualTo(message.GetHeader(MessageHeaders.Id)));
        Assert.That(messageBuilder.GetMessage().GetHeader("operation"), Is.EqualTo("foo"));

        var constructed = messageBuilder.Build(new TestContext(), nameof(MessageType.PLAINTEXT));
        Assert.That(constructed.GetHeaders().Count, Is.EqualTo(message.GetHeaders().Count + 1));
        Assert.That(constructed.GetHeader("operation"), Is.EqualTo("foo"));
        Assert.That(constructed.GetHeader(MessageHeaders.Id),
            Is.Not.EqualTo(message.GetHeader(MessageHeaders.Id)));
    }

    [Test]
    public async Task TestSendBuilderWithObjectMessageInstanceAdditionalHeader()
    {
        Mock.Get(_messageEndpoint).Reset();
        Mock.Get(_messageProducer).Reset();

        Mock.Get(_messageEndpoint).Setup(e => e.CreateProducer()).Returns(_messageProducer);

        Mock.Get(_messageProducer)
            .Setup(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback((IMessage message, TestContext _) =>
            {
                Assert.That(message.GetPayload<int>(), Is.EqualTo(10));
                Assert.That(message.GetHeader("operation"), Is.Not.Null);
                Assert.That(message.GetHeader("operation"), Is.EqualTo("foo"));
                Assert.That(message.GetHeader("additional"), Is.Not.Null);
                Assert.That(message.GetHeader("additional"), Is.EqualTo("new"));
            });

        var message = new DefaultMessage(10).SetHeader("operation", "foo");
        var runner = new DefaultTestCaseRunner(Context);

        await runner.Run(Send(_messageEndpoint)
            .Message(message)
            .Header("additional", "new"));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0], Is.TypeOf<SendMessageAction>());

        var action = (SendMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("send"));
        Assert.That(action.Endpoint, Is.SameAs(_messageEndpoint));
        Assert.That(action.MessageBuilder, Is.TypeOf<StaticMessageBuilder>());

        var messageBuilder = (StaticMessageBuilder)action.MessageBuilder;
        Assert.That(messageBuilder.BuildMessagePayload(Context, action.MessageType), Is.EqualTo(message.Payload));
        Assert.That(messageBuilder.BuildMessageHeaders(Context).Count, Is.EqualTo(2));
        Assert.That(messageBuilder.BuildMessageHeaders(Context)["additional"], Is.EqualTo("new"));
        Assert.That(messageBuilder.BuildMessageHeaders(Context)["operation"], Is.EqualTo("foo"));
        Assert.That(messageBuilder.GetMessage().GetHeader(MessageHeaders.Id),
            Is.EqualTo(message.GetHeader(MessageHeaders.Id)));
        Assert.That(messageBuilder.GetMessage().GetHeader("operation"), Is.EqualTo("foo"));

        var constructed = messageBuilder.Build(new TestContext(), nameof(MessageType.PLAINTEXT));
        Assert.That(constructed.GetHeaders().Count, Is.EqualTo(message.GetHeaders().Count + 2));
        Assert.That(constructed.GetHeader("operation"), Is.EqualTo("foo"));
        Assert.That(constructed.GetHeader("additional"), Is.EqualTo("new"));
    }

    [Test]
    public async Task TestSendBuilderWithPayloadBuilder()
    {
        Mock.Get(_messageEndpoint).Reset();
        Mock.Get(_messageProducer).Reset();

        Mock.Get(_messageEndpoint).Setup(e => e.CreateProducer()).Returns(_messageProducer);
        Mock.Get(_messageProducer)
            .Setup(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback((IMessage message, TestContext _) =>
            {
                Assert.That(message.GetPayload<string>(),
                    Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
            });

        _referenceResolver.Setup(r => r.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(r => r.Resolve<AsyncTestActionListeners>()).Returns(new AsyncTestActionListeners());
        _referenceResolver.Setup(r => r.ResolveAll<SequenceBeforeTest>())
            .Returns(new ConcurrentDictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(r => r.ResolveAll<SequenceAfterTest>())
            .Returns(new ConcurrentDictionary<string, SequenceAfterTest>());

        Context.SetReferenceResolver(_referenceResolver.Object);

        var runner = new DefaultTestCaseRunner(Context);

        await runner.Run(Send(_messageEndpoint)
            .Message()
            .Body(PayloadBuilder));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0], Is.TypeOf<SendMessageAction>());

        var action = (SendMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("send"));
        Assert.That(action.Endpoint, Is.SameAs(_messageEndpoint));
        Assert.That(action.MessageBuilder, Is.TypeOf<DefaultMessageBuilder>());

        var messageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        Assert.That(messageBuilder.BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
        Assert.That(messageBuilder.BuildMessageHeaders(Context).Count, Is.EqualTo(0));
        return;

        object PayloadBuilder(TestContext _)
        {
            return "<TestRequest><Message>Hello Agenix!</Message></TestRequest>";
        }
    }

    [Test]
    public async Task TestSendBuilderWithPayloadData()
    {
        Mock.Get(_messageEndpoint).Reset();
        Mock.Get(_messageProducer).Reset();

        Mock.Get(_messageEndpoint).Setup(e => e.CreateProducer()).Returns(_messageProducer);
        Mock.Get(_messageProducer)
            .Setup(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback((IMessage message, TestContext _) =>
            {
                Assert.That(message.GetPayload<string>(),
                    Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
            });
        var runner = new DefaultTestCaseRunner(Context);

        await runner.Run(Send(_messageEndpoint)
            .Message()
            .Body("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0], Is.TypeOf<SendMessageAction>());

        var action = (SendMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("send"));
        Assert.That(action.Endpoint, Is.SameAs(_messageEndpoint));

        Assert.That(action.MessageBuilder, Is.TypeOf<DefaultMessageBuilder>());

        var messageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        Assert.That(messageBuilder.BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
        Assert.That(messageBuilder.BuildMessageHeaders(Context).Count, Is.EqualTo(0));
    }
}
