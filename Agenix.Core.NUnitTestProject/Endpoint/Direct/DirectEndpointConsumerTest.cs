using System.Collections.Generic;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Api.Spi;
using Agenix.Core.Endpoint.Direct;
using Agenix.Core.Message;
using TestContext = Agenix.Api.Context.TestContext;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Endpoint.Direct;

public class DirectEndpointConsumerTest
{
    private TestContext _context;

    private Mock<IMessageQueue> _queueMock;
    private Mock<IReferenceResolver> _resolverMock;

    [SetUp]
    public void SetupMocks()
    {
        _queueMock = new Mock<IMessageQueue>();
        _resolverMock = new Mock<IReferenceResolver>();
        _context = new TestContext();
    }

    [Test]
    public void TestReceiveMessage()
    {
        var endpoint = new DirectEndpoint();
        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);

        var headers = new Dictionary<string, object>();
        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>", headers);

        _queueMock.Reset();

        _queueMock.Setup(q => q.Receive(5000L)).Returns(message);

        var receivedMessage = endpoint.CreateConsumer().Receive(_context);

        ClassicAssert.AreEqual(receivedMessage.Payload, message.Payload);
        ClassicAssert.AreEqual(receivedMessage.GetHeader(MessageHeaders.Id), message.Id);
    }

    [Test]
    public void TestReceiveMessageQueueNameResolver()
    {
        var endpoint = new DirectEndpoint();
        endpoint.EndpointConfiguration.SetQueueName("testQueue");

        _context.SetReferenceResolver(_resolverMock.Object);

        var headers = new Dictionary<string, object>();
        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>", headers);

        _queueMock.Reset();
        _resolverMock.Reset();

        _resolverMock.Setup(r => r.Resolve<IMessageQueue>("testQueue")).Returns(_queueMock.Object);
        _queueMock.Setup(q => q.Receive(5000L)).Returns(message);

        var receivedMessage = endpoint.CreateConsumer().Receive(_context);

        ClassicAssert.AreEqual(receivedMessage.Payload, message.Payload);
        ClassicAssert.AreEqual(receivedMessage.GetHeader(MessageHeaders.Id), message.Id);
    }

    [Test]
    public void TestReceiveMessageWithCustomTimeout()
    {
        var endpoint = new DirectEndpoint();

        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);
        endpoint.EndpointConfiguration.Timeout = 10000L;

        var headers = new Dictionary<string, object>();
        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>", headers);

        _queueMock.Reset();

        _queueMock.Setup(q => q.Receive(10000L)).Returns(message);

        var receivedMessage = endpoint.CreateConsumer().Receive(_context);

        ClassicAssert.AreEqual(receivedMessage.Payload, message.Payload);
        ClassicAssert.AreEqual(receivedMessage.GetHeader(MessageHeaders.Id), message.Id);
    }

    [Test]
    public void TestReceiveMessageTimeoutOverride()
    {
        var endpoint = new DirectEndpoint();

        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);
        endpoint.EndpointConfiguration.Timeout = 10000L;

        var headers = new Dictionary<string, object>();
        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>", headers);

        _queueMock.Reset();

        _queueMock.Setup(q => q.Receive(25000L)).Returns(message);

        var receivedMessage = endpoint.CreateConsumer().Receive(_context, 25000L);

        ClassicAssert.AreEqual(receivedMessage.Payload, message.Payload);
        ClassicAssert.AreEqual(receivedMessage.GetHeader(MessageHeaders.Id), message.Id);
    }

    [Test]
    public void TestReceiveTimeout()
    {
        var endpoint = new DirectEndpoint();

        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);

        _queueMock.Reset();

        _queueMock.Setup(q => q.Receive(5000L)).Returns((IMessage)null);

        try
        {
            endpoint.CreateConsumer().Receive(_context);
            Assert.Fail("Missing ActionTimeoutException because no message was received");
        }
        catch (ActionTimeoutException e)
        {
            ClassicAssert.IsTrue(
                e.Message.StartsWith("Action timeout after 5000 milliseconds. Failed to receive message on endpoint"));
        }
    }

    [Test]
    public void TestReceiveSelected()
    {
        var endpoint = new DirectEndpoint();

        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);
        endpoint.EndpointConfiguration.Timeout = 0L;

        try
        {
            endpoint.CreateConsumer().Receive("Operation = 'sayHello'", _context);
            Assert.Fail("Missing exception due to unsupported operation");
        }
        catch (AgenixSystemException e)
        {
            ClassicAssert.IsNotNull(e.Message);
        }

        var queueQueueMock = new Mock<IMessageQueue>();
        var message = new DefaultMessage("Hello").SetHeader("Operation", "sayHello");

        queueQueueMock
            .Setup(q => q.Receive(It.IsAny<MessageSelector>()))
            .Returns(message);

        endpoint.EndpointConfiguration.SetQueue(queueQueueMock.Object);
        var receivedMessage = endpoint.CreateConsumer().Receive("Operation = 'sayHello'", _context);

        ClassicAssert.AreEqual(receivedMessage.Payload, message.Payload);
        ClassicAssert.AreEqual(receivedMessage.GetHeader(MessageHeaders.Id), message.Id);
        ClassicAssert.AreEqual(receivedMessage.GetHeader("Operation"), "sayHello");
    }

    [Test]
    public void TestReceiveSelectedNoMessageWithTimeout()
    {
        var endpoint = new DirectEndpoint();

        _queueMock.Reset();
        _queueMock.Setup(q => q.Receive(It.IsAny<MessageSelector>(), 1500L))
            .Returns((IMessage)null); // force retry

        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);

        try
        {
            endpoint.CreateConsumer().Receive("Operation = 'sayHello'", _context, 1500L);
            Assert.Fail("Missing ActionTimeoutException because no message was received");
        }
        catch (ActionTimeoutException e)
        {
            ClassicAssert.IsTrue(
                e.Message.StartsWith("Action timeout after 1500 milliseconds. Failed to receive message on endpoint"));
        }
    }
}