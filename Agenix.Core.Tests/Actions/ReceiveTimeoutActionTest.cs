using Agenix.Api.Endpoint;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Core.Actions;
using Agenix.Core.Endpoint;
using Agenix.Core.Message;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Actions;

/// <summary>
///     Unit tests for the <c>ReceiveTimeoutAction</c> class, using NUnit Framework.
/// </summary>
public class ReceiveTimeoutActionTest : AbstractNUnitSetUp
{
    private Mock<ISelectiveConsumer> _consumer;
    private Mock<IEndpoint> _endpoint;
    private Mock<IEndpointConfiguration> _endpointConfiguration;

    [SetUp]
    public void SetUp()
    {
        _endpoint = new Mock<IEndpoint>();
        _consumer = new Mock<ISelectiveConsumer>();
        _endpointConfiguration = new Mock<IEndpointConfiguration>();
    }

    [Test]
    public void TestReceiveTimeout()
    {
        // Arrange
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        _consumer.Setup(c => c.Receive(Context, 1000L)).Returns((IMessage)null);

        var receiveTimeout = new ReceiveTimeoutAction.Builder()
            .Endpoint(_endpoint.Object)
            .Build();

        // Act
        receiveTimeout.Execute(Context);
    }

    [Test]
    public void TestReceiveTimeoutCustomTimeout()
    {
        // Arrange
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        _consumer.Setup(c => c.Receive(Context, 500L)).Returns((IMessage)null);

        var receiveTimeout = new ReceiveTimeoutAction.Builder()
            .Endpoint(_endpoint.Object)
            .Timeout(500L)
            .Build();

        // Act
        receiveTimeout.Execute(Context);
    }

    [Test]
    public void TestReceiveTimeoutFail()
    {
        // Arrange
        var message = new DefaultMessage("<TestMessage>Hello World!</TestMessage>");
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        _consumer.Setup(c => c.Receive(Context, 1000L)).Returns(message);

        var receiveTimeout = new ReceiveTimeoutAction.Builder()
            .Endpoint(_endpoint.Object)
            .Build();

        // Act & Assert
        var ex = Assert.Throws<AgenixSystemException>(() => receiveTimeout.Execute(Context));
        Assert.That(ex, Is.Not.Null);
        ClassicAssert.AreEqual(
            "Message timeout validation failed! Received message while waiting for timeout on destination", ex.Message);
    }

    [Test]
    public void TestReceiveTimeoutWithMessageSelector()
    {
        // Arrange
        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        _consumer.Setup(c => c.Receive("Operation = 'sayHello'", Context, 1000L)).Returns((IMessage)null);

        var receiveTimeout = new ReceiveTimeoutAction.Builder()
            .Endpoint(_endpoint.Object)
            .Selector("Operation = 'sayHello'")
            .Build();

        // Act
        receiveTimeout.Execute(Context);
    }

    [Test]
    public void TestReceiveTimeoutWithMessageSelectorVariableSupport()
    {
        // Arrange
        Context.SetVariable("operation", "sayHello");

        _endpoint.Setup(e => e.CreateConsumer()).Returns(_consumer.Object);
        _endpoint.Setup(e => e.EndpointConfiguration).Returns(_endpointConfiguration.Object);
        _endpointConfiguration.Setup(ec => ec.Timeout).Returns(5000L);

        _consumer.Setup(c => c.Receive("Operation = 'sayHello'", Context, 1000L)).Returns((IMessage)null);

        var receiveTimeout = new ReceiveTimeoutAction.Builder()
            .Endpoint(_endpoint.Object)
            .Selector("Operation = '${operation}'")
            .Build();

        // Act
        receiveTimeout.Execute(Context);
    }
}
