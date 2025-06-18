using Agenix.Api.Endpoint;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Api.Validation.Context;
using Agenix.Core.Actions;
using Agenix.Core.Message;
using Agenix.Core.Message.Builder;
using Agenix.Core.Validation;
using Agenix.Core.Validation.Builder;
using Agenix.Core.Validation.Xml;
using Agenix.Validation.Xml.Validation.Xml;
using Moq;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Xml.Tests.Validation.Xml;

public class SendMessageActionTest : AbstractNUnitSetUp
{
    private readonly Mock<IEndpoint> _endpoint = new();
    private readonly Mock<IEndpointConfiguration> _endpointConfiguration = new();
    private readonly Mock<IProducer> _producer = new();


    [Test]
    public void TestSendMessageOverwriteMessageElementsXPath()
    {
        var messageBuilder = new DefaultMessageBuilder();
        messageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><TestRequest><Message>?</Message></TestRequest>"));

        var overwriteElements = new Dictionary<string, object> { { "/TestRequest/Message", "Hello World!" } };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(overwriteElements)
            .Build();

        var controlMessage =
            new DefaultMessage(
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?><TestRequest><Message>Hello World!</Message></TestRequest>");

        // Reset mocks
        _endpoint.Reset();
        _producer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateProducer()).Returns(_producer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);

        _producer
            .Setup(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) =>
            {
                ValidateMessageToSend(message, controlMessage);
            });

        var sendAction = new SendMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(messageBuilder)
            .Type(MessageType.XML)
            .Process(processor)
            .Build();

        sendAction.Execute(Context);
    }

    [Test]
    public void TestSendMessageOverwriteMessageElementsDotNotation()
    {
        var messageBuilder = new DefaultMessageBuilder();
        messageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("<TestRequest><Message>?</Message></TestRequest>"));

        var overwriteElements = new Dictionary<string, object> { { "TestRequest.Message", "Hello World!" } };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(overwriteElements)
            .Build();

        var controlMessage = new DefaultMessage("""
                                                    <?xml version="1.0" encoding="UTF-8"?>
                                                    <TestRequest>
                                                        <Message>Hello World!</Message>
                                                    </TestRequest>
                                                """);

        // Reset mocks
        _endpoint.Reset();
        _producer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateProducer()).Returns(_producer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);

        _producer
            .Setup(p => p.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) =>
            {
                ValidateMessageToSend(message, controlMessage);
            });

        var sendAction = new SendMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(messageBuilder)
            .Type(MessageType.XML)
            .Process(processor)
            .Build();

        sendAction.Execute(Context);
    }

    private void ValidateMessageToSend(IMessage toSend, IMessage controlMessage)
    {
        XmlMessageValidationContext xmlValidationContext = new();
        new DomXmlMessageValidator().ValidateMessage(toSend, controlMessage, Context, [xmlValidationContext]);

        var validator = new DefaultMessageHeaderValidator();
        validator.ValidateMessage(toSend, controlMessage, Context, new HeaderValidationContext.Builder().Build());
    }
}
