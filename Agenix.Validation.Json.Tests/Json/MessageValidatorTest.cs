using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Core.Validation;
using Agenix.Validation.Json.Validation;
using NUnit.Framework;

namespace Agenix.Validation.Json.Tests.Json;

public class MessageValidatorTest
{
    [Test]
    public void TestLookup()
    {
        var validators = IMessageValidator<IValidationContext>.Lookup();
        Assert.That(validators.Count, Is.EqualTo(3));
        Assert.That(validators["header"], Is.Not.Null);
        Assert.That(validators["header"], Is.TypeOf<DefaultMessageHeaderValidator>());
        Assert.That(validators["json"], Is.Not.Null);
        Assert.That(validators["json"], Is.TypeOf<JsonTextMessageValidator>());
        Assert.That(validators["json-path"], Is.Not.Null);
        Assert.That(validators["json-path"], Is.TypeOf<JsonPathMessageValidator>());
    }

    [Test]
    public void TestTestLookup()
    {
        Assert.That(IMessageValidator<IValidationContext>.Lookup("header").IsPresent);
        Assert.That(IMessageValidator<IValidationContext>.Lookup("json").IsPresent);
        Assert.That(IMessageValidator<IValidationContext>.Lookup("json-path").IsPresent);
    }
}
