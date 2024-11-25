using Agenix.Core.Validation;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Validation;

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
        ClassicAssert.AreEqual(1, validators.Count);
        ClassicAssert.IsNotNull(validators["defaultHeaderValidator"]);
        ClassicAssert.AreEqual(validators["defaultHeaderValidator"].GetType(), typeof(DefaultHeaderValidator));
    }

    [Test]
    public void TestDefaultLookup()
    {
        var validator = IHeaderValidator.Lookup("default");
        ClassicAssert.IsTrue(validator.IsPresent);
    }
}