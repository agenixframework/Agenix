using Agenix.Api.Endpoint;
using Agenix.Api.IO;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Api.Report;
using Agenix.Api.Spi;
using Agenix.Api.Validation.Context;
using Agenix.Api.Variable.Dictionary;
using Agenix.Api.Xml;
using Agenix.Core;
using Agenix.Core.Actions;
using Agenix.Core.Container;
using Agenix.Core.Dsl;
using Agenix.Core.Message;
using Agenix.Core.Message.Builder;
using Agenix.Core.Validation;
using Agenix.Core.Validation.Builder;
using Agenix.Core.Validation.Xml;
using Agenix.Core.Variable;
using Agenix.Core.Xml;
using Agenix.Validation.Xml.Dsl;
using Agenix.Validation.Xml.Message.Builder;
using Agenix.Validation.Xml.Tests.Message;
using Agenix.Validation.Xml.Validation.Xml;
using Agenix.Validation.Xml.Variable.Dictionary.Xml;
using Moq;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Xml.Tests.Actions.Dsl;

public class ReceiveMessageActionBuilderTest : AbstractNUnitSetUp
{
    private readonly Mock<IEndpointConfiguration> _configuration = new();

    private readonly Xml2Marshaller _marshaller = new(typeof(TestRequest));
    private readonly Mock<IConsumer> _messageConsumer = new();
    private readonly Mock<IEndpoint> _messageEndpoint = new();
    private readonly Mock<IReferenceResolver> _referenceResolver = new();
    private readonly Mock<IResource> _resource = new();

    [SetUp]
    public void SetUp()
    {
        _marshaller.SetProperty("omitxmldeclaration", "true");
        _marshaller.SetProperty("stripnamespaces", "true");
        _marshaller.SetProperty("removeemptylines", "true");
    }

