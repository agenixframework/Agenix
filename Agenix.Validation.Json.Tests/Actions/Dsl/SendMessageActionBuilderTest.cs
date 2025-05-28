using Agenix.Api.Endpoint;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Api.Report;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Core;
using Agenix.Core.Actions;
using Agenix.Core.Container;
using Agenix.Core.Validation.Builder;
using Agenix.Validation.Json.Dsl;
using Agenix.Validation.Json.Json;
using Agenix.Validation.Json.Json.Schema;
using Agenix.Validation.Json.Message.Builder;
using Agenix.Validation.Json.Tests.Message;
using Agenix.Validation.Json.Validation;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;
using static Agenix.Core.Actions.SendMessageAction.Builder;
using IReferenceResolver = Agenix.Api.Spi.IReferenceResolver;

namespace Agenix.Validation.Json.Tests.Actions.Dsl;

/// <summary>
///     Unit test class for verifying the functionality of the SendMessageActionBuilder.
/// </summary>
public class SendMessageActionBuilderTest : AbstractNUnitSetUp
{
    private Mock<IEndpoint> _messageEndpoint;
    private Mock<IProducer> _messageProducer;
    private Mock<IReferenceResolver> _referenceResolver;

    private JsonSerializer _serializer;

    [SetUp]
    public void SetUp()
    {
        _referenceResolver = new Mock<IReferenceResolver>();
        _messageEndpoint = new Mock<IEndpoint>();
        _messageProducer = new Mock<IProducer>();
        _serializer = new JsonSerializer
        {
            ContractResolver = new LowercaseContractResolver()
        };
    }

