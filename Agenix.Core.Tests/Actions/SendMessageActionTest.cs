using System.Collections.Generic;
using System.Reflection;
using Agenix.Api.Endpoint;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Core.Actions;
using Agenix.Core.Message;
using Agenix.Core.Message.Builder;
using Agenix.Core.Validation;
using Agenix.Core.Validation.Builder;
using Agenix.Core.Variable;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.Tests.Actions;

public class SendMessageActionTest : AbstractNUnitSetUp
{
    private Mock<IEndpointConfiguration> _endpointConfigurationMock;

    private Mock<IEndpoint> _endpointMock;
    private Mock<IProducer> _producerMock;

    [SetUp]
    public void SetUp()
    {
        _endpointMock = new Mock<IEndpoint>();
        _producerMock = new Mock<IProducer>();
        _endpointConfigurationMock = new Mock<IEndpointConfiguration>();
    }

    [Test]
    public void TestSendMessageWithMessagePayloadData()
    {
        var messageBuilder = new DefaultMessageBuilder();
        messageBuilder.SetPayloadBuilder(
            new DefaultPayloadBuilder("<TestRequest><Message>Hello World!</Message></TestRequest>"));
        var controlMessage = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        // Reset and configure mock behaviors
        _endpointMock.Reset();
        _producerMock.Reset();
        _endpointConfigurationMock.Reset();

        _endpointMock.Setup(e => e.CreateProducer()).Returns(_producerMock.Object);
        _endpointMock.Setup(e => e.EndpointConfiguration).Returns(_endpointConfigurationMock.Object);

        _producerMock
            .Setup(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) => ValidateMessageToSend(message, controlMessage));

        // Build and execute SendMessageAction
        var sendAction = new SendMessageAction.Builder()
            .Endpoint(_endpointMock.Object)
            .Message(messageBuilder)
            .Build();

        sendAction.Execute(Context);

