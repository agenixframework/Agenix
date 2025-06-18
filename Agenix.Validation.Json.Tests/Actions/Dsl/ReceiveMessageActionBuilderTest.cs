using System.Diagnostics;
using System.Text;
using Agenix.Api.Endpoint;
using Agenix.Api.Exceptions;
using Agenix.Api.IO;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Api.Report;
using Agenix.Api.Validation.Context;
using Agenix.Core;
using Agenix.Core.Actions;
using Agenix.Core.Container;
using Agenix.Core.Message;
using Agenix.Core.Message.Builder;
using Agenix.Core.Validation.Builder;
using Agenix.Core.Validation.Json;
using Agenix.Validation.Json.Dsl;
using Agenix.Validation.Json.Json;
using Agenix.Validation.Json.Json.Schema;
using Agenix.Validation.Json.Message.Builder;
using Agenix.Validation.Json.Tests.Message;
using Agenix.Validation.Json.Validation;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NHamcrest;
using NHamcrest.Core;
using NUnit.Framework;
using IReferenceResolver = Agenix.Api.Spi.IReferenceResolver;
using TestContext = Agenix.Api.Context.TestContext;
using Contains = NHamcrest.Contains;
using IsNull = NHamcrest.Is;
using Has = NUnit.Framework.Has;
using Is = NUnit.Framework.Is;

namespace Agenix.Validation.Json.Tests.Actions.Dsl;

public class ReceiveMessageActionBuilderTest : AbstractNUnitSetUp
{
    private Mock<IEndpointConfiguration> _configuration;
    private Mock<IConsumer> _messageConsumer;
    private Mock<IEndpoint> _messageEndpoint;
    private Mock<IReferenceResolver> _referenceResolver;
    private Mock<IResource> _resource;

    private JsonSerializer _serializer;

    [SetUp]
    public void SetUp()
    {
        _referenceResolver = new Mock<IReferenceResolver>();
        _messageEndpoint = new Mock<IEndpoint>();
        _messageConsumer = new Mock<IConsumer>();
        _serializer = new JsonSerializer { ContractResolver = new LowercaseContractResolver() };
        _configuration = new Mock<IEndpointConfiguration>();
        _resource = new Mock<IResource>();
    }

