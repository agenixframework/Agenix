using Agenix.Api.Endpoint;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Core.Actions;
using Agenix.Core.Message;
using Agenix.Core.Message.Builder;
using Agenix.Core.Validation.Builder;
using Moq;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Xml.Tests.Validation.Xhtml;

public class XhtmlMessageValidatorTest : AbstractNUnitSetUp
{
    private readonly Mock<IConsumer> _consumer = new();
    private readonly Mock<IEndpoint> _endpoint = new();
    private readonly Mock<IEndpointConfiguration> _endpointConfiguration = new();

    [Test]
    public void TestXhtmlConversion()
    {
        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        var message = new DefaultMessage(
            "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">" +
            "<html xmlns=\"http://www.w3.org/1999/xhtml\">" +
            "<head>" +
            "<title></title>" +
            "</head>" +
            "<body>" +
            "<p>Hello TestFramework!</p>" +
            "<hr />" +
            "<form action=\"/\">" +
            "<input name=\"foo\" type=\"text\" />" +
            "</form>" +
            "</body>" +
            "</html>");

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">" +
            "<html xmlns=\"http://www.w3.org/1999/xhtml\">" +
            "<head>" +
            "<title></title>" +
            "</head>" +
            "<body>" +
            "<p>Hello TestFramework!</p>" +
            "<hr />" +
            "<form action=\"/\">" +
            "<input name=\"foo\" type=\"text\" />" +
            "</form>" +
            "</body>" +
            "</html>"));

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.XHTML)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestXhtmlValidation()
    {
        // Reset mocks
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        // Setup mock behavior
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        var message = new DefaultMessage(
            "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"org/w3/xhtml/xhtml1-strict.dtd\">" +
            "<html xmlns=\"http://www.w3.org/1999/xhtml\">" +
            "<head>" +
            "<title>Sample XHTML content</title>" +
            "</head>" +
            "<body>" +
            "<p>Hello TestFramework!</p>" +
            "<form action=\"/\">" +
            "<input name=\"foo\" type=\"text\" />" +
            "</form>" +
            "</body>" +
            "</html>");

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"org/w3/xhtml/xhtml1-strict.dtd\">" +
            "<html xmlns=\"http://www.w3.org/1999/xhtml\">" +
            "<head>" +
            "<title>Sample XHTML content</title>" +
            "</head>" +
            "<body>" +
            "<p>Hello TestFramework!</p>" +
            "<form action=\"/\">" +
            "<input name=\"foo\" type=\"text\" />" +
            "</form>" +
            "</body>" +
            "</html>"));

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.XHTML)
            .Build();

        receiveAction.Execute(Context);
    }
}
