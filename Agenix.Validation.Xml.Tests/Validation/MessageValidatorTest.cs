using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Core.Validation;
using Agenix.Validation.Xml.Validation.Xhtml;
using Agenix.Validation.Xml.Validation.Xml;
using NUnit.Framework;

namespace Agenix.Validation.Xml.Tests.Validation;

public class MessageValidatorTest
{
    [Test]
    public void TestLookup()
    {
        var validators = IMessageValidator<IValidationContext>.Lookup();
        Assert.That(validators.Count, Is.EqualTo(5));
        Assert.That(validators.ContainsKey("header"), Is.True);
        Assert.That(validators["header"].GetType(), Is.EqualTo(typeof(DefaultMessageHeaderValidator)));
        Assert.That(validators.ContainsKey("xml"), Is.True);
        Assert.That(validators["xml"].GetType(), Is.EqualTo(typeof(DomXmlMessageValidator)));
        Assert.That(validators.ContainsKey("xpath"), Is.True);
        Assert.That(validators["xpath"].GetType(), Is.EqualTo(typeof(XpathMessageValidator)));
        Assert.That(validators.ContainsKey("xhtml"), Is.True);
        Assert.That(validators["xhtml"].GetType(), Is.EqualTo(typeof(XhtmlMessageValidator)));
        Assert.That(validators.ContainsKey("xhtml-xpath"), Is.True);
        Assert.That(validators["xhtml-xpath"].GetType(), Is.EqualTo(typeof(XhtmlXpathMessageValidator)));
    }

    [Test]
    public void TestTestLookup()
    {
        Assert.That(IMessageValidator<IValidationContext>.Lookup("header").IsPresent, Is.True);
        Assert.That(IMessageValidator<IValidationContext>.Lookup("xml").IsPresent, Is.True);
        Assert.That(IMessageValidator<IValidationContext>.Lookup("xpath").IsPresent, Is.True);
        Assert.That(IMessageValidator<IValidationContext>.Lookup("xhtml").IsPresent, Is.True);
        Assert.That(IMessageValidator<IValidationContext>.Lookup("xhtml-xpath").IsPresent, Is.True);
    }
}
