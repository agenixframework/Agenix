using Agenix.Api.Endpoint;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Core.Actions;
using Agenix.Core.Message;
using Agenix.Core.Message.Builder;
using Agenix.Core.Validation.Builder;
using Agenix.Core.Validation.Xml;
using Moq;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Xml.Tests.Validation.Xhtml;

public class XhtmlXpathMessageValidatorTest : AbstractNUnitSetUp
{
    private readonly Mock<IConsumer> _consumer = new();
    private readonly Mock<IEndpoint> _endpoint = new();
    private readonly Mock<IEndpointConfiguration> _endpointConfiguration = new();

    [Test]
    public void TestXhtmlXpathValidation()
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
        var validationContext = new XpathMessageValidationContext
        {
            XpathExpressions =
            {
                // Set up XPath expressions for validation
                ["/xh:html/xh:head/xh:title"] = "Sample XHTML content", ["//xh:p"] = "Hello TestFramework!"
            },
            Namespaces =
            {
                // Set up namespace mapping
                ["xh"] = "http://www.w3.org/1999/xhtml"
            }
        };

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
            .Validate(validationContext)
            .Type(MessageType.XHTML)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestXhtmlXpathValidationFailed()
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
            "<h1>Hello TestFramework!</h1>" +
            "</body>" +
            "</html>");

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new XpathMessageValidationContext
        {
            XpathExpressions =
            {
                // Set up XPath expression that will fail validation
                ["//xh:h1"] = "Failed!"
            },
            Namespaces =
            {
                // Set up namespace mapping
                ["xh"] = "http://www.w3.org/1999/xhtml"
            }
        };

        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"org/w3/xhtml/xhtml1-strict.dtd\">" +
            "<html xmlns=\"http://www.w3.org/1999/xhtml\">" +
            "<head>" +
            "<title>Sample XHTML content</title>" +
            "</head>" +
            "<body>" +
            "<h1>Hello TestFramework!</h1>" +
            "</body>" +
            "</html>"));

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.XHTML)
            .Validate(validationContext)
            .Build();

        // Assert that ValidationException is thrown
        Assert.Throws<ValidationException>(() => receiveAction.Execute(Context));
    }
}
