using System;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Api.Spi;
using Agenix.Core.Endpoint.Direct;
using Agenix.Core.Message;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.Tests.Endpoint.Direct;

public class DirectEndpointProducerTest
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
    public void TestSendMessage()
    {
        var endpoint = new DirectEndpoint();

        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);

        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        _queueMock.Reset();

        endpoint.CreateProducer().Send(message, _context);

        _queueMock.Verify(q => q.Send(It.IsAny<IMessage>()), Times.Once);
    }

    [Test]
    public void TestSendMessageQueueNameResolver()
    {
        var endpoint = new DirectEndpoint();

        endpoint.EndpointConfiguration.SetQueueName("testQueue");

        _context.SetReferenceResolver(_resolverMock.Object);

        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        _queueMock.Reset();
        _resolverMock.Reset();

        _resolverMock.Setup(r => r.Resolve<IMessageQueue>("testQueue")).Returns(_queueMock.Object);

        endpoint.CreateProducer().Send(message, _context);

        _queueMock.Verify(q => q.Send(It.IsAny<IMessage>()), Times.Once);
    }

    [Test]
    public void TestSendMessageFailed()
    {
        var endpoint = new DirectEndpoint();

        endpoint.EndpointConfiguration.SetQueue(_queueMock.Object);

        var message = new DefaultMessage("<TestRequest><Message>Hello World!</Message></TestRequest>");

        _queueMock.Reset();

        _queueMock.Setup(q => q.Send(It.IsAny<IMessage>())).Throws(new SystemException("Internal error!"));

        try
        {
            endpoint.CreateProducer().Send(message, _context);
        }
        catch (AgenixSystemException e)
        {
            ClassicAssert.IsTrue(e.Message.StartsWith("Failed to send message to queue: "));
            ClassicAssert.IsNotNull(e.InnerException);
            ClassicAssert.AreEqual(e.InnerException.GetType(), typeof(SystemException));
            ClassicAssert.AreEqual(e.InnerException.Message, "Internal error!");
            return;
        }

        Assert.Fail("Missing " + nameof(AgenixSystemException) + " because no message was received");
    }
}