    [Test]
    public void TestReceiveEmpty()
    {
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);
        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<Message>Hello</Message>"));

        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));

        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.ValidationContexts.Count, Is.EqualTo(1));
        Assert.That(action.ValidationContexts.Any(x => x is HeaderValidationContext), Is.True);
    }

    [Test]
    public void TestReceiveBuilder()
    {
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);
        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("Foo").SetHeader("operation", "foo"));

        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message(new DefaultMessage("Foo").SetHeader("operation", "foo"))
            .Type(MessageType.PLAINTEXT));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));

        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.PLAINTEXT)));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.ValidationContexts.Count, Is.EqualTo(2));
        Assert.That(action.ValidationContexts.Any(x => x is HeaderValidationContext), Is.True);
        Assert.That(action.ValidationContexts.Any(x => x is DefaultMessageValidationContext), Is.True);

        Assert.That(action.MessageBuilder, Is.InstanceOf<StaticMessageBuilder>());
        Assert.That(((StaticMessageBuilder)action.MessageBuilder).GetMessage().Payload, Is.EqualTo("Foo"));
        Assert.That(((StaticMessageBuilder)action.MessageBuilder).GetMessage().GetHeader("operation"), Is.Not.Null);
    }

    [Test]
    public void TestReceiveBuilderWithPayloadModel()
    {
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);
        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello Agenix!</Message></TestRequest>")
                .SetHeader("operation", "foo"));

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());
        _referenceResolver.Setup(x => x.ResolveAll<IMarshaller>())
            .Returns(new Dictionary<string, IMarshaller> { { "marshaller", _marshaller } });
        _referenceResolver.Setup(x => x.Resolve<IMarshaller>()).Returns(_marshaller);

        Context.SetReferenceResolver(_referenceResolver.Object);
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body(new MarshallingPayloadBuilder(new TestRequest("Hello Agenix!"))));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));

        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.ValidationContexts.Count, Is.EqualTo(2));
        Assert.That(action.ValidationContexts.Any(x => x is HeaderValidationContext), Is.True);
        Assert.That(action.ValidationContexts.Any(x => x is DefaultMessageValidationContext), Is.True);

        Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        Assert.That(((DefaultMessageBuilder)action.MessageBuilder).BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
    }

    [Test]
    public void TestReceiveBuilderWithPayloadModelExplicitMarshaller()
    {
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);
        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello Agenix!</Message></TestRequest>")
                .SetHeader("operation", "foo"));

        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body(new MarshallingPayloadBuilder(new TestRequest("Hello Agenix!"), _marshaller)));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));

        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.ValidationContexts.Count, Is.EqualTo(2));
        Assert.That(action.ValidationContexts.Any(x => x is HeaderValidationContext), Is.True);
        Assert.That(action.ValidationContexts.Any(x => x is DefaultMessageValidationContext), Is.True);

        Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        Assert.That(((DefaultMessageBuilder)action.MessageBuilder).BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
    }

    [Test]
    public void TestReceiveBuilderWithPayloadModelExplicitMarshallerName()
    {
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);
        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello Agenix!</Message></TestRequest>")
                .SetHeader("operation", "foo"));

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());
        _referenceResolver.Setup(x => x.IsResolvable("myMarshaller")).Returns(true);
        _referenceResolver.Setup(x => x.Resolve<IMarshaller>("myMarshaller")).Returns(_marshaller);

        Context.SetReferenceResolver(_referenceResolver.Object);
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body(new MarshallingPayloadBuilder(new TestRequest("Hello Agenix!"), "myMarshaller")));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));

        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.ValidationContexts.Count, Is.EqualTo(2));
        Assert.That(action.ValidationContexts.Any(x => x is HeaderValidationContext), Is.True);
        Assert.That(action.ValidationContexts.Any(x => x is DefaultMessageValidationContext), Is.True);

        Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        Assert.That(((DefaultMessageBuilder)action.MessageBuilder).BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
    }

    [Test]
    public void TestReceiveBuilderWithPayloadString()
    {
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);
        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "foo"));

        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>"));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));

        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.ValidationContexts.Count, Is.EqualTo(2));
        Assert.That(action.ValidationContexts.Any(x => x is HeaderValidationContext), Is.True);
        Assert.That(action.ValidationContexts.Any(x => x is XmlMessageValidationContext), Is.True);

        Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        Assert.That(((DefaultMessageBuilder)action.MessageBuilder).BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo("<TestRequest><Message>Hello World!</Message></TestRequest>"));
    }

    [Test]
    public void TestReceiveBuilderWithPayloadResource()
    {
        _resource.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);

        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "foo"));

        _resource.Setup(x => x.Exists).Returns(true);
        _resource.Setup(x => x.InputStream)
            .Returns(new MemoryStream("<TestRequest><Message>Hello World!</Message></TestRequest>"u8.ToArray()));

        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body(_resource.Object));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));

        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.ValidationContexts.Count, Is.EqualTo(2));
        Assert.That(action.ValidationContexts.Any(x => x is HeaderValidationContext), Is.True);
        Assert.That(action.ValidationContexts.Any(x => x is XmlMessageValidationContext), Is.True);

        Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        Assert.That(((DefaultMessageBuilder)action.MessageBuilder).BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo("<TestRequest><Message>Hello World!</Message></TestRequest>"));
    }

    [Test]
    public void TestReceiveBuilderWithEndpointName()
    {
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);
        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "foo"));

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<IEndpoint>("fooMessageEndpoint")).Returns(_messageEndpoint.Object);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());

        Context.SetReferenceResolver(_referenceResolver.Object);
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive("fooMessageEndpoint")
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>"));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.EndpointUri, Is.EqualTo("fooMessageEndpoint"));
        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));
    }

    [Test]
    public void TestReceiveBuilderWithTimeout()
    {
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "foo"));

        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>")
            .Timeout(1000L));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.ReceiveTimeout, Is.EqualTo(1000L));
    }

    [Test]
    public void TestReceiveBuilderWithHeaders()
    {
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);
        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("some", "value")
                .SetHeader("operation", "sayHello")
                .SetHeader("foo", "bar"));

        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>")
            .Headers(new Dictionary<string, object> { { "some", "value" } })
            .Header("operation", "sayHello")
            .Header("foo", "bar"));

        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Header("operation", "sayHello")
            .Header("foo", "bar")
            .Headers(new Dictionary<string, object> { { "some", "value" } })
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>"));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(2));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));
        Assert.That(test.GetActions()[1].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));

        Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        Assert.That(((DefaultMessageBuilder)action.MessageBuilder).BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo("<TestRequest><Message>Hello World!</Message></TestRequest>"));
        Assert.That(((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaders(Context).ContainsKey("some"),
            Is.True);
        Assert.That(
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaders(Context).ContainsKey("operation"),
            Is.True);
        Assert.That(((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaders(Context).ContainsKey("foo"),
            Is.True);

        action = (ReceiveMessageAction)test.GetActions()[1];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));

        Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        Assert.That(((DefaultMessageBuilder)action.MessageBuilder).BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo("<TestRequest><Message>Hello World!</Message></TestRequest>"));
        Assert.That(((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaders(Context).ContainsKey("some"),
            Is.True);
        Assert.That(
            ((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaders(Context).ContainsKey("operation"),
            Is.True);
        Assert.That(((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaders(Context).ContainsKey("foo"),
            Is.True);
    }

    [Test]
    public void TestReceiveBuilderWithHeaderData()
    {
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);
        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "foo")
                .AddHeaderData("<Header><Name>operation</Name><Value>foo</Value></Header>"));

        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>")
            .Header("<Header><Name>operation</Name><Value>foo</Value></Header>"));

        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>"))
            .Header("<Header><Name>operation</Name><Value>foo</Value></Header>"));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(2));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));
        Assert.That(test.GetActions()[1].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));

        Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        Assert.That(((DefaultMessageBuilder)action.MessageBuilder).BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo("<TestRequest><Message>Hello World!</Message></TestRequest>"));
        Assert.That(((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(Context).Count,
            Is.EqualTo(1));
        Assert.That(((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(Context)[0],
            Is.EqualTo("<Header><Name>operation</Name><Value>foo</Value></Header>"));

        action = (ReceiveMessageAction)test.GetActions()[1];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));

        Assert.That(action.MessageBuilder, Is.InstanceOf<StaticMessageBuilder>());
        Assert.That(((StaticMessageBuilder)action.MessageBuilder).GetMessage().Payload,
            Is.EqualTo("<TestRequest><Message>Hello World!</Message></TestRequest>"));
        Assert.That(((StaticMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(Context).Count, Is.EqualTo(1));
        Assert.That(((StaticMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(Context)[0],
            Is.EqualTo("<Header><Name>operation</Name><Value>foo</Value></Header>"));
    }

    [Test]
    public void TestReceiveBuilderWithMultipleHeaderData()
    {
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);
        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "foo")
                .AddHeaderData("<Header><Name>operation</Name><Value>foo1</Value></Header>")
                .AddHeaderData("<Header><Name>operation</Name><Value>foo2</Value></Header>"));

        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>")
            .Header("<Header><Name>operation</Name><Value>foo1</Value></Header>")
            .Header("<Header><Name>operation</Name><Value>foo2</Value></Header>"));

        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>"))
            .Header("<Header><Name>operation</Name><Value>foo1</Value></Header>")
            .Header("<Header><Name>operation</Name><Value>foo2</Value></Header>"));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(2));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));
        Assert.That(test.GetActions()[1].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));

        Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        Assert.That(((DefaultMessageBuilder)action.MessageBuilder).BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo("<TestRequest><Message>Hello World!</Message></TestRequest>"));
        Assert.That(((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(Context).Count,
            Is.EqualTo(2));
        Assert.That(((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(Context)[0],
            Is.EqualTo("<Header><Name>operation</Name><Value>foo1</Value></Header>"));
        Assert.That(((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(Context)[1],
            Is.EqualTo("<Header><Name>operation</Name><Value>foo2</Value></Header>"));

        action = (ReceiveMessageAction)test.GetActions()[1];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));

        Assert.That(action.MessageBuilder, Is.InstanceOf<StaticMessageBuilder>());
        Assert.That(((StaticMessageBuilder)action.MessageBuilder).GetMessage().Payload,
            Is.EqualTo("<TestRequest><Message>Hello World!</Message></TestRequest>"));
        Assert.That(((StaticMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(Context).Count, Is.EqualTo(2));
        Assert.That(((StaticMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(Context)[0],
            Is.EqualTo("<Header><Name>operation</Name><Value>foo1</Value></Header>"));
        Assert.That(((StaticMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(Context)[1],
            Is.EqualTo("<Header><Name>operation</Name><Value>foo2</Value></Header>"));
    }

    [Test]
    public void TestReceiveBuilderWithHeaderFragment()
    {
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);
        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage()
                .AddHeaderData("<TestRequest><Message>Hello Agenix!</Message></TestRequest>")
                .SetHeader("operation", "foo"));

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());
        _referenceResolver.Setup(x => x.ResolveAll<IMarshaller>())
            .Returns(new Dictionary<string, IMarshaller> { { "marshaller", _marshaller } });
        _referenceResolver.Setup(x => x.Resolve<IMarshaller>()).Returns(_marshaller);

        Context.SetReferenceResolver(_referenceResolver.Object);
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Type(MessageType.XML)
            .Header(new MarshallingHeaderDataBuilder(new TestRequest("Hello Agenix!"))));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.ValidationContexts.Count, Is.EqualTo(1));
        Assert.That(action.ValidationContexts.Any(x => x is HeaderValidationContext), Is.True);

        Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        Assert.That(((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(Context).Count,
            Is.EqualTo(1));
        Assert.That(((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(Context)[0],
            Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
    }

    [Test]
    public void TestReceiveBuilderWithHeaderFragmentExplicitMarshaller()
    {
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);
        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage()
                .AddHeaderData("<TestRequest><Message>Hello Agenix!</Message></TestRequest>")
                .SetHeader("operation", "foo"));

        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Type(MessageType.XML)
            .Header(new MarshallingHeaderDataBuilder(new TestRequest("Hello Agenix!"), _marshaller)));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.ValidationContexts.Count, Is.EqualTo(1));
        Assert.That(action.ValidationContexts.Any(x => x is HeaderValidationContext), Is.True);

        Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        Assert.That(((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(Context).Count,
            Is.EqualTo(1));
        Assert.That(((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(Context)[0],
            Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
    }

    [Test]
    public void TestReceiveBuilderWithHeaderFragmentExplicitMarshallerName()
    {
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);
        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage()
                .AddHeaderData("<TestRequest><Message>Hello Agenix!</Message></TestRequest>")
                .SetHeader("operation", "foo"));

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());
        _referenceResolver.Setup(x => x.IsResolvable("myMarshaller")).Returns(true);
        _referenceResolver.Setup(x => x.Resolve<IMarshaller>("myMarshaller")).Returns(_marshaller);

        Context.SetReferenceResolver(_referenceResolver.Object);
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Type(MessageType.XML)
            .Header(new MarshallingHeaderDataBuilder(new TestRequest("Hello Agenix!"), "myMarshaller")));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.ValidationContexts.Count, Is.EqualTo(1));
        Assert.That(action.ValidationContexts.Any(x => x is HeaderValidationContext), Is.True);

        Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        Assert.That(((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(Context).Count,
            Is.EqualTo(1));
        Assert.That(((DefaultMessageBuilder)action.MessageBuilder).BuildMessageHeaderData(Context)[0],
            Is.EqualTo("<TestRequest><Message>Hello Agenix!</Message></TestRequest>"));
    }

    [Test]
    public void TestReceiveBuilderWithHeaderResource()
    {
        // Reset mocks
        _resource.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Setup mocks
        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);

        _messageConsumer.SetupSequence(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "foo")
                .AddHeaderData("<Header><Name>operation</Name><Value>foo</Value></Header>"))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "bar")
                .AddHeaderData("<Header><Name>operation</Name><Value>bar</Value></Header>"));

        _resource.Setup(x => x.Exists).Returns(true);
        _resource.SetupSequence(x => x.InputStream)
            .Returns(new MemoryStream("<Header><Name>operation</Name><Value>foo</Value></Header>"u8.ToArray()))
            .Returns(new MemoryStream("<Header><Name>operation</Name><Value>bar</Value></Header>"u8.ToArray()));

        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>")
            .Header(_resource.Object));

        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>"))
            .Header(_resource.Object));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(2));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));
        Assert.That(test.GetActions()[1].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));

        Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        var defaultBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        Assert.That(defaultBuilder.BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo("<TestRequest><Message>Hello World!</Message></TestRequest>"));
        Assert.That(defaultBuilder.BuildMessageHeaderData(Context).Count, Is.EqualTo(1));
        Assert.That(defaultBuilder.BuildMessageHeaderData(Context)[0],
            Is.EqualTo("<Header><Name>operation</Name><Value>foo</Value></Header>"));

        action = (ReceiveMessageAction)test.GetActions()[1];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.MessageType, Is.EqualTo(MessageType.XML.ToString()));

        Assert.That(action.MessageBuilder, Is.InstanceOf<StaticMessageBuilder>());
        var staticBuilder = (StaticMessageBuilder)action.MessageBuilder;
        Assert.That(staticBuilder.GetMessage().Payload,
            Is.EqualTo("<TestRequest><Message>Hello World!</Message></TestRequest>"));
        Assert.That(staticBuilder.BuildMessageHeaderData(Context).Count, Is.EqualTo(1));
        Assert.That(staticBuilder.BuildMessageHeaderData(Context)[0],
            Is.EqualTo("<Header><Name>operation</Name><Value>bar</Value></Header>"));
    }

    [Test]
    public void TestReceiveBuilderWithMultipleHeaderResource()
    {
        // Reset mocks
        _resource.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Setup mocks
        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);

        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "foo")
                .AddHeaderData("<Header><Name>operation</Name><Value>sayHello</Value></Header>")
                .AddHeaderData("<Header><Name>operation</Name><Value>foo</Value></Header>")
                .AddHeaderData("<Header><Name>operation</Name><Value>bar</Value></Header>"));

        _resource.Setup(x => x.Exists).Returns(true);
        _resource.SetupSequence(x => x.InputStream)
            .Returns(new MemoryStream("<Header><Name>operation</Name><Value>foo</Value></Header>"u8.ToArray()))
            .Returns(new MemoryStream("<Header><Name>operation</Name><Value>bar</Value></Header>"u8.ToArray()))
            .Returns(new MemoryStream("<Header><Name>operation</Name><Value>foo</Value></Header>"u8.ToArray()))
            .Returns(new MemoryStream("<Header><Name>operation</Name><Value>bar</Value></Header>"u8.ToArray()));

        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>")
            .Header("<Header><Name>operation</Name><Value>sayHello</Value></Header>")
            .Header(_resource.Object)
            .Header(_resource.Object));

        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>"))
            .Header("<Header><Name>operation</Name><Value>sayHello</Value></Header>")
            .Header(_resource.Object)
            .Header(_resource.Object));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(2));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));
        Assert.That(test.GetActions()[1].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));

        Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        var defaultBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        Assert.That(defaultBuilder.BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo("<TestRequest><Message>Hello World!</Message></TestRequest>"));
        Assert.That(defaultBuilder.BuildMessageHeaderData(Context).Count, Is.EqualTo(3));
        Assert.That(defaultBuilder.BuildMessageHeaderData(Context)[0],
            Is.EqualTo("<Header><Name>operation</Name><Value>sayHello</Value></Header>"));
        Assert.That(defaultBuilder.BuildMessageHeaderData(Context)[1],
            Is.EqualTo("<Header><Name>operation</Name><Value>foo</Value></Header>"));
        Assert.That(defaultBuilder.BuildMessageHeaderData(Context)[2],
            Is.EqualTo("<Header><Name>operation</Name><Value>bar</Value></Header>"));

        action = (ReceiveMessageAction)test.GetActions()[1];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));

        Assert.That(action.MessageBuilder, Is.InstanceOf<StaticMessageBuilder>());
        var staticBuilder = (StaticMessageBuilder)action.MessageBuilder;
        Assert.That(staticBuilder.GetMessage().Payload,
            Is.EqualTo("<TestRequest><Message>Hello World!</Message></TestRequest>"));
        Assert.That(staticBuilder.BuildMessageHeaderData(Context).Count, Is.EqualTo(3));
        Assert.That(staticBuilder.BuildMessageHeaderData(Context)[0],
            Is.EqualTo("<Header><Name>operation</Name><Value>sayHello</Value></Header>"));
        Assert.That(staticBuilder.BuildMessageHeaderData(Context)[1],
            Is.EqualTo("<Header><Name>operation</Name><Value>foo</Value></Header>"));
        Assert.That(staticBuilder.BuildMessageHeaderData(Context)[2],
            Is.EqualTo("<Header><Name>operation</Name><Value>bar</Value></Header>"));
    }

    [Test]
    public void TestReceiveBuilderWithDictionary()
    {
        var dictionary = new NodeMappingDataDictionary();

        // Reset mocks
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Setup mocks
        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);

        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("TestMessage").SetHeader("operation", "sayHello"));

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());

        Context.SetReferenceResolver(_referenceResolver.Object);
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Type(MessageType.PLAINTEXT)
            .Body("TestMessage")
            .Header("operation", "sayHello")
            .Dictionary(dictionary));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.PLAINTEXT)));
        Assert.That(action.DataDictionary, Is.EqualTo(dictionary));

        Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        var defaultBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        Assert.That(defaultBuilder.BuildMessagePayload(Context, action.MessageType), Is.EqualTo("TestMessage"));
        Assert.That(defaultBuilder.BuildMessageHeaders(Context).ContainsKey("operation"), Is.True);
    }

    [Test]
    public void TestReceiveBuilderWithDictionaryName()
    {
        var dictionary = new NodeMappingDataDictionary();

        // Reset mocks
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Setup mocks
        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);

        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("TestMessage").SetHeader("operation", "sayHello"));

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<IDataDictionary>("customDictionary")).Returns(dictionary);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());

        Context.SetReferenceResolver(_referenceResolver.Object);
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Type(MessageType.PLAINTEXT)
            .Body("TestMessage")
            .Header("operation", "sayHello")
            .Dictionary("customDictionary"));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.PLAINTEXT)));
        Assert.That(action.DataDictionary, Is.EqualTo(dictionary));

        Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        var defaultBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        Assert.That(defaultBuilder.BuildMessagePayload(Context, action.MessageType), Is.EqualTo("TestMessage"));
        Assert.That(defaultBuilder.BuildMessageHeaders(Context).ContainsKey("operation"), Is.True);
    }

    [Test]
    public void TestReceiveBuilderWithSelector()
    {
        var selectiveConsumer = new Mock<ISelectiveConsumer>();

        // Reset mocks
        _messageEndpoint.Reset();
        selectiveConsumer.Reset();
        _configuration.Reset();

        // Setup mocks
        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(selectiveConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);

        selectiveConsumer.Setup(x => x.Receive(
                It.Is<string>(s => s == "operation = 'sayHello'"),
                It.IsAny<TestContext>(),
                It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "sayHello"));

        var messageSelector = new Dictionary<string, object> { { "operation", "sayHello" } };

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());

        Context.SetReferenceResolver(_referenceResolver.Object);
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>")
            .Selector(messageSelector));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.MessageSelectors, Is.EqualTo(messageSelector));
    }

    [Test]
    public void TestReceiveBuilderWithSelectorExpression()
    {
        var selectiveConsumer = new Mock<ISelectiveConsumer>();

        // Reset mocks
        _messageEndpoint.Reset();
        selectiveConsumer.Reset();
        _configuration.Reset();

        // Setup mocks
        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(selectiveConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);

        selectiveConsumer.Setup(x => x.Receive(
                It.Is<string>(s => s == "operation = 'sayHello'"),
                It.IsAny<TestContext>(),
                It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "sayHello"));

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());

        Context.SetReferenceResolver(_referenceResolver.Object);
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message>Hello World!</Message></TestRequest>")
            .Selector("operation = 'sayHello'"));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.MessageSelectors.Count, Is.EqualTo(0));
        Assert.That(action.Selector, Is.EqualTo("operation = 'sayHello'"));
    }

    [Test]
    public void TestReceiveBuilderExtractFromPathExpression()
    {
        var received = new DefaultMessage("<TestRequest><Message lang=\"ENG\">Hello World!</Message></TestRequest>")
            .SetHeader("operation", "sayHello");

        // Reset mocks
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Setup mocks
        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);

        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(received);

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());

        Context.SetReferenceResolver(_referenceResolver.Object);
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message lang=\"ENG\">Hello World!</Message></TestRequest>")
            .Extract(XpathSupport.Xpath().Expression("/TestRequest/Message", "message")));

        Assert.That(Context.GetVariable("message"), Is.Not.Null);
        Assert.That(Context.GetVariable("message"), Is.EqualTo("Hello World!"));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.VariableExtractors.Count, Is.EqualTo(1));
    }

    [Test]
    public void TestReceiveBuilderExtractFromXpathExpression()
    {
        // Reset mocks
        _referenceResolver.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Setup mocks
        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);

        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message lang=\"ENG\">Hello World!</Message></TestRequest>")
                .SetHeader("operation", "sayHello"));

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());

        Context.SetReferenceResolver(_referenceResolver.Object);
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message lang=\"ENG\">Hello World!</Message></TestRequest>")
            .Extract(XpathSupport.Xpath()
                .Expression("/TestRequest/Message", "text")
                .Expression("/TestRequest/Message/@lang", "language")));

        Assert.That(Context.GetVariable("text"), Is.Not.Null);
        Assert.That(Context.GetVariable("language"), Is.Not.Null);
        Assert.That(Context.GetVariable("text"), Is.EqualTo("Hello World!"));
        Assert.That(Context.GetVariable("language"), Is.EqualTo("ENG"));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));

        Assert.That(action.VariableExtractors.Count, Is.EqualTo(1));
        Assert.That(action.VariableExtractors[0], Is.InstanceOf<XpathPayloadVariableExtractor>());

        var xpathExtractor = (XpathPayloadVariableExtractor)action.VariableExtractors[0];
        Assert.That(xpathExtractor.Expressions.ContainsKey("/TestRequest/Message"), Is.True);
        Assert.That(xpathExtractor.Expressions.ContainsKey("/TestRequest/Message/@lang"), Is.True);
    }

    [Test]
    public void TestReceiveBuilderExtractFromHeader()
    {
        // Reset mocks
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Setup mocks
        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);

        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message lang=\"ENG\">Hello World!</Message></TestRequest>")
                .SetHeader("operation", "sayHello")
                .SetHeader("requestId", "123456"));

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());

        Context.SetReferenceResolver(_referenceResolver.Object);
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message lang=\"ENG\">Hello World!</Message></TestRequest>")
            .Extract(MessageSupport.MessageHeaderSupport.FromHeaders()
                .Header("operation", "operationHeader")
                .Header("requestId", "id")));

        Assert.That(Context.GetVariable("operationHeader"), Is.Not.Null);
        Assert.That(Context.GetVariable("id"), Is.Not.Null);
        Assert.That(Context.GetVariable("operationHeader"), Is.EqualTo("sayHello"));
        Assert.That(Context.GetVariable("id"), Is.EqualTo("123456"));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));

        Assert.That(action.VariableExtractors.Count, Is.EqualTo(1));
        Assert.That(action.VariableExtractors[0], Is.InstanceOf<MessageHeaderVariableExtractor>());

        var headerExtractor = (MessageHeaderVariableExtractor)action.VariableExtractors[0];
        Assert.That(headerExtractor.GetHeaderMappings().ContainsKey("operation"), Is.True);
        Assert.That(headerExtractor.GetHeaderMappings().ContainsKey("requestId"), Is.True);
    }

    [Test]
    public void TestReceiveBuilderExtractCombined()
    {
        // Reset mocks
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Setup mocks
        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);

        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message lang=\"ENG\">Hello World!</Message></TestRequest>")
                .SetHeader("operation", "sayHello")
                .SetHeader("requestId", "123456"));

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());

        Context.SetReferenceResolver(_referenceResolver.Object);
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message lang=\"ENG\">Hello World!</Message></TestRequest>")
            .Extract(MessageSupport.MessageHeaderSupport.FromHeaders()
                .Header("operation", "operationHeader")
                .Header("requestId", "id"))
            .Extract(XpathSupport.Xpath()
                .Expression("/TestRequest/Message", "text")
                .Expression("/TestRequest/Message/@lang", "language")));

        // Verify header extractions
        Assert.That(Context.GetVariable("operationHeader"), Is.Not.Null);
        Assert.That(Context.GetVariable("id"), Is.Not.Null);
        Assert.That(Context.GetVariable("operationHeader"), Is.EqualTo("sayHello"));
        Assert.That(Context.GetVariable("id"), Is.EqualTo("123456"));

        // Verify XPath extractions
        Assert.That(Context.GetVariable("text"), Is.Not.Null);
        Assert.That(Context.GetVariable("language"), Is.Not.Null);
        Assert.That(Context.GetVariable("text"), Is.EqualTo("Hello World!"));
        Assert.That(Context.GetVariable("language"), Is.EqualTo("ENG"));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));

        Assert.That(action.VariableExtractors.Count, Is.EqualTo(2));

        // Verify first extractor (MessageHeaderVariableExtractor)
        Assert.That(action.VariableExtractors[0], Is.InstanceOf<MessageHeaderVariableExtractor>());
        var headerExtractor = (MessageHeaderVariableExtractor)action.VariableExtractors[0];
        Assert.That(headerExtractor.GetHeaderMappings().ContainsKey("operation"), Is.True);
        Assert.That(headerExtractor.GetHeaderMappings().ContainsKey("requestId"), Is.True);

        // Verify second extractor (XpathPayloadVariableExtractor)
        Assert.That(action.VariableExtractors[1], Is.InstanceOf<XpathPayloadVariableExtractor>());
        var xpathExtractor = (XpathPayloadVariableExtractor)action.VariableExtractors[1];
        Assert.That(xpathExtractor.Expressions.ContainsKey("/TestRequest/Message"), Is.True);
        Assert.That(xpathExtractor.Expressions.ContainsKey("/TestRequest/Message/@lang"), Is.True);
    }

    [Test]
    public void TestReceiveBuilderWithValidationProcessor()
    {
        var callback = new Mock<AbstractValidationProcessor<dynamic>>();

        // Reset mocks
        callback.Reset();
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Setup mocks
        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);

        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("TestMessage").SetHeader("operation", "sayHello"));

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());

        Context.SetReferenceResolver(_referenceResolver.Object);
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Type(MessageType.PLAINTEXT)
            .Body("TestMessage")
            .Header("operation", "sayHello")
            .Validate(callback.Object));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.PLAINTEXT)));
        Assert.That(action.Processor, Is.EqualTo(callback.Object));

        Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        var defaultMessageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        Assert.That(defaultMessageBuilder.BuildMessagePayload(Context, action.MessageType), Is.EqualTo("TestMessage"));
        Assert.That(defaultMessageBuilder.BuildMessageHeaders(Context).ContainsKey("operation"), Is.True);

        callback.Verify(x => x.SetReferenceResolver(Context.ReferenceResolver), Times.AtLeastOnce);
        callback.Verify(x => x.Validate(It.IsAny<IMessage>(), It.IsAny<TestContext>()), Times.Once);
    }

    [Test]
    public void TestReceiveBuilderWithNamespaceValidation()
    {
        // Reset mocks
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Setup mocks
        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);

        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage(
                    "<TestRequest xmlns:pfx=\"http://agenix.org/schemas/test\"><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "foo"));

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());

        Context.SetReferenceResolver(_referenceResolver.Object);
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body(
                "<TestRequest xmlns:pfx=\"http://agenix.org/schemas/test\"><Message>Hello World!</Message></TestRequest>")
            .Validate(XmlSupport.Xml()
                .Namespace("pfx", "http://agenix.org/schemas/test")));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.ValidationContexts.Count, Is.EqualTo(2));
        Assert.That(action.ValidationContexts.Any(x => x is HeaderValidationContext), Is.True);
        Assert.That(action.ValidationContexts.Any(x => x is XmlMessageValidationContext), Is.True);

        var validationContext = action.ValidationContexts
            .OfType<XmlMessageValidationContext>()
            .FirstOrDefault();

        Assert.That(validationContext, Is.Not.Null, "Missing validation context");

        Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        var defaultMessageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        Assert.That(defaultMessageBuilder.BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo(
                "<TestRequest xmlns:pfx=\"http://agenix.org/schemas/test\"><Message>Hello World!</Message></TestRequest>"));
        Assert.That(validationContext.ControlNamespaces["pfx"], Is.EqualTo("http://agenix.org/schemas/test"));
    }

    [Test]
    public void TestReceiveBuilderWithXpathExpressions()
    {
        // Reset mocks
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Setup mocks
        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);

        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage(
                    "<TestRequest><Message lang=\"ENG\">Hello World!</Message><Operation>SayHello</Operation></TestRequest>")
                .SetHeader("operation", "sayHello"));

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());

        Context.SetReferenceResolver(_referenceResolver.Object);
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body(
                "<TestRequest><Message lang=\"ENG\">Hello World!</Message><Operation>SayHello</Operation></TestRequest>")
            .Validate(XpathSupport.Xpath()
                .Expression("TestRequest.Message", "Hello World!")
                .Expression("TestRequest.Operation", "SayHello")));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.ValidationContexts.Count, Is.EqualTo(2));
        Assert.That(action.ValidationContexts.Any(x => x is HeaderValidationContext), Is.True);
        Assert.That(action.ValidationContexts.Any(x => x is XpathMessageValidationContext), Is.True);

        var validationContext = action.ValidationContexts
            .OfType<XpathMessageValidationContext>()
            .FirstOrDefault();

        Assert.That(validationContext, Is.Not.Null, "Missing validation context");

        Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        Assert.That(validationContext.XpathExpressions.Count, Is.EqualTo(2));
        Assert.That(validationContext.XpathExpressions["TestRequest.Message"], Is.EqualTo("Hello World!"));
        Assert.That(validationContext.XpathExpressions["TestRequest.Operation"], Is.EqualTo("SayHello"));
    }

    [Test]
    public void TestReceiveBuilderWithIgnoreElementsXpath()
    {
        // Reset mocks
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Setup mocks
        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);

        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
                .SetHeader("operation", "sayHello"));

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());

        Context.SetReferenceResolver(_referenceResolver.Object);
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message>?</Message></TestRequest>")
            .Validate(XmlSupport.Xml()
                .Ignore("TestRequest.Message")));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.MessageType, Is.EqualTo(nameof(MessageType.XML)));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.ValidationContexts.Count, Is.EqualTo(2));
        Assert.That(action.ValidationContexts.Any(x => x is HeaderValidationContext), Is.True);
        Assert.That(action.ValidationContexts.Any(x => x is XmlMessageValidationContext), Is.True);

        var validationContext = action.ValidationContexts
            .OfType<XmlMessageValidationContext>()
            .FirstOrDefault();

        Assert.That(validationContext, Is.Not.Null, "Missing validation context");

        Assert.That(action.MessageBuilder, Is.InstanceOf<DefaultMessageBuilder>());
        var defaultMessageBuilder = (DefaultMessageBuilder)action.MessageBuilder;
        Assert.That(defaultMessageBuilder.BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo("<TestRequest><Message>?</Message></TestRequest>"));
        Assert.That(validationContext.IgnoreExpressions.Count, Is.EqualTo(1));
        Assert.That(validationContext.IgnoreExpressions.First(), Is.EqualTo("TestRequest.Message"));
    }

    [Test]
    public void TestDeactivateSchemaValidation()
    {
        // Reset mocks
        _messageEndpoint.Reset();
        _messageConsumer.Reset();
        _configuration.Reset();

        // Setup mocks
        _messageEndpoint.Setup(x => x.CreateConsumer()).Returns(_messageConsumer.Object);
        _messageEndpoint.Setup(x => x.EndpointConfiguration).Returns(_configuration.Object);
        _configuration.Setup(x => x.Timeout).Returns(100L);
        _messageConsumer.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(new DefaultMessage("<TestRequest><Message>Hello Agenix!</Message></TestRequest>")
                .SetHeader("operation", "sayHello"));

        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new Dictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new Dictionary<string, SequenceAfterTest>());

        Context.SetReferenceResolver(_referenceResolver.Object);
        var runner = new DefaultTestCaseRunner(Context);
        runner.Run(ReceiveMessageAction.Builder.Receive(_messageEndpoint.Object)
            .Message()
            .Body("<TestRequest><Message>Hello Agenix!</Message></TestRequest>")
            .Validate(XmlSupport.Xml()
                .SchemaValidation(false)));

        var test = runner.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("receive"));
        Assert.That(action.Endpoint, Is.EqualTo(_messageEndpoint.Object));
        Assert.That(action.ValidationContexts.Count, Is.EqualTo(2));
        Assert.That(action.ValidationContexts.Any(x => x is HeaderValidationContext), Is.True);
        Assert.That(action.ValidationContexts.Any(x => x is XmlMessageValidationContext), Is.True);

        var xmlMessageValidationContext = action.ValidationContexts
            .OfType<XmlMessageValidationContext>()
            .FirstOrDefault();

        Assert.That(xmlMessageValidationContext, Is.Not.Null, "Missing validation context");
        Assert.That(xmlMessageValidationContext.IsSchemaValidationEnabled, Is.False);
    }
}