        // Verify interactions
        _producerMock.Verify(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()), Times.Once);
    }

    [Test]
    public void TestSendMessageWithMessagePayloadResource()
    {
        var textPayloadResource =
            $"assembly://{Assembly.GetExecutingAssembly().GetName().Name}/{Assembly.GetExecutingAssembly().GetName().Name}.ResourcesTest.actions/test-request-payload.xml";
        var messageBuilder = new DefaultMessageBuilder();
        messageBuilder.SetPayloadBuilder(new FileResourcePayloadBuilder(textPayloadResource));

        var controlMessage =
            new DefaultMessage(
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<TestRequest>\n    <Message>Hello World!</Message>\n</TestRequest>\n");
        _endpointMock.Reset();
        _producerMock.Reset();
        _endpointConfigurationMock.Reset();

        _endpointMock.Setup(e => e.CreateProducer()).Returns(_producerMock.Object);
        _endpointMock.Setup(e => e.EndpointConfiguration).Returns(_endpointConfigurationMock.Object);

        _producerMock
            .Setup(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) => ValidateMessageToSend(message, controlMessage));


        var sendAction = new SendMessageAction.Builder()
            .Endpoint(_endpointMock.Object)
            .Message(messageBuilder)
            .Build();

        sendAction.Execute(Context);

        _producerMock.Verify(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()), Times.Once);
    }

    [Test]
    public void TestSendMessageWithMessagePayloadDataVariablesSupport()
    {
        var messageBuilder = new DefaultMessageBuilder();
        messageBuilder.SetPayloadBuilder(
            new DefaultPayloadBuilder("<TestRequest><Message>${myText}</Message></TestRequest>"));

        Context.SetVariable("myText", "Hello World!");

        var controlMessage = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        _endpointMock.Reset();
        _producerMock.Reset();
        _endpointConfigurationMock.Reset();

        _endpointMock.Setup(e => e.CreateProducer()).Returns(_producerMock.Object);
        _endpointMock.Setup(e => e.EndpointConfiguration).Returns(_endpointConfigurationMock.Object);

        _producerMock
            .Setup(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) => ValidateMessageToSend(message, controlMessage));

        var sendAction = new SendMessageAction.Builder()
            .Endpoint(_endpointMock.Object)
            .Message(messageBuilder)
            .Build();

        sendAction.Execute(Context);

        _producerMock.Verify(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()), Times.Once);
    }

    [Test]
    public void TestSendMessageWithMessagePayloadResourceVariablesSupport()
    {
        var textPayloadResource =
            $"assembly://{Assembly.GetExecutingAssembly().GetName().Name}/{Assembly.GetExecutingAssembly().GetName().Name}.ResourcesTest.actions/test-request-payload-with-variables.xml";
        var messageBuilder = new DefaultMessageBuilder();
        messageBuilder.SetPayloadBuilder(new FileResourcePayloadBuilder(textPayloadResource));

        Context.SetVariable("myText", "Hello World!");

        var controlMessage =
            new DefaultMessage(
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<TestRequest>\n    <Message>Hello World!</Message>\n</TestRequest>\n");
        _endpointMock.Reset();
        _producerMock.Reset();
        _endpointConfigurationMock.Reset();

        _endpointMock.Setup(e => e.CreateProducer()).Returns(_producerMock.Object);
        _endpointMock.Setup(e => e.EndpointConfiguration).Returns(_endpointConfigurationMock.Object);

        _producerMock
            .Setup(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) => ValidateMessageToSend(message, controlMessage));


        var sendAction = new SendMessageAction.Builder()
            .Endpoint(_endpointMock.Object)
            .Message(messageBuilder)
            .Build();

        sendAction.Execute(Context);

        _producerMock.Verify(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()), Times.Once);
    }

    [Test]
    public void TestSendMessageWithMessagePayloadResourceFunctionsSupport()
    {
        var textPayloadResource =
            $"assembly://{Assembly.GetExecutingAssembly().GetName().Name}/{Assembly.GetExecutingAssembly().GetName().Name}.ResourcesTest.actions/test-request-payload-with-functions.xml";
        var messageBuilder = new DefaultMessageBuilder();
        messageBuilder.SetPayloadBuilder(new FileResourcePayloadBuilder(textPayloadResource));

        var controlMessage =
            new DefaultMessage(
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<TestRequest>\n    <Message>Hello World!</Message>\n</TestRequest>\n");
        _endpointMock.Reset();
        _producerMock.Reset();
        _endpointConfigurationMock.Reset();

        _endpointMock.Setup(e => e.CreateProducer()).Returns(_producerMock.Object);
        _endpointMock.Setup(e => e.EndpointConfiguration).Returns(_endpointConfigurationMock.Object);

        _producerMock
            .Setup(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) => ValidateMessageToSend(message, controlMessage));


        var sendAction = new SendMessageAction.Builder()
            .Endpoint(_endpointMock.Object)
            .Message(messageBuilder)
            .Build();

        sendAction.Execute(Context);

        _producerMock.Verify(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()), Times.Once);
    }

    [Test]
    public void TestSendMessageOverwriteMessageElements()
    {
        var messageBuilder = new DefaultMessageBuilder();
        messageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<TestRequest><Message>?</Message></TestRequest>"));

        var controlMessage = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        _endpointMock.Reset();
        _producerMock.Reset();
        _endpointConfigurationMock.Reset();

        _endpointMock.Setup(e => e.CreateProducer()).Returns(_producerMock.Object);
        _endpointMock.Setup(e => e.EndpointConfiguration).Returns(_endpointConfigurationMock.Object);

        _producerMock
            .Setup(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) => ValidateMessageToSend(message, controlMessage));

        var sendAction = new SendMessageAction.Builder()
            .Endpoint(_endpointMock.Object)
            .Message(messageBuilder)
            .Process(new MessageProcessor())
            .Build();

        sendAction.Execute(Context);

        _producerMock.Verify(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()), Times.Once);
    }

    [Test]
    public void TestSendMessageWithMessageHeaders()
    {
        var messageBuilder = new DefaultMessageBuilder();
        messageBuilder.SetPayloadBuilder(
            new DefaultPayloadBuilder("<TestRequest><Message>Hello World!</Message></TestRequest>"));

        Dictionary<string, object> controlHeaders = new() { ["Operation"] = "sayHello" };
        var controlMessage =
            new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>", controlHeaders);

        Dictionary<string, object> headers = new() { ["Operation"] = "sayHello" };
        messageBuilder.AddHeaderBuilder(new DefaultHeaderBuilder(headers));

        _endpointMock.Reset();
        _producerMock.Reset();
        _endpointConfigurationMock.Reset();

        _endpointMock.Setup(e => e.CreateProducer()).Returns(_producerMock.Object);
        _endpointMock.Setup(e => e.EndpointConfiguration).Returns(_endpointConfigurationMock.Object);

        _producerMock
            .Setup(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) => ValidateMessageToSend(message, controlMessage));

        var sendAction = new SendMessageAction.Builder()
            .Endpoint(_endpointMock.Object)
            .Message(messageBuilder)
            .Build();

        sendAction.Execute(Context);

        _producerMock.Verify(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()), Times.Once);
    }

    [Test]
    public void TestSendMessageWithHeaderValuesVariableSupport()
    {
        var messageBuilder = new DefaultMessageBuilder();
        messageBuilder.SetPayloadBuilder(
            new DefaultPayloadBuilder("<TestRequest><Message>Hello World!</Message></TestRequest>"));

        Context.SetVariable("myOperation", "sayHello");

        Dictionary<string, object> controlHeaders = new() { ["Operation"] = "sayHello" };
        var controlMessage =
            new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>", controlHeaders);

        Dictionary<string, object> headers = new() { ["Operation"] = "${myOperation}" };
        messageBuilder.AddHeaderBuilder(new DefaultHeaderBuilder(headers));

        _endpointMock.Reset();
        _producerMock.Reset();
        _endpointConfigurationMock.Reset();

        _endpointMock.Setup(e => e.CreateProducer()).Returns(_producerMock.Object);
        _endpointMock.Setup(e => e.EndpointConfiguration).Returns(_endpointConfigurationMock.Object);

        _producerMock
            .Setup(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) => ValidateMessageToSend(message, controlMessage));

        var sendAction = new SendMessageAction.Builder()
            .Endpoint(_endpointMock.Object)
            .Message(messageBuilder)
            .Build();

        sendAction.Execute(Context);

        _producerMock.Verify(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()), Times.Once);
    }

    [Test]
    public void TestSendMessageWithUnknownVariableInMessagePayload()
    {
        var messageBuilder = new DefaultMessageBuilder();
        messageBuilder.SetPayloadBuilder(
            new DefaultPayloadBuilder("<TestRequest><Message>${myText}</Message></TestRequest>"));

        _endpointMock.Reset();
        _producerMock.Reset();
        _endpointConfigurationMock.Reset();

        _endpointMock.Setup(e => e.CreateProducer()).Returns(_producerMock.Object);
        _endpointMock.Setup(e => e.EndpointConfiguration).Returns(_endpointConfigurationMock.Object);

        var sendAction = new SendMessageAction.Builder()
            .Endpoint(_endpointMock.Object)
            .Message(messageBuilder)
            .Build();

        Assert.Throws<AgenixSystemException>(() => sendAction.Execute(Context), "Unknown variable 'myText'");
    }

    [Test]
    public void TestSendMessageWithUnknownVariableInHeaders()
    {
        var messageBuilder = new DefaultMessageBuilder();
        messageBuilder.SetPayloadBuilder(
            new DefaultPayloadBuilder("<TestRequest><Message>Hello World!</Message></TestRequest>"));

        var headers = new Dictionary<string, object> { { "Operation", "${myOperation}" } };
        messageBuilder.AddHeaderBuilder(new DefaultHeaderBuilder(headers));

        _endpointMock.Reset();
        _producerMock.Reset();
        _endpointConfigurationMock.Reset();

        _endpointMock.Setup(e => e.CreateProducer()).Returns(_producerMock.Object);
        _endpointMock.Setup(e => e.EndpointConfiguration).Returns(_endpointConfigurationMock.Object);

        var sendAction = new SendMessageAction.Builder()
            .Endpoint(_endpointMock.Object)
            .Message(messageBuilder)
            .Build();

        var ex = Assert.Throws<AgenixSystemException>(() => sendAction.Execute(Context));
        Assert.That(ex.InnerException.Message, Is.EqualTo("Unknown variable 'myOperation'"));
    }

    [Test]
    public void TestSendMessageWithExtractHeaderValues()
    {
        var messageBuilder = new DefaultMessageBuilder();
        messageBuilder.SetPayloadBuilder(
            new DefaultPayloadBuilder("<TestRequest><Message>Hello World!</Message></TestRequest>"));

        var controlHeaders = new Dictionary<string, object> { { "Operation", "sayHello" } };
        var controlMessage =
            new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>", controlHeaders);

        var headers = new Dictionary<string, object> { { "Operation", "sayHello" } };
        messageBuilder.AddHeaderBuilder(new DefaultHeaderBuilder(headers));

        var extractVars = new Dictionary<string, string>
        {
            { "Operation", "myOperation" }, { MessageHeaders.Id, "correlationId" }
        };

        var variableExtractor = new MessageHeaderVariableExtractor.Builder()
            .Headers(extractVars)
            .Build();

        _endpointMock.Reset();
        _producerMock.Reset();
        _endpointConfigurationMock.Reset();

        _endpointMock.Setup(e => e.CreateProducer()).Returns(_producerMock.Object);
        _endpointMock.Setup(e => e.EndpointConfiguration).Returns(_endpointConfigurationMock.Object);

        _producerMock.Setup(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) =>
            {
                ClassicAssert.AreEqual(controlMessage.GetPayload<string>(), message.GetPayload<string>());
                ClassicAssert.AreEqual(controlMessage.GetHeaders()["Operation"], message.GetHeaders()["Operation"]);
            });

        var sendAction = new SendMessageAction.Builder()
            .Endpoint(_endpointMock.Object)
            .Process(variableExtractor)
            .Message(messageBuilder)
            .Build();

        sendAction.Execute(Context);

        Assert.That(Context.GetVariable("myOperation"), Is.EqualTo("sayHello"));
        Assert.That(Context.GetVariable("correlationId"), Is.Not.Null);
    }

    [Test]
    public void TestMissingMessagePayload()
    {
        _endpointMock.Reset();
        _producerMock.Reset();
        _endpointConfigurationMock.Reset();

        _endpointMock.Setup(e => e.CreateProducer()).Returns(_producerMock.Object);
        _endpointMock.Setup(e => e.EndpointConfiguration).Returns(_endpointConfigurationMock.Object);

        _producerMock.Setup(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) =>
            {
                ClassicAssert.AreEqual("", message.GetPayload<string>());
            });

        var sendAction = new SendMessageAction.Builder()
            .Endpoint(_endpointMock.Object)
            .Message(new DefaultMessageBuilder())
            .Build();

        sendAction.Execute(Context);

        _producerMock.Verify(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()), Times.Once);
    }

    [Test]
    public void TestSendMessageWithUtf16Encoding()
    {
        var messageBuilder = new DefaultMessageBuilder();
        messageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<?xml version=\"1.0\" encoding=\"UTF-16\"?><TestRequest><Message>Hello World!</Message></TestRequest>"));

        var controlMessage =
            new DefaultMessage(
                "<?xml version=\"1.0\" encoding=\"UTF-16\"?><TestRequest><Message>Hello World!</Message></TestRequest>");

        _endpointMock.Reset();
        _producerMock.Reset();
        _endpointConfigurationMock.Reset();

        _endpointMock.Setup(e => e.CreateProducer()).Returns(_producerMock.Object);
        _endpointMock.Setup(e => e.EndpointConfiguration).Returns(_endpointConfigurationMock.Object);

        _producerMock.Setup(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) =>
            {
                ClassicAssert.AreEqual(controlMessage.GetPayload<string>(), message.GetPayload<string>());
            });

        var sendAction = new SendMessageAction.Builder()
            .Endpoint(_endpointMock.Object)
            .Message(messageBuilder)
            .Build();

        sendAction.Execute(Context);

        _producerMock.Verify(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()), Times.Once);
    }

    [Test]
    public void TestSendMessageWithIsoEncoding()
    {
        var messageBuilder = new DefaultMessageBuilder();
        messageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><TestRequest><Message>Hello World!</Message></TestRequest>"));

        var controlMessage =
            new DefaultMessage(
                "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?><TestRequest><Message>Hello World!</Message></TestRequest>");

        _endpointMock.Reset();
        _producerMock.Reset();
        _endpointConfigurationMock.Reset();

        _endpointMock.Setup(e => e.CreateProducer()).Returns(_producerMock.Object);
        _endpointMock.Setup(e => e.EndpointConfiguration).Returns(_endpointConfigurationMock.Object);

        _producerMock.Setup(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) =>
            {
                ClassicAssert.AreEqual(controlMessage.GetPayload<string>(), message.GetPayload<string>());
            });

        var sendAction = new SendMessageAction.Builder()
            .Endpoint(_endpointMock.Object)
            .Message(messageBuilder)
            .Build();

        sendAction.Execute(Context);

        _producerMock.Verify(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()), Times.Once);
    }

    [Test]
    public void TestSendMessageWithMessagePayloadResourceIsoEncoding()
    {
        var textPayloadResource =
            $"assembly://{Assembly.GetExecutingAssembly().GetName().Name}/{Assembly.GetExecutingAssembly().GetName().Name}.ResourcesTest.actions/test-request-iso-encoding.xml";
        var messageBuilder = new DefaultMessageBuilder();
        messageBuilder.SetPayloadBuilder(new FileResourcePayloadBuilder(textPayloadResource));

        var controlMessage =
            new DefaultMessage(
                "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>\n<TestRequest>\n    <Message>Hello World!</Message>\n</TestRequest>\n");
        _endpointMock.Reset();
        _producerMock.Reset();
        _endpointConfigurationMock.Reset();

        _endpointMock.Setup(e => e.CreateProducer()).Returns(_producerMock.Object);
        _endpointMock.Setup(e => e.EndpointConfiguration).Returns(_endpointConfigurationMock.Object);

        _producerMock
            .Setup(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) => ValidateMessageToSend(message, controlMessage));


        var sendAction = new SendMessageAction.Builder()
            .Endpoint(_endpointMock.Object)
            .Message(messageBuilder)
            .Build();

        sendAction.Execute(Context);

        _producerMock.Verify(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()), Times.Once);
    }


    /// <summary>
    ///     Validates that the message to be sent matches the control message in payload and headers.
    /// </summary>
    /// <param name="toSend">The message to be sent.</param>
    /// <param name="controlMessage">The control message to validate against.</param>
    private void ValidateMessageToSend(IMessage toSend, IMessage controlMessage)
    {
        // Ensure the payloads are equivalent after normalization and trimming
        ClassicAssert.AreEqual(
            DefaultTextEqualsMessageValidator.NormalizeLineEndings(controlMessage.GetPayload<string>().Trim()),
            DefaultTextEqualsMessageValidator.NormalizeLineEndings(toSend.GetPayload<string>().Trim())
        );

        // Create and use a DefaultMessageHeaderValidator to validate headers
        var validator = new DefaultMessageHeaderValidator();
        var validationContext = new HeaderValidationContext();
        validator.ValidateMessage(toSend, controlMessage, Context, validationContext);
    }

    private class MessageProcessor : IMessageProcessor
    {
        public void Process(IMessage message, TestContext context)
        {
            message.Payload = message.GetPayload<string>().Replace("?", "Hello World!");
        }
    }
}
