using Agenix.Core.Exceptions;
using Agenix.Core.Message;
using Agenix.Core.Validation;
using Agenix.Core.Validation.Context;
using Moq;
using NUnit.Framework;

namespace Agenix.Core.NUnitTestProject.Validation;

/// <summary>
///     Unit tests for the DefaultEmptyMessageValidator class.
/// </summary>
public class DefaultEmptyMessageValidatorTest
{
    private Mock<IMessage> _controlMock;
    private Mock<IMessage> _receivedMock;
    private DefaultEmptyMessageValidator _validator;

    [SetUp]
    public void SetupMocks()
    {
        _receivedMock = new Mock<IMessage>();
        _controlMock = new Mock<IMessage>();
        _validator = new DefaultEmptyMessageValidator();
    }

    [Test]
    public void ShouldValidateEmptyMessage()
    {
        // Arrange
        _receivedMock.Setup(m => m.GetPayload<string>()).Returns(string.Empty);
        _controlMock.Setup(m => m.Payload).Returns(string.Empty);
        _controlMock.Setup(m => m.GetPayload<string>()).Returns(string.Empty);

        // Act and Assert
        Assert.DoesNotThrow(() =>
        {
            _validator.ValidateMessage(_receivedMock.Object, _controlMock.Object, new TestContext(),
                new DefaultValidationContext());
        });
    }

    [Test]
    public void ShouldSkipNullControlMessageMessage()
    {
        _validator.ValidateMessage(_receivedMock.Object, _controlMock.Object, new TestContext(),
            new DefaultValidationContext());
    }

    [Test]
    public void ShouldValidateNonEmptyMessage()
    {
        // Arrange
        _receivedMock.Setup(m => m.GetPayload<string>()).Returns("Hello");
        _controlMock.Setup(m => m.Payload).Returns(string.Empty);
        _controlMock.Setup(m => m.GetPayload<string>()).Returns(string.Empty);

        // Act and Assert
        var exception = Assert.Throws<ValidationException>(() =>
        {
            _validator.ValidateMessage(_receivedMock.Object, _controlMock.Object, new TestContext(),
                new DefaultValidationContext());
        });

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("Validation failed - received message content is not empty!"));
    }

    [Test]
    public void ShouldValidateInvalidControlMessage()
    {
        // Arrange
        _receivedMock.Setup(m => m.GetPayload<string>()).Returns(string.Empty);
        _controlMock.Setup(m => m.Payload).Returns("Hello");
        _controlMock.Setup(m => m.GetPayload<string>()).Returns("Hello");

        // Act and Assert
        var exception = Assert.Throws<ValidationException>(() =>
        {
            _validator.ValidateMessage(_receivedMock.Object, _controlMock.Object, new TestContext(),
                new DefaultValidationContext());
        });

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("Empty message validation failed - control message is not empty!"));
    }
}