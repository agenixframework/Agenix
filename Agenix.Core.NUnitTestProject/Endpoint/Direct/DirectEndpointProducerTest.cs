using System;
using Agenix.Core.Endpoint.Direct;
using Agenix.Core.Exceptions;
using Agenix.Core.Message;
using Agenix.Core.Spi;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Endpoint.Direct;

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
        catch (CoreSystemException e)
        {
            ClassicAssert.IsTrue(e.Message.StartsWith("Failed to send message to queue: "));
            ClassicAssert.IsNotNull(e.InnerException);
            ClassicAssert.AreEqual(e.InnerException.GetType(), typeof(SystemException));
            ClassicAssert.AreEqual(e.InnerException.Message, "Internal error!");
            return;
        }

        Assert.Fail("Missing " + nameof(CoreSystemException) + " because no message was received");
    }
}