    [Test]
    public void TestSendBuilderWithPayloadModel()
    {
        // Reset mocks individually
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageProducer.Reset();

        _messageEndpoint.Setup(x => x.CreateProducer()).Returns(_messageProducer.Object);

        _messageProducer.Setup(x => x.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, ctx) =>
            {
                Assert.That(message.GetPayload<string>(), Is.EqualTo("{\"message\":\"Hello Agenix!\"}"));
            });

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());

        var mappers = new Dictionary<string, JsonSerializer> { { "serializer", _serializer } };
        _referenceResolver.Setup(x => x.ResolveAll<JsonSerializer>()).Returns(mappers);
        _referenceResolver.Setup(x => x.Resolve<JsonSerializer>()).Returns(_serializer);

        Context.SetReferenceResolver(_referenceResolver.Object);

        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(Send(_messageEndpoint.Object).Message()
            .Body(new JsonSerializerPayloadBuilder(new TestRequest("Hello Agenix!"))));

        var test = runner.GetTestCase();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.GetActionCount(), Is.EqualTo(1));
            Assert.That(test.GetActions()[0], Is.InstanceOf<SendMessageAction>());
        }

        var action = (SendMessageAction)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.Name, Is.EqualTo("send"));

            Assert.That(action.Endpoint, Is.SameAs(_messageEndpoint.Object));
            Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        }

        var messageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(messageBuilder.BuildMessagePayload(Context, action.MessageType),
                Is.EqualTo("{\"message\":\"Hello Agenix!\"}"));
            Assert.That(messageBuilder.BuildMessageHeaders(Context), Is.Empty);
        }
    }

    [Test]
    public void TestSendBuilderWithExplicitHeaderValues()
    {
        // Reset mocks individually
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageProducer.Reset();

        _messageEndpoint.Setup(x => x.CreateProducer()).Returns(_messageProducer.Object);

        _messageProducer.Setup(x => x.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, ctx) =>
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(message.GetPayload<string>(), Is.EqualTo("{\"message\":\"Hello Agenix!\"}"));
                    Assert.That(message.GetHeaders(), Has.Count.EqualTo(5));
                }

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(message.GetHeaders()["Content-Type"], Is.EqualTo("application/json"));
                    Assert.That(message.GetHeaders()["X-CustomHeader"], Is.EqualTo("custom-value"));
                }
            });

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());
        _referenceResolver.Setup(x => x.ResolveAll<JsonSerializer>())
            .Returns(new Dictionary<string, JsonSerializer> { { "serializer", _serializer } });
        _referenceResolver.Setup(x => x.Resolve<JsonSerializer>()).Returns(_serializer);

        Context.SetReferenceResolver(_referenceResolver.Object);

        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(Send(_messageEndpoint.Object).Message()
            .Header("Content-Type", "application/json")
            .Header("X-CustomHeader", "custom-value")
            .Body(new JsonSerializerPayloadBuilder(new TestRequest("Hello Agenix!"))));

        var test = runner.GetTestCase();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.GetActionCount(), Is.EqualTo(1));
            Assert.That(test.GetActions()[0], Is.InstanceOf<SendMessageAction>());
        }

        var action = (SendMessageAction)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.Name, Is.EqualTo("send"));
            Assert.That(action.Endpoint, Is.SameAs(_messageEndpoint.Object));
            Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        }

        var messageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(messageBuilder.BuildMessagePayload(Context, action.MessageType),
                Is.EqualTo("{\"message\":\"Hello Agenix!\"}"));

            var headers = messageBuilder.BuildMessageHeaders(Context);
            Assert.That(headers.Count, Is.EqualTo(2));
            Assert.That(headers["Content-Type"], Is.EqualTo("application/json"));
            Assert.That(headers["X-CustomHeader"], Is.EqualTo("custom-value"));
        }
    }

    [Test]
    public void TestSendBuilderWithPayloadModelExplicitObjectMapper()
    {
        // Reset mocks individually
        _messageEndpoint.Reset();
        _messageProducer.Reset();

        _messageEndpoint.Setup(x => x.CreateProducer()).Returns(_messageProducer.Object);

        _messageProducer.Setup(x => x.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, ctx) =>
            {
                Assert.That(message.GetPayload<string>(), Is.EqualTo("{\"message\":\"Hello Agenix!\"}"));
            });

        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(Send(_messageEndpoint.Object).Message()
            .Body(new JsonSerializerPayloadBuilder(new TestRequest("Hello Agenix!"), _serializer)));

        var test = runner.GetTestCase();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.GetActionCount(), Is.EqualTo(1));
            Assert.That(test.GetActions()[0], Is.InstanceOf<SendMessageAction>());
        }

        var action = (SendMessageAction)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.Name, Is.EqualTo("send"));

            Assert.That(action.Endpoint, Is.SameAs(_messageEndpoint.Object));
            Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        }

        var messageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(messageBuilder.BuildMessagePayload(Context, action.MessageType),
                Is.EqualTo("{\"message\":\"Hello Agenix!\"}"));
            Assert.That(messageBuilder.BuildMessageHeaders(Context), Is.Empty);
        }
    }

    [Test]
    public void TestSendBuilderWithPayloadModelExplicitObjectMapperName()
    {
        // Reset mocks individually
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageProducer.Reset();

        _messageEndpoint.Setup(x => x.CreateProducer()).Returns(_messageProducer.Object);

        _messageProducer.Setup(x => x.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, ctx) =>
            {
                Assert.That(message.GetPayload<string>(), Is.EqualTo("{\"message\":\"Hello Agenix!\"}"));
            });

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());
        _referenceResolver.Setup(x => x.IsResolvable("myJsonSerializer")).Returns(true);
        _referenceResolver.Setup(x => x.Resolve<JsonSerializer>("myJsonSerializer"))
            .Returns(_serializer);

        Context.SetReferenceResolver(_referenceResolver.Object);

        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(Send(_messageEndpoint.Object).Message()
            .Body(new JsonSerializerPayloadBuilder(new TestRequest("Hello Agenix!"), "myJsonSerializer")));

        var test = runner.GetTestCase();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.GetActionCount(), Is.EqualTo(1));
            Assert.That(test.GetActions()[0], Is.InstanceOf<SendMessageAction>());
        }

        var action = (SendMessageAction)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.Name, Is.EqualTo("send"));

            Assert.That(action.Endpoint, Is.SameAs(_messageEndpoint.Object));
            Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        }

        var messageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(messageBuilder.BuildMessagePayload(Context, action.MessageType),
                Is.EqualTo("{\"message\":\"Hello Agenix!\"}"));
            Assert.That(messageBuilder.BuildMessageHeaders(Context), Is.Empty);
        }
    }

    [Test]
    public void TestSendBuilderExtractJsonPathFromPayload()
    {
        // Arrange
        _messageEndpoint.Reset();
        _messageProducer.Reset();

        _messageEndpoint.Setup(x => x.CreateProducer()).Returns(_messageProducer.Object);

        _messageProducer.Setup(x => x.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) =>
            {
                Assert.That(message.GetPayload<string>(),
                    Is.EqualTo(
                        "{\"text\":\"Hello World!\", \"person\":{\"name\":\"John\",\"surname\":\"Doe\"}, \"index\":5, \"id\":\"x123456789x\"}"));
            });

        // Act
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(Send(_messageEndpoint.Object)
            .Message()
            .Type(MessageType.JSON)
            .Body(
                "{\"text\":\"Hello World!\", \"person\":{\"name\":\"John\",\"surname\":\"Doe\"}, \"index\":5, \"id\":\"x123456789x\"}")
            .Extract(JsonPathSupport
                .JsonPath()
                .Expression("$.text", "text")
                .Expression("$.person", "person")));

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(Context.GetVariable("text"), Is.Not.Null);
            Assert.That(Context.GetVariable("person"), Is.Not.Null);
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Context.GetVariable("text"), Is.EqualTo("Hello World!"));
            Assert.That(Context.GetVariable("person"), Does.Contain("\"John\""));
        }

        var test = runner.GetTestCase();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.GetActionCount(), Is.EqualTo(1));
            Assert.That(test.GetActions()[0], Is.InstanceOf<SendMessageAction>());
        }

        var action = (SendMessageAction)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.Name, Is.EqualTo("send"));
            Assert.That(action.Endpoint, Is.SameAs(_messageEndpoint.Object));

            Assert.That(action.VariableExtractors.Count, Is.EqualTo(1));
        }

        Assert.That(action.VariableExtractors[0], Is.InstanceOf<JsonPathVariableExtractor>());

        var extractor = (JsonPathVariableExtractor)action.VariableExtractors[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(extractor.JsonPathExpressions.ContainsKey("$.text"), Is.True);
            Assert.That(extractor.JsonPathExpressions.ContainsKey("$.person"), Is.True);
        }
    }

    [Test]
    public void TestJsonPathSupport()
    {
        // Arrange
        _messageEndpoint.Reset();
        _messageProducer.Reset();

        _messageEndpoint.Setup(x => x.CreateProducer()).Returns(_messageProducer.Object);

        _messageProducer.Setup(x => x.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) =>
            {
                var payload = message.GetPayload<string>().Replace(" ", "");
                Assert.That(payload, Is.EqualTo("{\"TestRequest\":{\"Message\":\"HelloWorld!\"}}"));
            });

        // Act
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(Send(_messageEndpoint.Object)
            .Message()
            .Type(MessageType.JSON)
            .Body("{ \"TestRequest\": { \"Message\": \"?\" }}")
            .Process(JsonPathSupport.JsonPath()
                .Expression("$.TestRequest.Message", "Hello World!")));

        // Assert
        var test = runner.GetTestCase();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.GetActionCount(), Is.EqualTo(1));
            Assert.That(test.GetActions()[0], Is.InstanceOf<SendMessageAction>());
        }

        var action = (SendMessageAction)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.Name, Is.EqualTo("send"));
            Assert.That(action.Endpoint, Is.SameAs(_messageEndpoint.Object));

            Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
            Assert.That(action.Processors, Has.Count.EqualTo(1));
        }

        Assert.That(action.Processors[0], Is.InstanceOf<JsonPathMessageProcessor>());

        var processor = (JsonPathMessageProcessor)action.Processors[0];
        Assert.That(processor.JsonPathExpressions["$.TestRequest.Message"], Is.EqualTo("Hello World!"));
    }

    [Test]
    public void TestSendWithSchemaValidation()
    {
        // Arrange
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageProducer.Reset();

        _messageEndpoint.Setup(x => x.CreateProducer()).Returns(_messageProducer.Object);

        var schemaRepository = new JsonSchemaRepository();
        schemaRepository.SetName("fooRepository");

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.ResolveAll<JsonSchemaRepository>())
            .Returns(new Dictionary<string, JsonSchemaRepository> { { "fooRepository", schemaRepository } });

        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());

        Context.SetReferenceResolver(_referenceResolver.Object);

        // Act
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(Send(_messageEndpoint.Object)
            .Message()
            .SchemaValidation(true)
            .Schema("fooSchema")
            .SchemaRepository("fooRepository")
            .Type(MessageType.JSON)
            .Body("{ \"TestRequest\": { \"Message\": \"?\" }}"));

        // Assert
        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0], Is.InstanceOf<SendMessageAction>());

        var action = (SendMessageAction)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.Name, Is.EqualTo("send"));
            Assert.That(action.Endpoint, Is.SameAs(_messageEndpoint.Object));
            Assert.That(action.IsSchemaValidation, Is.True);
            Assert.That(action.Schema, Is.EqualTo("fooSchema"));
            Assert.That(action.SchemaRepository, Is.EqualTo("fooRepository"));
        }
    }

    [Test]
    public void TestSendWithSchemaRepositoryAndValidatorResolution()
    {
        // Arrange
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageProducer.Reset();

        _messageEndpoint.Setup(x => x.CreateProducer()).Returns(_messageProducer.Object); ;

        var schemaRepository = new JsonSchemaRepository();
        schemaRepository.SetName("fooRepository");

        var schemaValidator = new Mock<ISchemaValidator<ISchemaValidationContext>>();
        schemaValidator.Setup(x => x.SupportsMessageType(It.Is<string>(s => s == "JSON"), It.IsAny<IMessage>()))
            .Returns(true);
        schemaValidator.Setup(x => x.CanValidate(It.IsAny<IMessage>(), It.IsAny<bool>())).Returns(true);

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.ResolveAll<JsonSchemaRepository>())
            .Returns(new Dictionary<string, JsonSchemaRepository> { { "fooRepository", schemaRepository } });
        _referenceResolver.Setup(x => x.ResolveAll<ISchemaValidator<ISchemaValidationContext>>())
            .Returns(new Dictionary<string, ISchemaValidator<ISchemaValidationContext>> { { "jsonSchemaValidator", schemaValidator.Object } });

        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());

        Context.SetReferenceResolver(_referenceResolver.Object);
        Context.MessageValidatorRegistry.AddSchemeValidator("JSON", schemaValidator.Object);

        // Act
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(Send(_messageEndpoint.Object)
            .Message()
            .SchemaValidation(true)
            .Schema("fooSchema")
            .SchemaRepository("fooRepository")
            .Type(MessageType.JSON)
            .Body("{ \"TestRequest\": { \"Message\": \"?\" }}"));

        // Assert
        schemaValidator.Verify(x => x.Validate(
                It.IsAny<IMessage>(),
                It.IsAny<TestContext>(),
                It.Is<string>(s => s == "fooRepository"),
                It.Is<string>(s => s == "fooSchema")),
            Times.Once);

        var test = runner.GetTestCase();
        var action = (SendMessageAction)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.IsSchemaValidation, Is.True);
            Assert.That(action.Schema, Is.EqualTo("fooSchema"));
            Assert.That(action.SchemaRepository, Is.EqualTo("fooRepository"));
        }
    }

    // Create a custom contract resolver that converts property names to lowercase
    private class LowercaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLower();
        }
    }
}