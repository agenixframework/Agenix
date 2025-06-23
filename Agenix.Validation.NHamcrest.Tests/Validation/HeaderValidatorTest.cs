using Agenix.Api.Validation;
using Agenix.Core.Validation;
using Agenix.Validation.NHamcrest.Validation;
using NUnit.Framework.Legacy;

namespace Agenix.Validation.NHamcrest.Tests.Validation;

/// <summary>
///     Unit tests for the <see cref="IHeaderValidator" /> interface and its default implementation
///     <see cref="DefaultHeaderValidator" />.
/// </summary>
public class HeaderValidatorTest
{
    [Test]
    public void TestLookup()
    {
        var validators = IHeaderValidator.Lookup();
        Assert.That(validators.Count, Is.EqualTo(2));
        ClassicAssert.IsNotNull(validators["defaultHeaderValidator"]);
        Assert.That(typeof(DefaultHeaderValidator), Is.EqualTo(validators["defaultHeaderValidator"].GetType()));
        ClassicAssert.IsNotNull(validators["hamcrestHeaderValidator"]);
        Assert.That(typeof(NHamcrestHeaderValidator), Is.EqualTo(validators["hamcrestHeaderValidator"].GetType()));
    }

    [Test]
    public void TestDefaultLookup()
    {
        var validator = IHeaderValidator.Lookup("default");
        ClassicAssert.IsTrue(validator.IsPresent);
        validator = IHeaderValidator.Lookup("nhamcrest");
        ClassicAssert.IsTrue(validator.IsPresent);
    }
}
