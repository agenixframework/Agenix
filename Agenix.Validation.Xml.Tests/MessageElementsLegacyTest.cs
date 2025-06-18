using Agenix.Api.Endpoint;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Core.Actions;
using Agenix.Core.Message;
using Agenix.Core.Message.Builder;
using Agenix.Core.Validation.Builder;
using Agenix.Core.Validation.Xml;
using Agenix.Validation.Xml.Validation.Xml;
using Moq;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Xml.Tests;

public class MessageElementsLegacyTest : AbstractNUnitSetUp
{
    private Mock<IConsumer> _consumer;
    private Mock<IEndpoint> _endpoint;
    private Mock<IEndpointConfiguration> _endpointConfiguration;

    [SetUp]
    public void SetUp()
    {
        _endpoint = new Mock<IEndpoint>();
        _consumer = new Mock<IConsumer>();
        _endpointConfiguration = new Mock<IEndpointConfiguration>();
    }

    [Test]
    public void TestValidateMessageElements()
    {
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

        var validateMessageElements = new Dictionary<string, object>
        {
            { "root.element.sub-elementA", "text-value" }, { "sub-elementB", "text-value" }
        };

        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        Assert.DoesNotThrow(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestValidateEmptyMessageElements()
    {
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        var message = new DefaultMessage(
            "<root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-elementA attribute='A'></sub-elementA>" +
            "<sub-elementB attribute='B'></sub-elementB>" +
            "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        );

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>())).Returns(message);

        var validateMessageElements = new Dictionary<string, object>
        {
            { "root.element.sub-elementA", "" }, { "sub-elementB", "" }
        };

        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        Assert.DoesNotThrow(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestValidateMessageElementAttributes()
    {
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

        var validateMessageElements = new Dictionary<string, object>
        {
            { "root.element.sub-elementA.attribute", "A" }, { "sub-elementB.attribute", "B" }
        };

        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        Assert.DoesNotThrow(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestValidateMessageElementsWrongExpectedElement()
    {
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

        var validateMessageElements = new Dictionary<string, object>
        {
            { "root.element.sub-element-wrong", "text-value" }, { "sub-element-wrong", "text-value" }
        };

        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        Assert.Throws<AgenixSystemException>(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestValidateMessageElementsWrongExpectedValue()
    {
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

        var validateMessageElements = new Dictionary<string, object>
        {
            { "root.element.sub-elementA", "text-value-wrong" }, { "sub-elementB", "text-value-wrong" }
        };

        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        Assert.Throws<ValidationException>(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestValidateMessageElementAttributesWrongExpectedValue()
    {
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

        var validateMessageElements = new Dictionary<string, object>
        {
            { "root.element.sub-elementA.attribute", "wrong-value" }, { "sub-elementB.attribute", "wrong-value" }
        };

        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        Assert.Throws<ValidationException>(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestValidateMessageElementAttributesWrongExpectedAttribute()
    {
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

        var validateMessageElements = new Dictionary<string, object>
        {
            { "root.element.sub-elementA.attribute-wrong", "A" }, { "sub-elementB.attribute-wrong", "B" }
        };

        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(validationContext)
            .Build();

        Assert.Throws<AgenixSystemException>(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestSetMessageElements()
    {
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        var message = new DefaultMessage(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-elementA attribute='A'>text-value</sub-elementA>" +
            "<sub-elementB attribute='B'>text-value</sub-elementB>" +
            "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        );

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>())).Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-elementA attribute='A'>to be overwritten</sub-elementA>" +
            "<sub-elementB attribute='B'>to be overwritten</sub-elementB>" +
            "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        ));

        var messageElements = new Dictionary<string, object>
        {
            { "root.element.sub-elementA", "text-value" }, { "sub-elementB", "text-value" }
        };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(messageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.XML)
            .Process(processor)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestSetMessageElementsUsingEmptyString()
    {
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        var message = new DefaultMessage(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-elementA attribute='A'></sub-elementA>" +
            "<sub-elementB attribute='B'></sub-elementB>" +
            "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        );

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>())).Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-elementA attribute='A'>to be overwritten</sub-elementA>" +
            "<sub-elementB attribute='B'>to be overwritten</sub-elementB>" +
            "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        ));

        var messageElements = new Dictionary<string, object>
        {
            { "root.element.sub-elementA", "" }, { "sub-elementB", "" }
        };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(messageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.XML)
            .Process(processor)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestSetMessageElementsAndValidate()
    {
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        var message = new DefaultMessage(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-elementA attribute='A'>text-value</sub-elementA>" +
            "<sub-elementB attribute='B'>text-value</sub-elementB>" +
            "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        );

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>())).Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-elementA attribute='A'>to be overwritten</sub-elementA>" +
            "<sub-elementB attribute='B'>to be overwritten</sub-elementB>" +
            "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        ));

        var messageElements = new Dictionary<string, object>
        {
            { "root.element.sub-elementA", "text-value" }, { "sub-elementB", "text-value" }
        };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(messageElements)
            .Build();

        var validateElements = new Dictionary<string, object>
        {
            { "root.element.sub-elementA", "text-value" }, { "sub-elementB", "text-value" }
        };

        var validationContext = new XpathMessageValidationContext.Builder()
            .Expressions(validateElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.XML)
            .Validate(validationContext)
            .Process(processor)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestSetMessageElementAttributes()
    {
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        var message = new DefaultMessage(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-elementA attribute='A'>text-value</sub-elementA>" +
            "<sub-elementB attribute='B'>text-value</sub-elementB>" +
            "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        );

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>())).Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-elementA attribute='to be overwritten'>text-value</sub-elementA>" +
            "<sub-elementB attribute='to be overwritten'>text-value</sub-elementB>" +
            "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        ));

        var messageElements = new Dictionary<string, object>
        {
            { "root.element.sub-elementA.attribute", "A" }, { "sub-elementB.attribute", "B" }
        };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(messageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.XML)
            .Process(processor)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestSetMessageElementsError()
    {
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        var message = new DefaultMessage(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-elementA attribute='A'>text-value</sub-elementA>" +
            "<sub-elementB attribute='B'>text-value</sub-elementB>" +
            "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        );

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>())).Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-elementA attribute='A'>to be overwritten</sub-elementA>" +
            "<sub-elementB attribute='B'>to be overwritten</sub-elementB>" +
            "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        ));

        var messageElements = new Dictionary<string, object>
        {
            { "root.element.sub-element-wrong", "text-value" }, { "sub-element-wrong", "text-value" }
        };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(messageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.XML)
            .Process(processor)
            .Build();

        Assert.Throws<AgenixSystemException>(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestSetMessageElementAttributesError()
    {
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        var message = new DefaultMessage(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-elementA attribute='A'>text-value</sub-elementA>" +
            "<sub-elementB attribute='B'>text-value</sub-elementB>" +
            "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        );

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>())).Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-elementA attribute='to be overwritten'>text-value</sub-elementA>" +
            "<sub-elementB attribute='to be overwritten'>text-value</sub-elementB>" +
            "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        ));

        var messageElements = new Dictionary<string, object>
        {
            { "root.element.sub-elementA.attribute-wrong", "A" }, { "sub-elementB.attribute-wrong", "B" }
        };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(messageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.XML)
            .Process(processor)
            .Build();

        Assert.Throws<AgenixSystemException>(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestSetMessageElementAttributesErrorWrongElement()
    {
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(c => c.Timeout).Returns(5000L);

        var message = new DefaultMessage(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-elementA attribute='A'>text-value</sub-elementA>" +
            "<sub-elementB attribute='B'>text-value</sub-elementB>" +
            "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        );

        _consumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>())).Returns(message);

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-elementA attribute='to be overwritten'>text-value</sub-elementA>" +
            "<sub-elementB attribute='to be overwritten'>text-value</sub-elementB>" +
            "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        ));

        var messageElements = new Dictionary<string, object>
        {
            { "root.element.sub-elementA-wrong.attribute", "A" }, { "sub-elementB-wrong.attribute", "B" }
        };

        var processor = new XpathMessageProcessor.Builder()
            .Expressions(messageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.XML)
            .Process(processor)
            .Build();

        Assert.Throws<AgenixSystemException>(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestExtractMessageElements()
    {
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

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-elementA attribute='A'>text-value</sub-elementA>" +
            "<sub-elementB attribute='B'>text-value</sub-elementB>" +
            "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        ));

        var extractMessageElements = new Dictionary<string, object>
        {
            { "root.element.sub-elementA", "${valueA}" }, { "root.element.sub-elementB", "${valueB}" }
        };

        var variableExtractor = new XpathPayloadVariableExtractor.Builder()
            .Expressions(extractMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Process(variableExtractor)
            .Build();

        receiveAction.Execute(Context);

        Assert.That(Context.GetVariables().ContainsKey("valueA"), Is.True);
        Assert.That(Context.GetVariables()["valueA"], Is.EqualTo("text-value"));
        Assert.That(Context.GetVariables().ContainsKey("valueB"), Is.True);
        Assert.That(Context.GetVariables()["valueB"], Is.EqualTo("text-value"));
    }

    [Test]
    public void TestExtractMessageAttributes()
    {
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

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-elementA attribute='A'>text-value</sub-elementA>" +
            "<sub-elementB attribute='B'>text-value</sub-elementB>" +
            "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        ));

        var extractMessageElements = new Dictionary<string, object>
        {
            { "root.element.sub-elementA.attribute", "${valueA}" },
            { "root.element.sub-elementB.attribute", "${valueB}" }
        };

        var variableExtractor = new XpathPayloadVariableExtractor.Builder()
            .Expressions(extractMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Process(variableExtractor)
            .Build();

        receiveAction.Execute(Context);

        Assert.That(Context.GetVariables().ContainsKey("valueA"), Is.True);
        Assert.That(Context.GetVariables()["valueA"], Is.EqualTo("A"));
        Assert.That(Context.GetVariables().ContainsKey("valueB"), Is.True);
        Assert.That(Context.GetVariables()["valueB"], Is.EqualTo("B"));
    }

    [Test]
    public void TestExtractMessageElementsForWrongElement()
    {
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

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-elementA attribute='A'>text-value</sub-elementA>" +
            "<sub-elementB attribute='B'>text-value</sub-elementB>" +
            "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        ));

        var extractMessageElements = new Dictionary<string, object>
        {
            { "root.element.sub-element-wrong", "${valueA}" }, { "element.sub-element-wrong", "${valueB}" }
        };

        var variableExtractor = new XpathPayloadVariableExtractor.Builder()
            .Expressions(extractMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Process(variableExtractor)
            .Build();

        Assert.Throws<AgenixSystemException>(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestExtractMessageElementsForWrongAttribute()
    {
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

        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "<root>" +
            "<element attributeA='attribute-value' attributeB='attribute-value'>" +
            "<sub-elementA attribute='A'>text-value</sub-elementA>" +
            "<sub-elementB attribute='B'>text-value</sub-elementB>" +
            "<sub-elementC attribute='C'>text-value</sub-elementC>" +
            "</element>" +
            "</root>"
        ));

        var extractMessageElements = new Dictionary<string, object>
        {
            { "root.element.sub-elementA.attribute-wrong", "${attributeA}" }
        };

        var variableExtractor = new XpathPayloadVariableExtractor.Builder()
            .Expressions(extractMessageElements)
            .Build();

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Process(variableExtractor)
            .Build();

        Assert.That(
            () => receiveAction.Execute(Context),
            Throws.TypeOf<AgenixSystemException>()
        );
    }
}
