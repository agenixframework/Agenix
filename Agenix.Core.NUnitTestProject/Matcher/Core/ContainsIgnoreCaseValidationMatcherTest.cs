using System.Collections.Generic;
using Agenix.Core.Exceptions;
using Agenix.Core.Validation.Matcher.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Matcher.Core;

public class ContainsIgnoreCaseValidationMatcherTest : AbstractNUnitSetUp
{
    private readonly ContainsIgnoreCaseValidationMatcher _matcher = new();

    [Test]
    public void TestValidateSuccess()
    {
        _matcher.Validate("field", "This is a test", new List<string> { "is a" }, Context);
        _matcher.Validate("field", "This is a test", new List<string> { "this" }, Context);
        _matcher.Validate("field", "This is a test", new List<string> { "TEST" }, Context);
        _matcher.Validate("field", "This is a 0815test", new List<string> { "0815" }, Context);
        _matcher.Validate("field", "This is a test", new List<string> { " " }, Context);
        _matcher.Validate("field", "This is a test", new List<string> { " IS A " }, Context);
    }

    [Test]
    public void TestValidateError()
    {
        AssertException("field", "This is a test", new List<string> { "0815" });
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
            ClassicAssert.IsTrue(e.GetMessage().Contains(value));
            ClassicAssert.IsTrue(e.GetMessage().Contains(control[0]));
        }
    }
}