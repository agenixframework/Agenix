using Agenix.Api.Endpoint;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Core;
using Agenix.Core.Actions;
using Agenix.Core.Message;
using Agenix.Core.Message.Builder;
using Agenix.Core.Validation.Builder;
using Agenix.Core.Validation.Json;
using Agenix.Validation.Json.Validation;
using Moq;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Json.Tests.Json;

public class ReceiveMessageActionTest : AbstractNUnitSetUp
{
    private Mock<ISelectiveConsumer> _consumerMock;
    private Mock<IEndpointConfiguration> _endpointConfigurationMock;
    private Mock<IEndpoint> _endpointMock;
    private Mock<IMessageQueue> _mockQueueMock;

    protected override TestContextFactory CreateTestContextFactory()
    {
        _endpointMock = new Mock<IEndpoint>();
        _consumerMock = new Mock<ISelectiveConsumer>();
        _endpointConfigurationMock = new Mock<IEndpointConfiguration>();
        _mockQueueMock = new Mock<IMessageQueue>();
        var factory = base.CreateTestContextFactory();
        factory.ReferenceResolver.Bind("mockQueue", _mockQueueMock.Object);
        return factory;
    }

    [Test]
    public void TestReceiveMessageOverwriteMessageElementsJsonPath()
    {
        // Setup control message builder with JSON payload template
        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new JsonMessageValidationContext();
        controlMessageBuilder.SetPayloadBuilder(
            new DefaultPayloadBuilder("{ \"TestRequest\": { \"Message\": \"?\" }}"));

        // Create JSONPath expressions to modify message
        var overwriteElements = new Dictionary<string, object> { ["$.TestRequest.Message"] = "Hello World!" };

        // Create JsonPath processor
        var processor = new JsonPathMessageProcessor.Builder()
            .Expressions(overwriteElements)
            .Build();

        // Mock message to be received by consumer
        var controlMessage = new DefaultMessage("{ \"TestRequest\": { \"Message\": \"Hello World!\" }}");

        // Reset and setup mocks
        _endpointMock.Reset();
        _consumerMock.Reset();
        _endpointConfigurationMock.Reset();

        _endpointMock.Setup(x => x.CreateConsumer()).Returns(_consumerMock.Object);
        _endpointMock.Setup(x => x.EndpointConfiguration).Returns(_endpointConfigurationMock.Object);
        _endpointConfigurationMock.Setup(x => x.Timeout).Returns(5000L);

        // Mock consumer to return the control message
        _consumerMock
            .Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        // Create and execute receive action
        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpointMock.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.JSON)
            .Validate(validationContext)
            .Process(processor)
            .Build();

