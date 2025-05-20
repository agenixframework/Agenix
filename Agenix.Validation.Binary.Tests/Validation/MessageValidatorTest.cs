using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Core.Validation;
using Agenix.Validation.Binary.Validation;
using NUnit.Framework;

namespace Agenix.Validation.Binary.Tests.Validation;

/// <summary>
/// A test suite for validating the functionality and behavior of message validators.
/// This class is designed to verify the correct lookup and configuration of message validators
/// in the Agenix application, ensuring their proper registration and implementation.
/// </summary>
public class MessageValidatorTest
{
    [Test]
    public void ShouldLookupValidators()
    {
        //WHEN
        var validators = IMessageValidator<IValidationContext>.Lookup();

        //THEN
        Assert.That(validators, Has.Count.EqualTo(2));

        Assert.That(validators["header"], Is.Not.Null);
        Assert.That(validators["header"],
            Is.TypeOf<DefaultMessageHeaderValidator>());

        Assert.That(validators["binary"], Is.Not.Null);
        Assert.That(validators["binary"],
            Is.TypeOf<BinaryMessageValidator>());
    }

    [Test]
    public void ShouldLookupSpecificValidators()
    {
        //WHEN/THEN
        Assert.That(IMessageValidator<IValidationContext>.Lookup("header").IsPresent, Is.True);
        Assert.That(IMessageValidator<IValidationContext>.Lookup("binary").IsPresent, Is.True);
    }
}