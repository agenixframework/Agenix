using System.Collections.Generic;
using Agenix.Api.Exceptions;
using Agenix.Core.Validation.Matcher.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Matcher.Core;

public class GreaterThanValidationMatcherTest : AbstractNUnitSetUp
{
    private readonly GreaterThanValidationMatcher _matcher = new();

    [Test]
    public void TestValidateSuccess()
    {
        _matcher.Validate("field", "3", new List<string> { "2" }, Context);
        _matcher.Validate("field", "1", new List<string> { "-1" }, Context);
        _matcher.Validate("field", "0.000000001", new List<string> { "0" }, Context);
        _matcher.Validate("field", "0", new List<string> { "-0.000000001" }, Context);
    }

    [Test]
    public void TestValidateError()
    {
        AssertException("field", "NaN", new List<string> { "2" });
        AssertException("field", "2", new List<string> { "NaN" });
        AssertException("field", "2.0", new List<string> { "2.0" });
        AssertException("field", "2.0", new List<string> { "2.1" });
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