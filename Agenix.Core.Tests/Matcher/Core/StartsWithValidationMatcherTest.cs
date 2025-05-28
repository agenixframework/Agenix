using System.Collections.Generic;
using Agenix.Api.Exceptions;
using Agenix.Core.Validation.Matcher.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Matcher.Core;

public class StartsWithValidationMatcherTest : AbstractNUnitSetUp
{
    private readonly StartsWithValidationMatcher _matcher = new();

    [Test]
    public void TestValidateSuccess()
    {
        _matcher.Validate("field", "This is a test", new List<string> { "" }, Context);
        _matcher.Validate("field", "This is a test", new List<string> { "T" }, Context);
        _matcher.Validate("field", "This is a test", new List<string> { "This" }, Context);
        _matcher.Validate("field", "This is a test", new List<string> { "This is" }, Context);
    }

    [Test]
    public void TestValidateError()
    {
        AssertException("field", "This is a test", new List<string> { "his" });
        AssertException("field", "This is a test", new List<string> { "test" });
    }

    private void AssertException(string fieldName, string value, List<string> control)
    {
        try
        {
            _matcher.Validate(fieldName, value, control, Context);
            Assert.Fail("Expected exception not thrown!");
        }
        catch (ValidationException e)
        {
            ClassicAssert.IsTrue(e.GetMessage().Contains(fieldName));
            ClassicAssert.IsTrue(e.GetMessage().Contains(control[0]));
            ClassicAssert.IsTrue(e.GetMessage().Contains(value));
        }
    }
}
