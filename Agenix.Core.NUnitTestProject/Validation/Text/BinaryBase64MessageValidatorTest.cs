using System;
using Agenix.Core.Exceptions;
using Agenix.Core.Message;
using Agenix.Core.Validation.Context;
using Agenix.Core.Validation.Text;
using NUnit.Framework;

namespace Agenix.Core.NUnitTestProject.Validation.Text;

/// <summary>
///     A test class for validating messages using BinaryBase64MessageValidator.
/// </summary>
public class BinaryBase64MessageValidatorTest : AbstractNUnitSetUp
{
    private readonly IValidationContext _validationContext = new DefaultValidationContext();
    private readonly BinaryBase64MessageValidator _validator = new();

    [Test]
    public void TestBinaryBase64Validation()
    {
        var receivedMessage = new DefaultMessage("Hello World!"u8.ToArray());
        var controlMessage = new DefaultMessage(Convert.ToBase64String("Hello World!"u8.ToArray()));

        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
    }

    [Test]
    public void TestBinaryBase64ValidationNoBinaryData()
    {
        var receivedMessage = new DefaultMessage("SGVsbG8gV29ybGQh");
        var controlMessage = new DefaultMessage(Convert.ToBase64String("Hello World!"u8.ToArray()));

        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
    }

    [Test]
    public void TestBinaryBase64ValidationError()
    {
        var receivedMessage = new DefaultMessage("Hello World!"u8.ToArray());
        var controlMessage = new DefaultMessage(Convert.ToBase64String("Hello Agenix!"u8.ToArray()));

        var exception = Assert.Throws<ValidationException>(() =>
            _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext));
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message.Contains("expected 'SGVsbG8gQWdlbml4IQ=='"), Is.True);
        Assert.That(exception.Message.Contains("but was 'SGVsbG8gV29ybGQh'"), Is.True);
    }
}