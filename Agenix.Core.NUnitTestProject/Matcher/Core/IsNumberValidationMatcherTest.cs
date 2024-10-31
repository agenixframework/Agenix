using System.Collections.Generic;
using Agenix.Core.Exceptions;
using Agenix.Core.Validation.Matcher.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Matcher.Core;

public class IsNumberValidationMatcherTest : AbstractNUnitSetUp
{
    private readonly IsNumberValidationMatcher _matcher = new();

    [Test]
    public void TestValidateSuccess()
    {
        // control is irrelevant here
        _matcher.Validate("field", "2", new List<string>(), Context);
        _matcher.Validate("field", "-1", new List<string>(), Context);
        _matcher.Validate("field", "-0.000000001", new List<string>(), Context);
        _matcher.Validate("field", "1E+07", new List<string>(), Context);
        _matcher.Validate("field", "1E-7", new List<string>(), Context);
    }

    [Test]
    public void TestValidateError()
    {
        AssertException("field", "2a", new List<string>());
        AssertException("field", "a2.0", new List<string>());
        AssertException("field", "2.1A+07", new List<string>());
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
        }
    }
}