        receiveAction.Execute(Context);
    }

    [Test]
    public void TestReceiveMessageWithExtractVariablesFromMessageJsonPath()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        var validationContext = new JsonMessageValidationContext();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder(
            "{\"text\":\"Hello World!\", \"person\":{\"name\":\"John\",\"surname\":\"Doe\"}, \"index\":5, \"id\":\"x123456789x\"}"));

        var extractMessageElements = new Dictionary<string, object>
        {
            ["$.text"] = "messageVar", ["$.person"] = "person"
        };

        var variableExtractor = new JsonPathVariableExtractor.Builder()
            .Expressions(extractMessageElements)
            .Build();

        var controlMessage =
            new DefaultMessage(
                "{\"text\":\"Hello World!\", \"person\":{\"name\":\"John\",\"surname\":\"Doe\"}, \"index\":5, \"id\":\"x123456789x\"}");

        _endpointMock.Reset();
        _consumerMock.Reset();
        _endpointConfigurationMock.Reset();

        _endpointMock.Setup(x => x.CreateConsumer()).Returns(_consumerMock.Object);
        _endpointMock.Setup(x => x.EndpointConfiguration).Returns(_endpointConfigurationMock.Object);
        _endpointConfigurationMock.Setup(x => x.Timeout).Returns(5000L);

        _consumerMock
            .Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpointMock.Object)
            .Message(controlMessageBuilder)
            .Process(variableExtractor)
            .Validate(validationContext)
            .Build();

        receiveAction.Execute(Context);


        Assert.That(Context.GetVariable("messageVar"), Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Context.GetVariable("messageVar"), Is.EqualTo("Hello World!"));

            Assert.That(Context.GetVariable("person"), Is.Not.Null);
        }

        Assert.That(Context.GetVariable("person"), Does.Contain("\"John\""));
    }

    [Test]
    public void TestReceiveMessageWithJsonPathValidation()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();

        var jsonPathExpressions = new Dictionary<string, object>
        {
            ["$..text"] = "Hello World!",
            ["$.person.name"] = "John",
            ["$.person.surname"] = "Doe",
            ["$.index"] = "5"
        };

        var validationContext = new JsonPathMessageValidationContext.Builder()
            .Expressions(jsonPathExpressions)
            .Build();

        var controlMessage =
            new DefaultMessage(
                "{\"text\":\"Hello World!\", \"person\":{\"name\":\"John\",\"surname\":\"Doe\"}, \"index\":5, \"id\":\"x123456789x\"}");

        _endpointMock.Reset();
        _consumerMock.Reset();
        _endpointConfigurationMock.Reset();

        _endpointMock.Setup(x => x.CreateConsumer()).Returns(_consumerMock.Object);
        _endpointMock.Setup(x => x.EndpointConfiguration).Returns(_endpointConfigurationMock.Object);
        _endpointConfigurationMock.Setup(x => x.Timeout).Returns(5000L);

        _consumerMock
            .Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpointMock.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.JSON)
            .Validate(validationContext)
            .Build();

        // Using Assert.That to verify no exceptions are thrown during execution
        Assert.That(() => receiveAction.Execute(Context), Throws.Nothing);
    }

    [Test]
    public void TestReceiveMessageWithJsonPathValidationFailure()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();

        var jsonPathExpressions = new Dictionary<string, object> { ["$..text"] = "Hello Agenix!" };

        var validationContext = new JsonPathMessageValidationContext.Builder()
            .Expressions(jsonPathExpressions)
            .Build();

        var controlMessage =
            new DefaultMessage(
                "{\"text\":\"Hello World!\", \"person\":{\"name\":\"John\",\"surname\":\"Doe\"}, \"index\":5, \"id\":\"x123456789x\"}");

        _endpointMock.Reset();
        _consumerMock.Reset();
        _endpointConfigurationMock.Reset();

        _endpointMock.Setup(x => x.CreateConsumer()).Returns(_consumerMock.Object);
        _endpointMock.Setup(x => x.EndpointConfiguration).Returns(_endpointConfigurationMock.Object);
        _endpointConfigurationMock.Setup(x => x.Timeout).Returns(5000L);

        _consumerMock
            .Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpointMock.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.JSON)
            .Validate(validationContext)
            .Build();

        // Using Assert.That with Throws.TypeOf to verify an exception
        Assert.That(() => receiveAction.Execute(Context),
            Throws.TypeOf<ValidationException>());
    }

    [Test]
    public void TestReceiveMessageWithJsonPathValidationNoPathResult()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();

        var jsonPathExpressions = new Dictionary<string, object> { ["$.person.age"] = "50" };

        var validationContext = new JsonPathMessageValidationContext.Builder()
            .Expressions(jsonPathExpressions)
            .Build();

        var controlMessage =
            new DefaultMessage(
                "{\"text\":\"Hello World!\", \"person\":{\"name\":\"John\",\"surname\":\"Doe\"}, \"index\":5, \"id\":\"x123456789x\"}");

        _endpointMock.Reset();
        _consumerMock.Reset();
        _endpointConfigurationMock.Reset();

        _endpointMock.Setup(x => x.CreateConsumer()).Returns(_consumerMock.Object);
        _endpointMock.Setup(x => x.EndpointConfiguration).Returns(_endpointConfigurationMock.Object);
        _endpointConfigurationMock.Setup(x => x.Timeout).Returns(5000L);

        _consumerMock
            .Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpointMock.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.JSON)
            .Validate(validationContext)
            .Build();

        // Using Assert.That with Throws.TypeOf to verify the specific exception
        Assert.That(() => receiveAction.Execute(Context),
            Throws.TypeOf<AgenixSystemException>());
    }

    [Test]
    public void TestReceiveEmptyMessagePayloadAsExpected()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();

        var controlMessage = new DefaultMessage("");

        _endpointMock.Reset();
        _consumerMock.Reset();
        _endpointConfigurationMock.Reset();

        _endpointMock.Setup(x => x.CreateConsumer()).Returns(_consumerMock.Object);
        _endpointMock.Setup(x => x.EndpointConfiguration).Returns(_endpointConfigurationMock.Object);
        _endpointConfigurationMock.Setup(x => x.Timeout).Returns(5000L);

        _consumerMock
            .Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpointMock.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.JSON)
            .Build();

        // Using Assert.That to verify no exceptions are thrown during execution
        Assert.That(() => receiveAction.Execute(Context), Throws.Nothing);
    }

    [Test]
    public void TestReceiveEmptyMessagePayloadUnexpected()
    {
        var controlMessageBuilder = new DefaultMessageBuilder();
        controlMessageBuilder.SetPayloadBuilder(new DefaultPayloadBuilder("{\"text\":\"Hello World!\"}"));

        var controlMessage = new DefaultMessage("");

        _endpointMock.Reset();
        _consumerMock.Reset();
        _endpointConfigurationMock.Reset();

        _endpointMock.Setup(x => x.CreateConsumer()).Returns(_consumerMock.Object);
        _endpointMock.Setup(x => x.EndpointConfiguration).Returns(_endpointConfigurationMock.Object);
        _endpointConfigurationMock.Setup(x => x.Timeout).Returns(5000L);

        _consumerMock
            .Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(controlMessage);

        var receiveAction = new ReceiveMessageAction.Builder()
            .Endpoint(_endpointMock.Object)
            .Message(controlMessageBuilder)
            .Type(MessageType.JSON)
            .Build();

        // Using Assert.That with Throws.Exception and Has.Message to verify both the exception type and message
        Assert.That(() => receiveAction.Execute(Context),
            Throws.TypeOf<ValidationException>()
                .With.Message.EqualTo("Validation failed - expected message contents, but received empty message!"));
    }
}
