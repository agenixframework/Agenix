using Agenix.Api.Exceptions;
using Agenix.Api.Validation.Matcher;
using Agenix.Core.Validation.Matcher;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Matcher;

public class ValidationMatcherRegistryTest : AbstractNUnitSetUp
{
    private readonly Mock<IValidationMatcher> _matcher = new();
    private ValidationMatcherLibrary _validationMatcherLibrary;

    [OneTimeSetUp]
    public void TestFixtureSetUp()
    {
        _validationMatcherLibrary = new ValidationMatcherLibrary
        {
            Name = "fooValidationMatcherLibrary",
            Prefix = "foo:"
        };
        _validationMatcherLibrary.Members.Add("customMatcher", _matcher.Object);
    }

    [Test]
    public void TestGetValidationMatcherLibrary()
    {
        Context.ValidationMatcherRegistry.AddValidationMatcherLibrary(_validationMatcherLibrary);
        ClassicAssert.AreEqual(_validationMatcherLibrary,
            Context.ValidationMatcherRegistry.GetLibraryForPrefix("foo:"));
    }

    [Test]
    public void TestUnknownValidationMatcherLibrary()
    {
        try
        {
            Context.ValidationMatcherRegistry.GetLibraryForPrefix("unknown:");
        }
        catch (NoSuchValidationMatcherLibraryException e)
        {
            ClassicAssert.IsTrue(e.GetMessage().Contains("unknown:"));
        }
    }
}