    [Test]
    public void TestReceiveBuilderWithPayloadModel()
    {
        // Arrange
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);

        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("{\"message\": \"Hello Agenix!\"}")
                .SetHeader("operation", "foo"));

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

        // Act
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body(new JsonSerializerPayloadBuilder(new TestRequest("Hello Agenix!"))));

        // Assert
        var test = runner.GetTestCase();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.GetActionCount(), Is.EqualTo(1));
            Assert.That(test.GetActions()[0], Is.InstanceOf<ReceiveMessageAction>());
        }

        var action = (ReceiveMessageAction)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.Name, Is.EqualTo("receive"));
            Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.JSON)));
            Assert.That(action.Endpoint, Is.SameAs(_messageEndpoint.Object));

            Assert.That(action.ValidationContexts.Count, Is.EqualTo(2));
        }

        Assert.That(action.ValidationContexts, Has.Some.InstanceOf<HeaderValidationContext>());
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.ValidationContexts, Has.Some.InstanceOf<DefaultMessageValidationContext>());

            Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        }

        var messageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        Assert.That(messageBuilder.BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo("{\"message\":\"Hello Agenix!\"}"));
    }

    [Test]
    public void TestReceiveBuilderWithPayloadModelExplicitSerializer()
    {
        // Arrange
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);

        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("{\"message\": \"Hello Agenix!\"}")
                .SetHeader("operation", "foo"));

        // Act
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body(new JsonSerializerPayloadBuilder(new TestRequest("Hello Agenix!"), _serializer)));

        // Assert
        var test = runner.GetTestCase();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.GetActionCount(), Is.EqualTo(1));
            Assert.That(test.GetActions()[0], Is.InstanceOf<ReceiveMessageAction>());
        }

        var action = (ReceiveMessageAction)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.Name, Is.EqualTo("receive"));

            Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.JSON)));
            Assert.That(action.Endpoint, Is.SameAs(_messageEndpoint.Object));
            Assert.That(action.ValidationContexts, Has.Count.EqualTo(2));
        }

        Assert.That(action.ValidationContexts, Has.Some.InstanceOf<HeaderValidationContext>());
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.ValidationContexts, Has.Some.InstanceOf<DefaultMessageValidationContext>());

            Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        }

        var messageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        Assert.That(messageBuilder.BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo("{\"message\":\"Hello Agenix!\"}"));
    }

    [Test]
    public void TestReceiveBuilderWithPayloadModelExplicitSerializerName()
    {
        // Arrange
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);

        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("{\"message\": \"Hello Agenix!\"}")
                .SetHeader("operation", "foo"));

        // Setting up reference resolver
        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());

        // Setting up sequence dictionaries
        var beforeTestDict = new Dictionary<string, SequenceBeforeTest>();
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(beforeTestDict);

        var afterTestDict = new Dictionary<string, SequenceAfterTest>();
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(afterTestDict);

        // Setting up a custom serializer reference
        _referenceResolver.Setup(x => x.IsResolvable("mySerializer")).Returns(true);
        _referenceResolver.Setup(x => x.Resolve<JsonSerializer>("mySerializer"))
            .Returns(_serializer);

        Context.SetReferenceResolver(_referenceResolver.Object);

        // Act
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body(new JsonSerializerPayloadBuilder(new TestRequest("Hello Agenix!"), "mySerializer")));

        // Assert
        var test = runner.GetTestCase();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.GetActionCount(), Is.EqualTo(1));
            Assert.That(test.GetActions()[0], Is.InstanceOf<ReceiveMessageAction>());
        }

        var action = (ReceiveMessageAction)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.Name, Is.EqualTo("receive"));

            Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.JSON)));
            Assert.That(action.Endpoint, Is.SameAs(_messageEndpoint.Object));
            Assert.That(action.ValidationContexts, Has.Count.EqualTo(2));
        }

        // Checking validation contexts
        Assert.That(action.ValidationContexts, Has.Some.InstanceOf<HeaderValidationContext>());
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.ValidationContexts, Has.Some.InstanceOf<DefaultMessageValidationContext>());

            Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        }

        var messageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        Assert.That(messageBuilder.BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo("{\"message\":\"Hello Agenix!\"}"));
    }

    [Test]
    public void TestReceiveBuilderWithHeaderFragment()
    {
        // Arrange
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);

        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage()
                .AddHeaderData("{\"message\": \"Hello Agenix!\"}")
                .SetHeader("operation", "foo"));

        // Setting up reference resolver
        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());

        var beforeTestDict = new Dictionary<string, SequenceBeforeTest>();
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(beforeTestDict);

        var afterTestDict = new Dictionary<string, SequenceAfterTest>();
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(afterTestDict);

        var serializerDict = new Dictionary<string, JsonSerializer> { { "serializer", _serializer } };
        _referenceResolver.Setup(x => x.ResolveAll<JsonSerializer>())
            .Returns(serializerDict);

        _referenceResolver.Setup(x => x.Resolve<JsonSerializer>()).Returns(_serializer);

        Context.SetReferenceResolver(_referenceResolver.Object);

        // Act
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Header(new JsonSerializerHeaderDataBuilder(new TestRequest("Hello Agenix!"))));

        // Assert
        var test = runner.GetTestCase();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.GetActionCount(), Is.EqualTo(1));
            Assert.That(test.GetActions()[0], Is.InstanceOf<ReceiveMessageAction>());
        }

        var action = (ReceiveMessageAction)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.Name, Is.EqualTo("receive"));

            Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.JSON)));
            Assert.That(action.Endpoint, Is.SameAs(_messageEndpoint.Object));
            Assert.That(action.ValidationContexts, Has.Count.EqualTo(1));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.ValidationContexts, Has.Some.InstanceOf<HeaderValidationContext>());

            Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        }

        var messageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        Assert.That(messageBuilder.BuildMessageHeaderData(Context), Has.Count.EqualTo(1));
        Assert.That(messageBuilder.BuildMessageHeaderData(Context)[0],
            Is.EqualTo("{\"message\":\"Hello Agenix!\"}"));
    }

    [Test]
    public void TestReceiveBuilderWithHeaderFragmentExplicitObjectMapper()
    {
        // Arrange
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);

        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage()
                .AddHeaderData("{\"message\": \"Hello Agenix!\"}")
                .SetHeader("operation", "foo"));

        // Act
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Header(new JsonSerializerHeaderDataBuilder(new TestRequest("Hello Agenix!"), _serializer)));

        // Assert
        var test = runner.GetTestCase();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.GetActionCount(), Is.EqualTo(1));
            Assert.That(test.GetActions()[0], Is.InstanceOf<ReceiveMessageAction>());
        }

        var action = (ReceiveMessageAction)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.Name, Is.EqualTo("receive"));

            Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.JSON)));
            Assert.That(action.Endpoint, Is.SameAs(_messageEndpoint.Object));
            Assert.That(action.ValidationContexts.Count, Is.EqualTo(1));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.ValidationContexts, Has.Some.InstanceOf<HeaderValidationContext>());

            Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        }

        var messageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        Assert.That(messageBuilder.BuildMessageHeaderData(Context), Has.Count.EqualTo(1));
        Assert.That(messageBuilder.BuildMessageHeaderData(Context)[0],
            Is.EqualTo("{\"message\":\"Hello Agenix!\"}"));
    }

    [Test]
    public void TestReceiveBuilderWithHeaderFragmentExplicitObjectMapperName()
    {
        // Arrange
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);

        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage()
                .AddHeaderData("{\"message\": \"Hello Agenix!\"}")
                .SetHeader("operation", "foo"));

        // Set up a reference resolver for various objects
        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());

        // This is the key difference from the previous test - using named JsonSerializer instead of direct instance
        _referenceResolver.Setup(x => x.IsResolvable("myObjectMapper")).Returns(true);
        _referenceResolver.Setup(x => x.Resolve<JsonSerializer>("myObjectMapper")).Returns(_serializer);

        Context.SetReferenceResolver(_referenceResolver.Object);

        // Act
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Header(new JsonSerializerHeaderDataBuilder(new TestRequest("Hello Agenix!"), "myObjectMapper")));

        // Assert
        var test = runner.GetTestCase();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.GetActionCount(), Is.EqualTo(1));
            Assert.That(test.GetActions()[0], Is.InstanceOf<ReceiveMessageAction>());
        }

        var action = (ReceiveMessageAction)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.Name, Is.EqualTo("receive"));

            Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.JSON)));
            Assert.That(action.Endpoint, Is.SameAs(_messageEndpoint.Object));
            Assert.That(action.ValidationContexts.Count, Is.EqualTo(1));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.ValidationContexts, Has.Some.InstanceOf<HeaderValidationContext>());

            Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        }

        var messageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        Assert.That(messageBuilder.BuildMessageHeaderData(Context).Count, Is.EqualTo(1));
        Assert.That(messageBuilder.BuildMessageHeaderData(Context)[0],
            Is.EqualTo("{\"message\":\"Hello Agenix!\"}"));
    }

    [Test]
    public void TestReceiveBuilderWithHeaderResource()
    {
        // Arrange
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);

        // Set up mock to return different messages on consecutive calls
        _messageConsumer.SetupSequence(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "foo")
                .AddHeaderData("<Header><Name>operation</Name><Value>foo</Value></Header>"))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "bar")
                .AddHeaderData("<Header><Name>operation</Name><Value>bar</Value></Header>"));

        // Set up resource mock to return different content on consecutive calls
        _resource.Setup(x => x.Exists).Returns(true);
        _resource.SetupSequence(x => x.InputStream)
            .Returns(new MemoryStream(
                Encoding.UTF8.GetBytes("<Header><Name>operation</Name><Value>foo</Value></Header>")))
            .Returns(new MemoryStream(
                Encoding.UTF8.GetBytes("<Header><Name>operation</Name><Value>bar</Value></Header>")));

        // Act
        var runner = new DefaultTestCaseRunner(Context);

        // First, receive action with a separate body and header from the resource
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>")
            .Header(_resource.Object));

        // Second receive action with a complete message and header from the resource
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>"))
            .Header(_resource.Object));

        // Assert
        var test = runner.GetTestCase();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.GetActionCount(), Is.EqualTo(2));
            Assert.That(test.GetActions()[0], Is.InstanceOf<ReceiveMessageAction>());
            Assert.That(test.GetActions()[1], Is.InstanceOf<ReceiveMessageAction>());
        }

        // Assert first action
        var action = (ReceiveMessageAction)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.Name, Is.EqualTo("receive"));
            Assert.That(action.Endpoint, Is.SameAs(_messageEndpoint.Object));
            Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));

            Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        }

        var messageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(messageBuilder.BuildMessagePayload(Context, action.MessageType),
                Is.EqualTo("<TestRequest><Message>Hello World!</Message></TestRequest>"));
            Assert.That(messageBuilder.BuildMessageHeaderData(Context).Count, Is.EqualTo(1));
        }

        Assert.That(messageBuilder.BuildMessageHeaderData(Context)[0],
            Is.EqualTo("<Header><Name>operation</Name><Value>foo</Value></Header>"));

        // Assert second action
        var action2 = (ReceiveMessageAction)test.GetActions()[1];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action2.Name, Is.EqualTo("receive"));
            Assert.That(action2.Endpoint, Is.SameAs(_messageEndpoint.Object));
            Assert.That(action2.MessageType, Is.EqualTo(nameof(MessageType.XML)));

            Assert.That(action2.MessageBuilder, Is.InstanceOf<StaticMessageBuilder>());
        }

        var staticMessageBuilder = (StaticMessageBuilder)action2.MessageBuilder;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(staticMessageBuilder.GetMessage().Payload,
                Is.EqualTo("<TestRequest><Message>Hello World!</Message></TestRequest>"));
            Assert.That(staticMessageBuilder.BuildMessageHeaderData(Context), Has.Count.EqualTo(1));
        }

        Assert.That(staticMessageBuilder.BuildMessageHeaderData(Context)[0],
            Is.EqualTo("<Header><Name>operation</Name><Value>bar</Value></Header>"));
    }

    [Test]
    public void TestReceiveBuilderWithMultipleHeaderResource()
    {
        // Arrange
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);

        // Configure the message consumer to return a message with multiple header data elements
        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "foo")
                .AddHeaderData("<Header><Name>operation</Name><Value>sayHello</Value></Header>")
                .AddHeaderData("<Header><Name>operation</Name><Value>foo</Value></Header>")
                .AddHeaderData("<Header><Name>operation</Name><Value>bar</Value></Header>"));

        // Set up resource mock to return different content on consecutive calls
        _resource.Setup(x => x.Exists).Returns(true);
        _resource.SetupSequence(x => x.InputStream)
            .Returns(new MemoryStream("<Header><Name>operation</Name><Value>foo</Value></Header>"u8.ToArray()))
            .Returns(new MemoryStream("<Header><Name>operation</Name><Value>bar</Value></Header>"u8.ToArray()))
            .Returns(new MemoryStream("<Header><Name>operation</Name><Value>foo</Value></Header>"u8.ToArray()))
            .Returns(new MemoryStream("<Header><Name>operation</Name><Value>bar</Value></Header>"u8.ToArray()));

        // Act
        var runner = new DefaultTestCaseRunner(Context);

        // First receive action with DefaultMessageBuilder
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>")
            .Header("<Header><Name>operation</Name><Value>sayHello</Value></Header>")
            .Header(_resource.Object)
            .Header(_resource.Object));

        // Second receive action with StaticMessageBuilder
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>"))
            .Header("<Header><Name>operation</Name><Value>sayHello</Value></Header>")
            .Header(_resource.Object)
            .Header(_resource.Object));

        // Assert
        var test = runner.GetTestCase();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.GetActionCount(), Is.EqualTo(2));
            Assert.That(test.GetActions()[0], Is.InstanceOf<ReceiveMessageAction>());
            Assert.That(test.GetActions()[1], Is.InstanceOf<ReceiveMessageAction>());
        }

        // Assert the first action with DefaultMessageBuilder
        var action = (ReceiveMessageAction)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.Name, Is.EqualTo("receive"));
            Assert.That(action.Endpoint, Is.SameAs(_messageEndpoint.Object));
            Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));

            Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        }

        var messageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(messageBuilder.BuildMessagePayload(Context, action.MessageType),
                Is.EqualTo("<TestRequest><Message>Hello World!</Message></TestRequest>"));

            // Verify multiple header data elements
            Assert.That(messageBuilder.BuildMessageHeaderData(Context), Has.Count.EqualTo(3));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(messageBuilder.BuildMessageHeaderData(Context)[0],
                Is.EqualTo("<Header><Name>operation</Name><Value>sayHello</Value></Header>"));
            Assert.That(messageBuilder.BuildMessageHeaderData(Context)[1],
                Is.EqualTo("<Header><Name>operation</Name><Value>foo</Value></Header>"));
            Assert.That(messageBuilder.BuildMessageHeaderData(Context)[2],
                Is.EqualTo("<Header><Name>operation</Name><Value>bar</Value></Header>"));
        }

        // Assert the second action with StaticMessageBuilder
        var action2 = (ReceiveMessageAction)test.GetActions()[1];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action2.Name, Is.EqualTo("receive"));
            Assert.That(action2.Endpoint, Is.SameAs(_messageEndpoint.Object));
            Assert.That(action2.MessageType, Is.EqualTo(nameof(MessageType.XML)));

            Assert.That(action2.MessageBuilder, Is.InstanceOf<StaticMessageBuilder>());
        }

        var staticMessageBuilder = (StaticMessageBuilder)action2.MessageBuilder;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(staticMessageBuilder.GetMessage().Payload,
                Is.EqualTo("<TestRequest><Message>Hello World!</Message></TestRequest>"));

            // Verify multiple header data elements for the static message builder
            Assert.That(staticMessageBuilder.BuildMessageHeaderData(Context), Has.Count.EqualTo(3));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(staticMessageBuilder.BuildMessageHeaderData(Context)[0],
                Is.EqualTo("<Header><Name>operation</Name><Value>sayHello</Value></Header>"));
            Assert.That(staticMessageBuilder.BuildMessageHeaderData(Context)[1],
                Is.EqualTo("<Header><Name>operation</Name><Value>foo</Value></Header>"));
            Assert.That(staticMessageBuilder.BuildMessageHeaderData(Context)[2],
                Is.EqualTo("<Header><Name>operation</Name><Value>bar</Value></Header>"));
        }
    }

    [Test]
    public void TestReceiveBuilderExtractJsonPathFromJsonPathExpression()
    {
        // Arrange
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(e => e.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(e => e.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);

        _messageConsumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage(
                    "{\"text\":\"Hello World!\", \"person\":{\"name\":\"John\",\"surname\":\"Doe\"}, \"index\":5, \"id\":\"x123456789x\"}")
                .SetHeader("operation", "sayHello"));

        _referenceResolver.Setup(r => r.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(r => r.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(r => r.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(r => r.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());

        Context.SetReferenceResolver(_referenceResolver.Object);
        var runner = new DefaultTestCaseRunner(Context);

        // Act
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Type(MessageType.JSON)
            .Body(
                "{\"text\":\"Hello World!\", \"person\":{\"name\":\"John\",\"surname\":\"Doe\"}, \"index\":5, \"id\":\"x123456789x\"}")
            .Extract(JsonPathSupport.JsonPath()
                .Expression("$.text", "text")
                .Expression("$.ToString()", "payload")
                .Expression("$.person", "person")
            ));

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(Context.GetVariable("text"), Is.Not.Null);
            Assert.That(Context.GetVariable("person"), Is.Not.Null);
            Assert.That(Context.GetVariable("payload"), Is.Not.Null);
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Context.GetVariable("text"), Is.EqualTo("Hello World!"));
            Assert.That(Context.GetVariable("payload"),
                Is.EqualTo(
                    "{\"text\":\"Hello World!\",\"person\":{\"name\":\"John\",\"surname\":\"Doe\"},\"index\":5,\"id\":\"x123456789x\"}"));
            Assert.That(Context.GetVariable("person"), Does.Contain("\"John\""));
        }

        var test = runner.GetTestCase();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.GetActionCount(), Is.EqualTo(1));
            Assert.That(test.GetActions()[0], Is.TypeOf<ReceiveMessageAction>());
        }

        var action = (ReceiveMessageAction)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.Name, Is.EqualTo("receive"));

            Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.JSON)));
            Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));

            Assert.That(action.VariableExtractors, Has.Count.EqualTo(1));
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
    public void TestReceiveBuilderWithJsonPathExpressions()
    {
        // Arrange
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(e => e.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(e => e.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);

        _messageConsumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage(
                    "{\"text\":\"Hello World!\", \"person\":{\"name\":\"John\",\"surname\":\"Doe\",\"active\": true}, \"index\":5, \"id\":\"x123456789x\"}")
                .SetHeader("operation", "sayHello"));

        // Act
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Type(MessageType.JSON)
            .Body(
                "{\"text\":\"Hello World!\", \"person\":{\"name\":\"John\",\"surname\":\"Doe\",\"active\": true}, \"index\":5, \"id\":\"x123456789x\"}")
            .Validate(JsonPathSupport.JsonPath()
                .Expression("$.person.name", "John")
                .Expression("$.person.active", true)
                .Expression("$.id", Matches.AnyOf(Contains.String("123456789"), IsNull.Null()))
                .Expression("$.text", "Hello World!")
                .Expression("$.index", 5)));

        // Assert
        var test = runner.GetTestCase();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.GetActionCount(), Is.EqualTo(1));
            Assert.That(test.GetActions()[0], Is.TypeOf<ReceiveMessageAction>());
        }

        var action = (ReceiveMessageAction)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.Name, Is.EqualTo("receive"));

            Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.JSON)));
            Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
            Assert.That(action.ValidationContexts, Has.Count.EqualTo(3));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.ValidationContexts.Any(ctx => ctx is HeaderValidationContext), Is.True);
            Assert.That(action.ValidationContexts.Any(ctx => ctx is JsonMessageValidationContext), Is.True);
            Assert.That(action.ValidationContexts.Any(ctx => ctx is JsonPathMessageValidationContext), Is.True);
        }

        var validationContext = action.ValidationContexts
            .OfType<JsonPathMessageValidationContext>()
            .FirstOrDefault();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(validationContext, Is.Not.Null, "Missing validation context");

            Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        }

        Debug.Assert(validationContext != null, nameof(validationContext) + " != null");
        Assert.That(validationContext.JsonPathExpressions, Has.Count.EqualTo(5));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(validationContext.JsonPathExpressions["$.person.name"], Is.EqualTo("John"));
            Assert.That(validationContext.JsonPathExpressions["$.person.active"], Is.EqualTo(true));
            Assert.That(validationContext.JsonPathExpressions["$.text"], Is.EqualTo("Hello World!"));
            Assert.That(validationContext.JsonPathExpressions["$.index"], Is.EqualTo(5));
            Assert.That(validationContext.JsonPathExpressions["$.id"], Is.TypeOf<AnyOfMatcher<string>>());
        }
    }

    [Test]
    public void TestReceiveBuilderWithMultipleJsonPathExpressions()
    {
        // Arrange
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(e => e.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(e => e.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);

        _messageConsumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("{\"text\":\"Agenix rocks!\", \"user\": \"andy\"}")
                .SetHeader("operation", "sayHello"));

        // Act
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Type(MessageType.JSON)
            .Body("{\"text\":\"Agenix rocks!\", \"user\":\"andy\"}")
            .Validate(JsonPathSupport.JsonPath()
                .Expression("$.user", "andy"))
            .Validate(JsonPathSupport.JsonPath()
                .Expression("$.text", "Agenix rocks!")));

        // Assert
        var test = runner.GetTestCase();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.GetActionCount(), Is.EqualTo(1));
            Assert.That(test.GetActions()[0], Is.TypeOf<ReceiveMessageAction>());
        }

        var action = (ReceiveMessageAction)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.Name, Is.EqualTo("receive"));

            Assert.That(action.MessageType, Is.EqualTo(MessageType.JSON.ToString()));
            Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
            Assert.That(action.ValidationContexts, Has.Count.EqualTo(4));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.ValidationContexts.Any(ctx => ctx is HeaderValidationContext), Is.True);
            Assert.That(action.ValidationContexts.Any(ctx => ctx is JsonMessageValidationContext), Is.True);
            Assert.That(action.ValidationContexts.Count(ctx => ctx is JsonPathMessageValidationContext), Is.EqualTo(2));
        }

        // Combine all JsonPath expressions from both validation contexts
        var jsonPathExpressions = action.ValidationContexts
            .Where(ctx => ctx is JsonPathMessageValidationContext)
            .Cast<JsonPathMessageValidationContext>()
            .Select(ctx => ctx.JsonPathExpressions)
            .Aggregate(new Dictionary<string, object>(), (acc, map) =>
            {
                foreach (var item in map)
                {
                    acc[item.Key] = item.Value;
                }

                return acc;
            });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(jsonPathExpressions, Is.Not.Null, "Missing validation context");
            Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        }

        Assert.That(jsonPathExpressions.Count, Is.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(jsonPathExpressions["$.user"], Is.EqualTo("andy"));
            Assert.That(jsonPathExpressions["$.text"], Is.EqualTo("Agenix rocks!"));
        }
    }

    [Test]
    public void TestReceiveBuilderWithJsonPathExpressionsFailure()
    {
        // Arrange
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(e => e.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(e => e.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);

        _messageConsumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage(
                    "{\"text\":\"Hello World!\", \"person\":{\"name\":\"John\",\"surname\":\"Doe\"}, \"index\":5, \"id\":\"x123456789x\"}")
                .SetHeader("operation", "sayHello"));

        // Act & Assert
        var runner = new DefaultTestCaseRunner(Context);

        Assert.Throws<TestCaseFailedException>(() =>
            runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
                .Message()
                .Type(MessageType.JSON)
                .Body(
                    "{\"text\":\"Hello World!\", \"person\":{\"name\":\"John\",\"surname\":\"Doe\"}, \"index\":5, \"id\":\"x123456789x\"}")
                .Validate(JsonPathSupport.JsonPath()
                    .Expression("$.person.name", "John")
                    .Expression("$.text", "Hello Agenix!")))
        );
    }

    [Test]
    public void TestReceiveBuilderWithJsonValidationFailure()
    {
        // Arrange
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(e => e.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(e => e.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);

        _messageConsumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage(
                    "{\"text\":\"Hello World!\", \"person\":{\"name\":\"John\",\"surname\":\"Doe\"}, \"index\":5, \"id\":\"x123456789x\"}")
                .SetHeader("operation", "sayHello"));

        // Act & Assert
        var runner = new DefaultTestCaseRunner(Context);

        Assert.Throws<TestCaseFailedException>(() =>
            runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
                .Message()
                .Type(MessageType.JSON)
                .Body(
                    "{\"text\":\"Hello Agenix!\", \"person\":{\"name\":\"John\",\"surname\":\"Doe\"}, \"index\":5, \"id\":\"x123456789x\"}")
                .Validate(JsonSupport.Json()
                    .Expression("$.person.name", "John")
                    .Expression("$.text", "Hello World!")
                )));
    }

    [Test]
    public void TestReceiveBuilderWithIgnoreElementsJson()
    {
        // Arrange
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(e => e.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(e => e.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);

        _messageConsumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage(
                    "{\"text\":\"Hello World!\", \"person\":{\"name\":\"John\",\"surname\":\"Doe\"}, \"index\":5, \"id\":\"x123456789x\"}")
                .SetHeader("operation", "sayHello"));

        // Act
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Type(MessageType.JSON)
            .Body(
                "{\"text\":\"?\", \"person\":{\"name\":\"John\",\"surname\":\"?\"}, \"index\":0, \"id\":\"x123456789x\"}")
            .Validate(JsonSupport.Json()
                .Ignore("$..text")
                .Ignore("$.person.surname")
                .Ignore("$.index")));

        // Assert
        var test = runner.GetTestCase();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.GetActionCount(), Is.EqualTo(1));
            Assert.That(test.GetActions()[0], Is.InstanceOf<ReceiveMessageAction>());
        }

        var action = (ReceiveMessageAction)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.Name, Is.EqualTo("receive"));

            Assert.That(action.MessageType, Is.EqualTo("JSON"));
            Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
            Assert.That(action.ValidationContexts, Has.Count.EqualTo(2));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.ValidationContexts.Any(c => c is HeaderValidationContext), Is.True);
            Assert.That(action.ValidationContexts.Any(c => c is JsonMessageValidationContext), Is.True);
        }

        var validationContext = action.ValidationContexts
            .OfType<JsonMessageValidationContext>()
            .FirstOrDefault();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(validationContext, Is.Not.Null, "Missing validation context");

            Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        }

        var messagePayload = ((DefaultMessageBuilder)action.MessageBuilder)
            .BuildMessagePayload(Context, action.MessageType);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(messagePayload,
                Is.EqualTo(
                    "{\"text\":\"?\", \"person\":{\"name\":\"John\",\"surname\":\"?\"}, \"index\":0, \"id\":\"x123456789x\"}"));

            Debug.Assert(validationContext != null, nameof(validationContext) + " != null");
            Assert.That(validationContext.IgnoreExpressions, Has.Count.EqualTo(3));
        }

        Assert.That(validationContext.IgnoreExpressions, Does.Contain("$..text"));
        Assert.That(validationContext.IgnoreExpressions, Does.Contain("$.person.surname"));
        Assert.That(validationContext.IgnoreExpressions, Does.Contain("$.index"));
    }

    [Test]
    public void TestReceiveBuilderWithJsonSchemaRepository()
    {
        // Arrange
        var schema = new SimpleJsonSchema();
        var schemaRepository = new JsonSchemaRepository();
        schemaRepository.AddSchema(schema);
        schemaRepository.SetName("customJsonSchemaRepository");

        // Arrange
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _referenceResolver.Setup(x => x.ResolveAll<JsonSchemaRepository>())
            .Returns(new Dictionary<string, JsonSchemaRepository>
            {
                { "customJsonSchemaRepository", schemaRepository }
            });
        _messageEndpoint.Setup(e => e.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(e => e.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);

        _messageConsumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("{}")
                .SetHeader("operation", "sayHello"));

        Context.SetReferenceResolver(_referenceResolver.Object);

        // Act
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body("{}")
            .Validate(JsonSupport.Json()
                .SchemaRepository("customJsonSchemaRepository")
            ));

        // Assert
        var test = runner.GetTestCase();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.GetActionCount(), Is.EqualTo(1));
            Assert.That(test.GetActions()[0], Is.InstanceOf<ReceiveMessageAction>());
        }

        var action = (ReceiveMessageAction)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.Name, Is.EqualTo("receive"));

            Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
            Assert.That(action.ValidationContexts.Count, Is.EqualTo(2));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.ValidationContexts.Any(c => c is HeaderValidationContext), Is.True);
            Assert.That(action.ValidationContexts.Any(c => c is JsonMessageValidationContext), Is.True);
        }

        var validationContext = action.ValidationContexts
            .OfType<JsonMessageValidationContext>()
            .FirstOrDefault();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(validationContext, Is.Not.Null, "Missing validation context");

            Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        }

        var messagePayload = ((DefaultMessageBuilder)action.MessageBuilder)
            .BuildMessagePayload(Context, action.MessageType);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(messagePayload, Is.EqualTo("{}"));

            Debug.Assert(validationContext != null, nameof(validationContext) + " != null");
            Assert.That(validationContext.SchemaRepository, Is.EqualTo("customJsonSchemaRepository"));
        }
    }

    [Test]
    public void TestReceiveBuilderWithJsonSchema()
    {
        // Arrange
        var schema = new SimpleJsonSchema();
        _referenceResolver.Setup(x => x.Resolve<SimpleJsonSchema>())
            .Returns(schema);

        // Arrange
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(e => e.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(e => e.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);

        _messageConsumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("{}")
                .SetHeader("operation", "sayHello"));

        Context.SetReferenceResolver(_referenceResolver.Object);

        // Act
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body("{}")
            .Validate(JsonSupport.Json()
                .Schema("jsonTestSchema")
            ));

        // Assert
        var test = runner.GetTestCase();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.GetActionCount(), Is.EqualTo(1));
            Assert.That(test.GetActions()[0], Is.InstanceOf<ReceiveMessageAction>());
        }

        var action = (ReceiveMessageAction)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.Name, Is.EqualTo("receive"));

            Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
            Assert.That(action.ValidationContexts, Has.Count.EqualTo(2));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.ValidationContexts.Any(c => c is HeaderValidationContext), Is.True);
            Assert.That(action.ValidationContexts.Any(c => c is JsonMessageValidationContext), Is.True);
        }

        var validationContext = action.ValidationContexts
            .OfType<JsonMessageValidationContext>()
            .FirstOrDefault();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(validationContext, Is.Not.Null, "Missing validation context");

            Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        }

        var messagePayload = ((DefaultMessageBuilder)action.MessageBuilder)
            .BuildMessagePayload(Context, action.MessageType);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(messagePayload, Is.EqualTo("{}"));

            Debug.Assert(validationContext != null, nameof(validationContext) + " != null");
            Assert.That(validationContext.Schema, Is.EqualTo("jsonTestSchema"));
        }
    }

    [Test]
    public void TestActivateSchemaValidation()
    {
        // Arrange
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(e => e.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(e => e.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);

        _messageConsumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("{}")
                .SetHeader("operation", "sayHello"));

        // Act
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body("{}")
            .Validate(JsonSupport.Json()
                .SchemaValidation(true)));

        // Assert
        var test = runner.GetTestCase();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.GetActionCount(), Is.EqualTo(1));
            Assert.That(test.GetActions()[0], Is.InstanceOf<ReceiveMessageAction>());
        }

        var action = (ReceiveMessageAction)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.Name, Is.EqualTo("receive"));

            Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
            Assert.That(action.ValidationContexts, Has.Count.EqualTo(2));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.ValidationContexts.Any(c => c is HeaderValidationContext), Is.True);
            Assert.That(action.ValidationContexts.Any(c => c is JsonMessageValidationContext), Is.True);
        }

        var jsonMessageValidationContext = action.ValidationContexts
            .OfType<JsonMessageValidationContext>()
            .FirstOrDefault();

        Assert.That(jsonMessageValidationContext, Is.Not.Null, "Missing validation context");
        Assert.That(jsonMessageValidationContext.IsSchemaValidationEnabled, Is.True);
    }

    [Test]
    public void TestDeactivateSchemaValidation()
    {
        // Arrange
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(e => e.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(e => e.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(c => c.Timeout).Returns(100L);

        _messageConsumer.Setup(c => c.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("{}")
                .SetHeader("operation", "sayHello"));

        // Act
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body("{}")
            .Validate(JsonSupport.Json()
                .SchemaValidation(false)));

        // Assert
        var test = runner.GetTestCase();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.GetActionCount(), Is.EqualTo(1));
            Assert.That(test.GetActions()[0], Is.InstanceOf<ReceiveMessageAction>());
        }

        var action = (ReceiveMessageAction)test.GetActions()[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.Name, Is.EqualTo("receive"));

            Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
            Assert.That(action.ValidationContexts, Has.Count.EqualTo(2));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(action.ValidationContexts.Any(c => c is HeaderValidationContext), Is.True);
            Assert.That(action.ValidationContexts.Any(c => c is JsonMessageValidationContext), Is.True);
        }

        var jsonMessageValidationContext = action.ValidationContexts
            .OfType<JsonMessageValidationContext>()
            .FirstOrDefault();

        Assert.That(jsonMessageValidationContext, Is.Not.Null, "Missing validation context");
        Assert.That(jsonMessageValidationContext.IsSchemaValidationEnabled, Is.False);
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
