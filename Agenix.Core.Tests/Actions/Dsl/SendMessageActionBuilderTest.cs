using System.Collections.Generic;
using Agenix.Api.Endpoint;
using Agenix.Api.IO;
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
using NUnit.Framework.Legacy;
using TestContext = Agenix.Api.Context.TestContext;
using static Agenix.Core.Actions.SendMessageAction.Builder;

namespace Agenix.Core.Tests.Actions.Dsl;

public class SendMessageActionBuilderTest : AbstractNUnitSetUp
{
    private readonly Mock<IReferenceResolver> _referenceResolver = new();
    private readonly IEndpoint messageEndpoint = new Mock<IEndpoint>().Object;
    private readonly IProducer messageProducer = new Mock<IProducer>().Object;
    private readonly IResource resource = new Mock<IResource>().Object;

    private readonly IMessageValidator<IValidationContext> validator =
        new Mock<IMessageValidator<IValidationContext>>().Object;

    protected override TestContextFactory CreateTestContextFactory()
    {
        Mock.Get(validator)
            .Setup(v => v.SupportsMessageType(It.IsAny<string>(), It.IsAny<IMessage>()))
            .Returns(true);

        var factory = base.CreateTestContextFactory();
        factory.MessageValidatorRegistry.AddMessageValidator("validator", validator);

        return factory;
    }

