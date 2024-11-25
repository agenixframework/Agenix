using System.Collections.Generic;
using Agenix.Core.Endpoint;
using Agenix.Core.Endpoint.Direct;
using Agenix.Core.Exceptions;
using Agenix.Core.Spi;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Endpoint;

public class DefaultEndpointFactoryTest
{
    private Mock<IReferenceResolver> _referenceResolver;

    [SetUp]
    public void SetUp()
    {
        _referenceResolver = new Mock<IReferenceResolver>();
    }

    [Test]
    public void TestResolveDirectEndpoint()
    {
        // Arrange
        _referenceResolver.Setup(r => r.Resolve<IEndpoint>("myEndpoint")).Returns(new Mock<IEndpoint>().Object);
        var context = new TestContext();
        context.SetReferenceResolver(_referenceResolver.Object);
        var factory = new DefaultEndpointFactory();

        // Act
        var endpoint = factory.Create("myEndpoint", context);

        // Assert
        ClassicAssert.IsNotNull(endpoint);
    }

    [Test]
    public void TestResolveCustomEndpoint()
    {
        // Arrange
        var components = new Dictionary<string, IEndpointComponent>
        {
            { "custom", new DirectEndpointComponent() }
        };

        _referenceResolver.Setup(r => r.ResolveAll<IEndpointComponent>()).Returns(components);
        var context = new TestContext();
        context.SetReferenceResolver(_referenceResolver.Object);

        var factory = new DefaultEndpointFactory();

        // Act
        var endpoint = factory.Create("custom:custom.queue", context);

        // Assert
        ClassicAssert.IsInstanceOf<DirectEndpoint>(endpoint);
        ClassicAssert.AreEqual("custom.queue", ((DirectEndpoint)endpoint).EndpointConfiguration.GetQueueName());
    }

    [Test]
    public void TestOverwriteEndpointComponent()
    {
        // Arrange
        var components = new Dictionary<string, IEndpointComponent>
        {
            { "jms", new DirectEndpointComponent() }
        };

        _referenceResolver.Setup(r => r.ResolveAll<IEndpointComponent>()).Returns(components);
        var context = new TestContext();
        context.SetReferenceResolver(_referenceResolver.Object);

        var factory = new DefaultEndpointFactory();

        // Act
        var endpoint = factory.Create("jms:custom.queue", context);

        // Assert
        ClassicAssert.IsInstanceOf<DirectEndpoint>(endpoint);
        ClassicAssert.AreEqual("custom.queue", ((DirectEndpoint)endpoint).EndpointConfiguration.GetQueueName());
    }

    [Test]
    public void TestResolveUnknownEndpointComponent()
    {
        // Arrange
        _referenceResolver.Setup(r => r.ResolveAll<IEndpointComponent>())
            .Returns(new Dictionary<string, IEndpointComponent>());
        var context = new TestContext();
        context.SetReferenceResolver(_referenceResolver.Object);

        var factory = new DefaultEndpointFactory();

        // Act & Assert
        var ex = Assert.Throws<CoreSystemException>(() => factory.Create("unknown:unknown", context));

        Assert.That(ex, Is.Not.Null);
        ClassicAssert.IsTrue(ex.Message.StartsWith("Unable to create endpoint component"));
    }

    [Test]
    public void TestResolveInvalidEndpointUri()
    {
        // Arrange
        var context = new TestContext();
        context.SetReferenceResolver(_referenceResolver.Object);
        var factory = new DefaultEndpointFactory();

        // Act & Assert
        var ex = Assert.Throws<CoreSystemException>(() => factory.Create("jms:", context));
        Assert.That(ex, Is.Not.Null);
        ClassicAssert.IsTrue(ex.Message.StartsWith("Invalid endpoint uri"));
    }
}