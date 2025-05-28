using Agenix.Api.Annotations;
using Agenix.Api.Endpoint;
using Agenix.Api.Endpoint.Resolver;
using Agenix.Api.Message;
using Agenix.Api.Spi;
using Agenix.Core.Endpoint.Direct;
using Agenix.Core.Endpoint.Direct.Annotation;
using Agenix.Core.Message;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.NUnitTestProject.Endpoint.Direct.Annotation;

public class DirectSyncEndpointConfigParserTest
{
    private TestContext _context;
    [AgenixEndpoint] [DirectSyncEndpointConfig(QueueName = "testQueue")]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private DirectSyncEndpoint _directSyncEndpoint1;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [AgenixEndpoint]
    [DirectSyncEndpointConfig(Timeout = 10000L,
        Queue = "myQueue",
        Correlator = "replyMessageCorrelator")]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private DirectSyncEndpoint _directSyncEndpoint2;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [AgenixEndpoint] [DirectSyncEndpointConfig(Correlator = "replyMessageCorrelator")]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private DirectSyncEndpoint _directSyncEndpoint3;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [AgenixEndpoint] [DirectSyncEndpointConfig(QueueName = "testQueue")]
#pragma warning disable CS0169 // Field is never used
    private DirectSyncEndpoint _directSyncEndpoint4;
#pragma warning restore CS0169 // Field is never used

    [AgenixEndpoint] [DirectSyncEndpointConfig(QueueName = "testQueue")]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private DirectSyncEndpoint _directSyncEndpoint5;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [AgenixEndpoint]
    [DirectSyncEndpointConfig(Timeout = 10000L,
        Queue = "myQueue",
        Correlator = "replyMessageCorrelator")]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private DirectSyncEndpoint _directSyncEndpoint6;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [AgenixEndpoint] [DirectSyncEndpointConfig(Correlator = "replyMessageCorrelator")]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private DirectSyncEndpoint _directSyncEndpoint7;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [AgenixEndpoint]
    [DirectSyncEndpointConfig(QueueName = "testQueue",
        PollingInterval = 250)]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private DirectSyncEndpoint _directSyncEndpoint8;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
    private Mock<IEndpointUriResolver> _endpointNameResolver;
    private Mock<IMessageCorrelator> _messageCorrelator;
    private Mock<IMessageQueue> _myQueue;

    private Mock<IReferenceResolver> _referenceResolver;

    [OneTimeSetUp]
    public void Setup()
    {
        _referenceResolver = new Mock<IReferenceResolver>();
        _myQueue = new Mock<IMessageQueue>();
        _endpointNameResolver = new Mock<IEndpointUriResolver>();
        _messageCorrelator = new Mock<IMessageCorrelator>();

        _context = new TestContext();

        _referenceResolver
            .Setup(r => r.Resolve<IMessageQueue>("myQueue"))
            .Returns(_myQueue.Object);
        _referenceResolver
            .Setup(r => r.Resolve<IEndpointUriResolver>("endpointNameResolver"))
            .Returns(_endpointNameResolver.Object);
        _referenceResolver
            .Setup(r => r.Resolve<IMessageCorrelator>("replyMessageCorrelator"))
            .Returns(_messageCorrelator.Object);
    }

    [SetUp]
    public void SetMocks()
    {
        _context.EndpointFactory = new DefaultEndpointFactory();
        _context.SetReferenceResolver(_referenceResolver.Object);
    }

    [Test]
    public void TestDirectSyncEndpointAsConsumerParser()
    {
        AgenixEndpointAnnotations.InjectEndpoints(this, _context);

        // 1st message receiver
        ClassicAssert.AreEqual("testQueue", _directSyncEndpoint1.EndpointConfiguration.GetQueueName());
        ClassicAssert.IsNull(_directSyncEndpoint1.EndpointConfiguration.GetQueue());
        ClassicAssert.AreEqual(5000L, _directSyncEndpoint1.EndpointConfiguration.Timeout);
        ClassicAssert.AreEqual(typeof(DefaultMessageCorrelator),
            _directSyncEndpoint1.EndpointConfiguration.Correlator.GetType());

        // 2nd message receiver
        ClassicAssert.IsNull(_directSyncEndpoint2.EndpointConfiguration.GetQueueName());
        ClassicAssert.IsNotNull(_directSyncEndpoint2.EndpointConfiguration.GetQueue());
        ClassicAssert.AreEqual(10000L, _directSyncEndpoint2.EndpointConfiguration.Timeout);
        ClassicAssert.AreEqual(_messageCorrelator.Object, _directSyncEndpoint2.EndpointConfiguration.Correlator);

        // 3rd message receiver
        ClassicAssert.IsNull(_directSyncEndpoint3.EndpointConfiguration.GetQueueName());
        ClassicAssert.IsNull(_directSyncEndpoint3.EndpointConfiguration.GetQueue());
        ClassicAssert.AreEqual(_messageCorrelator.Object, _directSyncEndpoint3.EndpointConfiguration.Correlator);

        // 4th message receiver
        ClassicAssert.AreEqual("testQueue", _directSyncEndpoint5.EndpointConfiguration.GetQueueName());
        ClassicAssert.IsNull(_directSyncEndpoint5.EndpointConfiguration.GetQueue());
        ClassicAssert.AreEqual(5000L, _directSyncEndpoint5.EndpointConfiguration.Timeout);
        ClassicAssert.AreEqual(500L, _directSyncEndpoint5.EndpointConfiguration.PollingInterval);
        ClassicAssert.AreEqual(typeof(DefaultMessageCorrelator),
            _directSyncEndpoint5.EndpointConfiguration.Correlator.GetType());

        // 5th message sender
        ClassicAssert.IsNull(_directSyncEndpoint6.EndpointConfiguration.GetQueueName());
        ClassicAssert.IsNotNull(_directSyncEndpoint6.EndpointConfiguration.GetQueue());
        ClassicAssert.AreEqual(10000L, _directSyncEndpoint6.EndpointConfiguration.Timeout);
        ClassicAssert.AreEqual(_messageCorrelator.Object, _directSyncEndpoint6.EndpointConfiguration.Correlator);

        // 6th message sender
        ClassicAssert.IsNull(_directSyncEndpoint7.EndpointConfiguration.GetQueueName());
        ClassicAssert.IsNull(_directSyncEndpoint7.EndpointConfiguration.GetQueue());
        ClassicAssert.AreEqual(_messageCorrelator.Object, _directSyncEndpoint7.EndpointConfiguration.Correlator);

        // 7th message sender
        ClassicAssert.IsNotNull(_directSyncEndpoint8.EndpointConfiguration.PollingInterval);
        ClassicAssert.AreEqual(250L, _directSyncEndpoint8.EndpointConfiguration.PollingInterval);
    }
}