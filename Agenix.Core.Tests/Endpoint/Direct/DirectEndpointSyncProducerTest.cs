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

namespace Agenix.Core.Tests.Endpoint.Direct;

public class DirectEndpointSyncProducerTest
{
    private TestContext _context;
    private Mock<IMessageCorrelator> _messageCorrelatorMock;
    private Mock<IMessageQueue> _queueMock;
    private Mock<IReferenceResolver> _resolverMock;

    [SetUp]
    public void SetupMocks()
    {
        _queueMock = new Mock<IMessageQueue>();
        _messageCorrelatorMock = new Mock<IMessageCorrelator>();
        _resolverMock = new Mock<IReferenceResolver>();
        _context = new TestContext();
    }

    [Test]
    public void TestSendMessage()
    {
        var endpoint = new DirectSyncEndpoint();
        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);

        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        var responseHeaders = new Dictionary<string, object>();
        var responseMessage = new DefaultMessage("<TestResponse>Hello World!</TestResponse>", responseHeaders);

        Mock.Get(_queueMock.Object).Reset();
        _queueMock.Setup(q => q.Send(It.IsAny<IMessage>())).Callback<IMessage>(msg =>
        {
            ClassicAssert.IsNotNull(msg.GetHeader(DirectMessageHeaders.ReplyQueue));
            var replyQueue = msg.GetHeader(DirectMessageHeaders.ReplyQueue) as IMessageQueue;
            replyQueue?.Send(responseMessage);
        });