    [Test]
    public void TestSendBuilderWithMessageInstance()
    {
        Mock.Get(messageEndpoint).Reset();
        Mock.Get(messageProducer).Reset();

        Mock.Get(messageEndpoint).Setup(e => e.CreateProducer()).Returns(messageProducer);
        Mock.Get(messageProducer).Setup(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback((IMessage message, TestContext context) =>
            {
                ClassicAssert.AreEqual("Foo", message.GetPayload<string>());
                ClassicAssert.NotNull(message.GetHeader("operation"));
                ClassicAssert.AreEqual("foo", message.GetHeader("operation"));
            });

        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(Send(messageEndpoint)
            .Message(new DefaultMessage("Foo").SetHeader("operation", "foo"))
            .Header("additional", "additionalValue"));

        var test = runner.GetTestCase();
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.AreEqual(typeof(SendMessageAction), test.GetActions()[0].GetType());

        var action = (SendMessageAction)test.GetActions()[0];
        ClassicAssert.AreEqual("send", action.Name);
        ClassicAssert.AreEqual(messageEndpoint, action.Endpoint);
        ClassicAssert.AreEqual(typeof(StaticMessageBuilder), action.MessageBuilder.GetType());

        var messageBuilder = (StaticMessageBuilder)action.MessageBuilder;
        ClassicAssert.AreEqual("Foo", messageBuilder.BuildMessagePayload(Context, action.MessageType));
        ClassicAssert.AreEqual("foo", messageBuilder.BuildMessageHeaders(Context)["operation"]);
        ClassicAssert.AreEqual("additionalValue", messageBuilder.BuildMessageHeaders(Context)["additional"]);
    }

    [Test]
    public void TestSendBuilderWithObjectMessageInstance()
    {
        Mock.Get(messageEndpoint).Reset();
        Mock.Get(messageProducer).Reset();

        Mock.Get(messageEndpoint).Setup(e => e.CreateProducer()).Returns(messageProducer);

        Mock.Get(messageProducer)
            .Setup(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback((IMessage message, TestContext context) =>
            {
                ClassicAssert.AreEqual(10, message.GetPayload<int>());
                ClassicAssert.NotNull(message.GetHeader("operation"));
                ClassicAssert.AreEqual("foo", message.GetHeader("operation"));
            });

        var message = new DefaultMessage(10).SetHeader("operation", "foo");
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(Send(messageEndpoint).Message(message));

        var test = runner.GetTestCase();
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.AreEqual(typeof(SendMessageAction), test.GetActions()[0].GetType());

        var action = (SendMessageAction)test.GetActions()[0];
        ClassicAssert.AreEqual("send", action.Name);
        ClassicAssert.AreEqual(messageEndpoint, action.Endpoint);
        ClassicAssert.AreEqual(typeof(StaticMessageBuilder), action.MessageBuilder.GetType());

        var messageBuilder = (StaticMessageBuilder)action.MessageBuilder;
        ClassicAssert.AreEqual(message.Payload, messageBuilder.BuildMessagePayload(Context, action.MessageType));
        ClassicAssert.AreEqual(1, messageBuilder.BuildMessageHeaders(Context).Count);
        ClassicAssert.AreEqual("foo", messageBuilder.BuildMessageHeaders(Context)["operation"]);
        ClassicAssert.AreEqual(message.GetHeader(MessageHeaders.Id),
            messageBuilder.GetMessage().GetHeader(MessageHeaders.Id));
        ClassicAssert.AreEqual("foo", messageBuilder.GetMessage().GetHeader("operation"));

        var constructed = messageBuilder.Build(new TestContext(), MessageType.PLAINTEXT.ToString());
        ClassicAssert.AreEqual(message.GetHeaders().Count + 1, constructed.GetHeaders().Count);
        ClassicAssert.AreEqual("foo", constructed.GetHeader("operation"));
        ClassicAssert.AreNotEqual(message.GetHeader(MessageHeaders.Id), constructed.GetHeader(MessageHeaders.Id));
    }

    [Test]
    public void TestSendBuilderWithObjectMessageInstanceAdditionalHeader()
    {
        Mock.Get(messageEndpoint).Reset();
        Mock.Get(messageProducer).Reset();

        Mock.Get(messageEndpoint).Setup(e => e.CreateProducer()).Returns(messageProducer);

        Mock.Get(messageProducer)
            .Setup(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback((IMessage message, TestContext context) =>
            {
                ClassicAssert.AreEqual(10, message.GetPayload<int>());
                ClassicAssert.NotNull(message.GetHeader("operation"));
                ClassicAssert.AreEqual("foo", message.GetHeader("operation"));
                ClassicAssert.NotNull(message.GetHeader("additional"));
                ClassicAssert.AreEqual("new", message.GetHeader("additional"));
            });

        var message = new DefaultMessage(10).SetHeader("operation", "foo");
        var runner = new DefaultTestCaseRunner(Context);

        runner.Run(Send(messageEndpoint)
            .Message(message)
            .Header("additional", "new"));

        var test = runner.GetTestCase();
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.AreEqual(typeof(SendMessageAction), test.GetActions()[0].GetType());

        var action = (SendMessageAction)test.GetActions()[0];
        ClassicAssert.AreEqual("send", action.Name);
        ClassicAssert.AreEqual(messageEndpoint, action.Endpoint);
        ClassicAssert.AreEqual(typeof(StaticMessageBuilder), action.MessageBuilder.GetType());

        var messageBuilder = (StaticMessageBuilder)action.MessageBuilder;
        ClassicAssert.AreEqual(message.Payload, messageBuilder.BuildMessagePayload(Context, action.MessageType));
        ClassicAssert.AreEqual(2, messageBuilder.BuildMessageHeaders(Context).Count);
        ClassicAssert.AreEqual("new", messageBuilder.BuildMessageHeaders(Context)["additional"]);
        ClassicAssert.AreEqual("foo", messageBuilder.BuildMessageHeaders(Context)["operation"]);
        ClassicAssert.AreEqual(message.GetHeader(MessageHeaders.Id),
            messageBuilder.GetMessage().GetHeader(MessageHeaders.Id));
        ClassicAssert.AreEqual("foo", messageBuilder.GetMessage().GetHeader("operation"));

        var constructed = messageBuilder.Build(new TestContext(), MessageType.PLAINTEXT.ToString());
        ClassicAssert.AreEqual(message.GetHeaders().Count + 2, constructed.GetHeaders().Count);
        ClassicAssert.AreEqual("foo", constructed.GetHeader("operation"));
        ClassicAssert.AreEqual("new", constructed.GetHeader("additional"));
    }

    [Test]
    public void TestSendBuilderWithPayloadBuilder()
    {
        MessagePayloadBuilder payloadBuilder = context => "<TestRequest><Message>Hello Agenix!</Message></TestRequest>";

        Mock.Get(messageEndpoint).Reset();
        Mock.Get(messageProducer).Reset();

        Mock.Get(messageEndpoint).Setup(e => e.CreateProducer()).Returns(messageProducer);
        Mock.Get(messageProducer)
            .Setup(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback((IMessage message, TestContext context) =>
            {
                ClassicAssert.AreEqual(message.GetPayload<string>(),
                    "<TestRequest><Message>Hello Agenix!</Message></TestRequest>");
            });

        _referenceResolver.Setup(r => r.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(r => r.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(r => r.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(r => r.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());

        Context.SetReferenceResolver(_referenceResolver.Object);

        var runner = new DefaultTestCaseRunner(Context);

        runner.Run(Send(messageEndpoint)
            .Message()
            .Body(payloadBuilder));

        var test = runner.GetTestCase();
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.AreEqual(typeof(SendMessageAction), test.GetActions()[0].GetType());

        var action = (SendMessageAction)test.GetActions()[0];
        ClassicAssert.AreEqual("send", action.Name);
        ClassicAssert.AreEqual(messageEndpoint, action.Endpoint);
        ClassicAssert.AreEqual(action.MessageBuilder.GetType(), typeof(DefaultMessageBuilder));

        var messageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        ClassicAssert.AreEqual(messageBuilder.BuildMessagePayload(Context, action.MessageType),
            "<TestRequest><Message>Hello Agenix!</Message></TestRequest>");
        ClassicAssert.AreEqual(messageBuilder.BuildMessageHeaders(Context).Count, 0L);
    }

    [Test]
    public void TestSendBuilderWithPayloadData()
    {
        Mock.Get(messageEndpoint).Reset();
        Mock.Get(messageProducer).Reset();

        Mock.Get(messageEndpoint).Setup(e => e.CreateProducer()).Returns(messageProducer);
        Mock.Get(messageProducer)
            .Setup(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback((IMessage message, TestContext context) =>
            {
                ClassicAssert.AreEqual(message.GetPayload<string>(),
                    "<TestRequest><Message>Hello Agenix!</Message></TestRequest>");
            });
        var runner = new DefaultTestCaseRunner(Context);

        runner.Run(Send(messageEndpoint)
            .Message()
            .Body("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));

        var test = runner.GetTestCase();
        ClassicAssert.AreEqual(1, test.GetActionCount());
        ClassicAssert.AreEqual(typeof(SendMessageAction), test.GetActions()[0].GetType());

        var action = (SendMessageAction)test.GetActions()[0];
        ClassicAssert.AreEqual("send", action.Name);
        ClassicAssert.AreEqual(messageEndpoint, action.Endpoint);

        ClassicAssert.AreEqual(action.MessageBuilder.GetType(), typeof(DefaultMessageBuilder));

        var messageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        ClassicAssert.AreEqual(messageBuilder.BuildMessagePayload(Context, action.MessageType),
            "<TestRequest><Message>Hello Agenix!</Message></TestRequest>");
        ClassicAssert.AreEqual(messageBuilder.BuildMessageHeaders(Context).Count, 0L);
    }
}
