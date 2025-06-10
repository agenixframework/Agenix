using Agenix.Api.Endpoint;
using Agenix.Api.Exceptions;
using Agenix.Api.Messaging;
using Agenix.Api.Validation.Context;
using Agenix.Core.Actions;
using Agenix.Core.Message;
using Agenix.Core.Message.Builder;
using Agenix.Core.Validation.Builder;
using Agenix.Core.Variable;
using Moq;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Xml.Tests;

public class HeaderValuesTest : AbstractNUnitSetUp
{
    private readonly Mock<IConsumer> _consumer = new();
    private readonly Mock<IEndpoint> _endpoint = new();
    private readonly Mock<IEndpointConfiguration> _endpointConfiguration = new();

    [Test]
    public void TestValidateHeaderValues()
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
            )
            .SetHeader("header-valueA", "A")
            .SetHeader("header-valueB", "B")
            .SetHeader("header-valueC", "C");

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

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

        var validateHeaderValues = new Dictionary<string, object> { { "header-valueA", "A" } };

        controlMessageBuilder.AddHeaderBuilder(new DefaultHeaderBuilder(validateHeaderValues));

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestValidateHeaderValuesComplete()
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
            )
            .SetHeader("header-valueA", "A")
            .SetHeader("header-valueB", "B")
            .SetHeader("header-valueC", "C");

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

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

        var validateHeaderValues = new Dictionary<string, object>
        {
            { "header-valueA", "A" }, { "header-valueB", "B" }, { "header-valueC", "C" }
        };

        controlMessageBuilder.AddHeaderBuilder(new DefaultHeaderBuilder(validateHeaderValues));

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Validate(new HeaderValidationContext.Builder().Build())
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestValidateHeaderValuesWrongExpectedValue()
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
            )
            .SetHeader("header-valueA", "A")
            .SetHeader("header-valueB", "B")
            .SetHeader("header-valueC", "C");

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

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

        var validateHeaderValues = new Dictionary<string, object> { { "header-valueA", "wrong" } };

        controlMessageBuilder.AddHeaderBuilder(new DefaultHeaderBuilder(validateHeaderValues));

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        Assert.Throws<ValidationException>(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestValidateHeaderValuesForWrongElement()
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
            )
            .SetHeader("header-valueA", "A")
            .SetHeader("header-valueB", "B")
            .SetHeader("header-valueC", "C");

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

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

        var validateHeaderValues = new Dictionary<string, object> { { "header-wrong", "A" } };

        controlMessageBuilder.AddHeaderBuilder(new DefaultHeaderBuilder(validateHeaderValues));

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        Assert.Throws<ValidationException>(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestValidateEmptyHeaderValues()
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
            )
            .SetHeader("header-valueA", "")
            .SetHeader("header-valueB", "")
            .SetHeader("header-valueC", "");

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

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

        var validateHeaderValues = new Dictionary<string, object>
        {
            { "header-valueA", "" }, { "header-valueB", "" }, { "header-valueC", "" }
        };

        controlMessageBuilder.AddHeaderBuilder(new DefaultHeaderBuilder(validateHeaderValues));

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestValidateHeaderValuesNullComparison()
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
            )
            .SetHeader("header-valueA", "")
            .SetHeader("header-valueB", "")
            .SetHeader("header-valueC", "");

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

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

        var validateHeaderValues = new Dictionary<string, object>
        {
            { "header-valueA", "null" }, { "header-valueB", "null" }, { "header-valueC", "null" }
        };

        controlMessageBuilder.AddHeaderBuilder(new DefaultHeaderBuilder(validateHeaderValues));

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpoint.Object)
            .Message(controlMessageBuilder)
            .Build();

        Assert.Throws<ValidationException>(() => receiveAction.Execute(Context));
    }

    [Test]
    public void TestExtractHeaderValues()
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
            )
            .SetHeader("header-valueA", "A")
            .SetHeader("header-valueB", "B")
            .SetHeader("header-valueC", "C");

        _consumer
            .Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

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

        var extractHeaderValues = new Dictionary<string, string>
        {
            { "header-valueA", "${valueA}" }, { "header-valueB", "${valueB}" }
        };

        var variableExtractor = new MessageHeaderVariableExtractor.Builder()
            .Headers(extractHeaderValues)
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
}