        var producer = endpoint.CreateProducer();
        producer.Send(message, _context);
    }

    [Test]
    public void TestSendMessageCustomReplyQueue()
    {
        var endpoint = new DirectSyncEndpoint();
        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);

        var replyQueue = new DefaultMessageQueue("testQueue");
        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>")
            .SetHeader(DirectMessageHeaders.ReplyQueue, replyQueue);

        var responseHeaders = new Dictionary<string, object>();
        var responseMessage = new DefaultMessage("<TestResponse>Hello World!</TestResponse>", responseHeaders);

        Mock.Get(_queueMock.Object).Reset();
        _queueMock.Setup(q => q.Send(It.IsAny<IMessage>())).Callback<IMessage>(msg =>
        {
            ClassicAssert.IsNotNull(msg.GetHeader(DirectMessageHeaders.ReplyQueue));
            ClassicAssert.AreEqual(msg.GetHeader(DirectMessageHeaders.ReplyQueue), replyQueue);
            replyQueue.Send(responseMessage);
        });

        var producer = endpoint.CreateProducer();
        producer.Send(message, _context);
    }

    [Test]
    public void TestSendMessageQueueNameResolver()
    {
        var endpoint = new DirectSyncEndpoint();
        endpoint.EndpointConfiguration.SetQueueName("testQueue");

        _context.SetReferenceResolver(_resolverMock.Object);

        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        var responseHeaders = new Dictionary<string, object>();
        var responseMessage = new DefaultMessage("<TestResponse>Hello World!</TestResponse>", responseHeaders);

        Mock.Get(_queueMock.Object).Reset();
        Mock.Get(_resolverMock.Object).Reset();

        _resolverMock.Setup(r => r.Resolve<IMessageQueue>("testQueue")).Returns(_queueMock.Object);
        _queueMock.Setup(q => q.Send(It.IsAny<IMessage>())).Callback<IMessage>(msg =>
        {
            ClassicAssert.IsNotNull(msg.GetHeader(DirectMessageHeaders.ReplyQueue));
            var replyQueue = msg.GetHeader(DirectMessageHeaders.ReplyQueue) as IMessageQueue;
            replyQueue?.Send(responseMessage);
        });

        var producer = endpoint.CreateProducer();
        producer.Send(message, _context);
    }

    [Test]
    public void TestSendMessageWithReplyHandler()
    {
        var endpoint = new DirectSyncEndpoint();
        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);

        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        var responseHeaders = new Dictionary<string, object>();
        var responseMessage = new DefaultMessage("<TestResponse>Hello World!</TestResponse>", responseHeaders);

        Mock.Get(_queueMock.Object).Reset();
        _queueMock.Setup(q => q.Send(It.IsAny<IMessage>())).Callback<IMessage>(msg =>
        {
            ClassicAssert.IsNotNull(msg.GetHeader(DirectMessageHeaders.ReplyQueue));
            var replyQueue = msg.GetHeader(DirectMessageHeaders.ReplyQueue) as IMessageQueue;
            replyQueue?.Send(responseMessage);
        });

        var producer = (DirectSyncProducer)endpoint.CreateProducer();
        producer.Send(message, _context);

        var correlationKey = endpoint.EndpointConfiguration.Correlator.GetCorrelationKey(message);
        var replyMessage = producer.CorrelationManager.Find(correlationKey, endpoint.EndpointConfiguration.Timeout);

        ClassicAssert.AreEqual(replyMessage.Payload, responseMessage.Payload);
        ClassicAssert.AreEqual(replyMessage.GetHeader(MessageHeaders.Id), responseMessage.Id);
    }

    [Test]
    public void TestSendMessageWithCustomReplyTimeout()
    {
        var endpoint = new DirectSyncEndpoint();
        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);

        endpoint.EndpointConfiguration.Timeout = 10000L;

        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        var responseHeaders = new Dictionary<string, object>();
        var responseMessage = new DefaultMessage("<TestResponse>Hello World!</TestResponse>", responseHeaders);

        Mock.Get(_queueMock.Object).Reset();
        _queueMock.Setup(q => q.Send(It.IsAny<IMessage>())).Callback<IMessage>(msg =>
        {
            ClassicAssert.IsNotNull(msg.GetHeader(DirectMessageHeaders.ReplyQueue));
            var replyQueue = msg.GetHeader(DirectMessageHeaders.ReplyQueue) as IMessageQueue;
            replyQueue?.Send(responseMessage);
        });

        var producer = (DirectSyncProducer)endpoint.CreateProducer();
        producer.Send(message, _context);

        var correlationKey = endpoint.EndpointConfiguration.Correlator.GetCorrelationKey(message);
        var replyMessage = producer.CorrelationManager.Find(correlationKey, endpoint.EndpointConfiguration.Timeout);

        ClassicAssert.AreEqual(replyMessage.Payload, responseMessage.Payload);
        ClassicAssert.AreEqual(replyMessage.GetHeader(MessageHeaders.Id), responseMessage.Id);
    }

    [Test]
    public void TestSendMessageWithReplyMessageCorrelator()
    {
        var endpoint = new DirectSyncEndpoint();
        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);
        endpoint.EndpointConfiguration.Correlator = _messageCorrelatorMock.Object;

        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        var responseHeaders = new Dictionary<string, object>();
        var responseMessage = new DefaultMessage("<TestResponse>Hello World!</TestResponse>", responseHeaders);

        Mock.Get(_queueMock.Object).Reset();
        Mock.Get(_messageCorrelatorMock.Object).Reset();

        _queueMock.Setup(q => q.Send(It.IsAny<IMessage>())).Callback<IMessage>(msg =>
        {
            ClassicAssert.IsNotNull(msg.GetHeader(DirectMessageHeaders.ReplyQueue));
            var replyQueue = msg.GetHeader(DirectMessageHeaders.ReplyQueue) as IMessageQueue;
            replyQueue?.Send(responseMessage);
        });

        _messageCorrelatorMock.Setup(c => c.GetCorrelationKey(message)).Returns(MessageHeaders.Id + " = '123456789'");
        _messageCorrelatorMock.Setup(c => c.GetCorrelationKeyName(It.IsAny<string>())).Returns("correlationKeyName");

        var producer = (DirectSyncProducer)endpoint.CreateProducer();
        producer.Send(message, _context);

        var replyMessage = producer.CorrelationManager.Find(MessageHeaders.Id + " = '123456789'",
            endpoint.EndpointConfiguration.Timeout);

        ClassicAssert.AreEqual(replyMessage.Payload, responseMessage.Payload);
        ClassicAssert.AreEqual(replyMessage.GetHeader(MessageHeaders.Id), responseMessage.Id);
    }

    [Test]
    public void TestSendMessageNoResponse()
    {
        var endpoint = new DirectSyncEndpoint();
        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);

        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        Mock.Get(_queueMock.Object).Reset();
        _queueMock.Setup(q => q.ToString()).Returns("mockQueue");
        _queueMock.Setup(q => q.Send(It.IsAny<IMessage>())).Callback<IMessage>(msg =>
        {
            ClassicAssert.IsNotNull(msg.GetHeader(DirectMessageHeaders.ReplyQueue));
        });

        try
        {
            endpoint.CreateProducer().Send(message, _context);
        }
        catch (AgenixSystemException e)
        {
            ClassicAssert.AreEqual(e.Message,
                "Failed to receive synchronous reply message on endpoint: 'mockQueue'");
            return;
        }

        Assert.Fail("Missing CoreSystemException because of reply timeout");
    }

    [Test]
    public void TestOnReplyMessage()
    {
        var endpoint = new DirectSyncEndpoint();
        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        var producer = (DirectSyncProducer)endpoint.CreateProducer();
        var correlationKeyName = endpoint.EndpointConfiguration.Correlator.GetCorrelationKeyName(producer.Name);

        producer.CorrelationManager.SaveCorrelationKey(correlationKeyName, producer.ToString(), _context);
        producer.CorrelationManager.Store(producer.ToString(), message);

        var receivedMessage = producer.Receive(_context);

        ClassicAssert.AreEqual(receivedMessage.Payload, message.Payload);
    }

    [Test]
    public void TestOnReplyMessageWithCorrelatorKey()
    {
        var endpoint = new DirectSyncEndpoint();

        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        var producer = (DirectSyncProducer)endpoint.CreateProducer();
        var messageCorrelator = new DefaultMessageCorrelator();
        var correlationKey = messageCorrelator.GetCorrelationKey(message);

        producer.CorrelationManager.Store(correlationKey, message);

        var receivedMessage = producer.Receive(correlationKey, _context);

        ClassicAssert.AreEqual(receivedMessage.Payload, message.Payload);
    }
}
