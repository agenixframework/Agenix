using Agenix.Api.Endpoint;
using Agenix.Api.Exceptions;
using Agenix.Api.Messaging;
using Agenix.Core.Actions;
using Agenix.Core.Message;
using Agenix.Core.Message.Builder;
using Agenix.Core.Validation.Builder;
using Agenix.Core.Validation.Xml;
using Moq;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Xml.Tests;

public class IgnoreElementsTest : AbstractNUnitSetUp
{
    private readonly Mock<IConsumer> _consumer = new();
    private readonly Mock<IEndpoint> _endpoint = new();
    private readonly Mock<IEndpointConfiguration> _endpointConfiguration = new();

    protected override TestContext CreateTestContext()
    {
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        var message = new DefaultMessage(
            "<root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-elementA attribute='A'>text-value</sub-elementA>" +
            "<sub-elementB attribute='B'>text-value</sub-elementB>" +
            "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        );

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>())).Returns(message);
        return base.CreateTestContext();
    }

    [Test]
    public void TestIgnoreElements()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-elementA attribute='A'>no validation</sub-elementA>" +
            "<sub-elementB attribute='B'>no validation</sub-elementB>" +
            "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        ));

        var ignoreMessageElements = new HashSet<string>
        {
            "//root/element/sub-elementA",
            "//sub-elementB"
        };

        var validationContext = new XmlMessageValidationContext.Builder()
            .Ignore(ignoreMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestIgnoreNodeListElements()
    {
        _consumer.Reset();

        var message = new DefaultMessage(
            "<root>" +
            "<element>" +
            "<sub-element attribute='A'>text-value</sub-element>" +
            "<sub-element attribute='B'>text-value</sub-element>" +
            "<sub-element attribute='C'>text-value</sub-element>" +
            "</element>" +
            "</root>"
        );

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<root>" +
            "<element>" +
            "<sub-element attribute='A'>no validation</sub-element>" +
            "<sub-element attribute='B'>no validation</sub-element>" +
            "<sub-element attribute='C'>no validation</sub-element>" +
            "</element>" +
            "</root>"
        ));

        var ignoreMessageElements = new HashSet<string>
        {
            "//sub-element"
        };

        var validationContext = new XmlMessageValidationContext.Builder()
            .Ignore(ignoreMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestIgnoreMultipleElements()
    {
        _consumer.Reset();

        var message = new DefaultMessage(
            "<root>" +
            "<element>" +
                "<sub-element attribute='A'>text-value</sub-element>" +
                "<sub-element attribute='B'>text-value</sub-element>" +
                "<sub-element attribute='C'>text-value</sub-element>" +
            "</element>" +
            "</root>"
        );

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<root>" +
            "<element>" +
                "<sub-element attribute='wrong'>no validation</sub-element>" +
                "<sub-element attribute='B'>text-value</sub-element>" +
                "<sub-element attribute='wrong'>no validation</sub-element>" +
            "</element>" +
            "</root>"
        ));

        var ignoreMessageElements = new HashSet<string>
        {
            "//sub-element[1]",
            "//sub-element[3]"
        };

        var validationContext = new XmlMessageValidationContext.Builder()
            .Ignore(ignoreMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestIgnoreAllElements()
    {
        _consumer.Reset();

        var message = new DefaultMessage(
            "<root>" +
            "<element>" +
                "<another-element attribute='Z'>text-value</another-element>" +
                "<sub-element attribute='A'>text-value</sub-element>" +
                "<sub-element attribute='B'>text-value</sub-element>" +
                "<sub-element attribute='C'>text-value</sub-element>" +
                "<another-element attribute='Z'>text-value</another-element>" +
            "</element>" +
            "</root>"
        );

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<root>" +
            "<element>" +
                "<another-element>no validation</another-element>" +
                "<sub-element attribute='wrong'>no validation</sub-element>" +
                "<sub-element attribute='wrong'>no validation</sub-element>" +
                "<sub-element attribute='wrong'>no validation</sub-element>" +
            "</element>" +
            "</root>"
        ));

        var ignoreMessageElements = new HashSet<string>
        {
            "/*"
        };

        var validationContext = new XmlMessageValidationContext.Builder()
            .Ignore(ignoreMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestIgnoreAttributes()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                "<sub-elementA attribute='no validation'>text-value</sub-elementA>" +
                "<sub-elementB attribute='no validation'>text-value</sub-elementB>" +
                "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        ));

        var ignoreMessageElements = new HashSet<string>
        {
            "//root/element/sub-elementA/@attribute",
            "//sub-elementB/@attribute"
        };

        var validationContext = new XmlMessageValidationContext.Builder()
            .Ignore(ignoreMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestIgnoreAttributesAll()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                "<sub-elementA attribute='no validation'>text-value</sub-elementA>" +
                "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        ));

        var ignoreMessageElements = new HashSet<string>
        {
            "//@attribute"
        };

        var validationContext = new XmlMessageValidationContext.Builder()
            .Ignore(ignoreMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestIgnoreAttributesUsingArrays()
    {
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        var message = new DefaultMessage(
            "<root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                "<sub-element attribute='A'>text-value</sub-element>" +
                "<sub-element attribute='B'>text-value</sub-element>" +
                "<sub-element attribute='C'>text-value</sub-element>" +
            "</element>" +
            "</root>"
        );

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                "<sub-element attribute='no validation'>text-value</sub-element>" +
                "<sub-element attribute='no validation'>text-value</sub-element>" +
                "<sub-element attribute='C'>text-value</sub-element>" +
            "</element>" +
            "</root>"
        ));

        var ignoreMessageElements = new HashSet<string>
        {
            "//sub-element[1]/@attribute",
            "//sub-element[2]/@attribute"
        };

        var validationContext = new XmlMessageValidationContext.Builder()
            .Ignore(ignoreMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestIgnoreRootElement()
    {
        _endpoint.Reset();
        _consumer.Reset();
        _endpointConfiguration.Reset();

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        var message = new DefaultMessage(
            "<root>" +
            "<element>Text</element>" +
            "</root>"
        );

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<root>" +
            "<element additonal-attribute='some'>Wrong text</element>" +
            "</root>"
        ));

        var ignoreMessageElements = new HashSet<string>
        {
            "//root"
        };

        var validationContext = new XmlMessageValidationContext.Builder()
            .Ignore(ignoreMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestIgnoreElementsAndValidate()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                "<sub-elementA attribute='A'>no validation</sub-elementA>" +
                "<sub-elementB attribute='B'>no validation</sub-elementB>" +
                "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        ));

        var ignoreMessageElements = new HashSet<string>
        {
            "//root/element/sub-elementA",
            "//sub-elementB"
        };

        var validateElements = new Dictionary<string, object>
        {
            { "//root/element/sub-elementA", "wrong value" },
            { "//sub-elementB", "wrong value" }
        };

        var validationContext = new XpathMessageValidationContext.Builder()
            .Ignore(ignoreMessageElements)
            .Expressions(validateElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestIgnoreElementsByPlaceholder()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-elementA attribute='A'>@Ignore@</sub-elementA>" +
            "<sub-elementB attribute='B'> @Ignore@ </sub-elementB>" +
            "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        ));

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestIgnoreSubElementsByPlaceholder()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>@Ignore@</element>" +
            "</root>"
        ));

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestIgnoreAttributesByPlaceholder()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                "<sub-elementA attribute='@Ignore@'>text-value</sub-elementA>" +
                "<sub-elementB attribute=' @Ignore@ '>text-value</sub-elementB>" +
                "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        ));

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestIgnoreElementsNoMatch()
    { var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
                "<sub-elementA attribute='A'>text-value</sub-elementA>" +
                "<sub-elementB attribute='B'>text-value</sub-elementB>" +
                "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        ));

        var ignoreMessageElements = new HashSet<string>
        {
            "//something-else"
        };

        var validationContext = new XmlMessageValidationContext.Builder()
            .Ignore(ignoreMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        Assert.Throws<AgenixSystemException>(() => receiveAction.Execute(Context), "No result for XPath expression: '//something-else'");
    }
}
