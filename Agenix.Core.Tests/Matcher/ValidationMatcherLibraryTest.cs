using Agenix.Api.Exceptions;
using Agenix.Api.Validation.Matcher;
using Agenix.Core.Validation.Matcher;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Matcher;

public class ValidationMatcherLibraryTest
{
    private readonly Mock<IValidationMatcher> _matcher = new();
    private readonly ValidationMatcherLibrary _validationMatcherLibrary = new();

    [OneTimeSetUp]
    public void TestFixtureSetUp()
    {
        _validationMatcherLibrary.Name = "fooValidationMatcherLibrary";
        _validationMatcherLibrary.Prefix = "foo:";
        _validationMatcherLibrary.Members.Add("customMatcher", _matcher.Object);
    }

    [Test]
    public void TestGetValidationMatcher()
    {
        ClassicAssert.IsNotNull(_validationMatcherLibrary.GetValidationMatcher("customMatcher"));
    }

    [Test]
    public void TestKnowsValidationMatcher()
    {
        ClassicAssert.IsTrue(_validationMatcherLibrary.KnowsValidationMatcher("foo:customMatcher()"));
        ClassicAssert.IsFalse(_validationMatcherLibrary.KnowsValidationMatcher("foo:unknownMatcher()"));
    }

    [Test]
    public void TestUnknownValidationMatcher()
    {
        try
        {
            _validationMatcherLibrary.GetValidationMatcher("unknownMatcher");
        }
        catch (NoSuchValidationMatcherException e)
        {
            ClassicAssert.IsTrue(e.GetMessage().Contains("unknownMatcher"));
            ClassicAssert.IsTrue(e.GetMessage().Contains(_validationMatcherLibrary.Name));
            ClassicAssert.IsTrue(e.GetMessage().Contains(_validationMatcherLibrary.Prefix));
        }
    }
}
