using System.Collections.Generic;
using Agenix.Api.Exceptions;
using Agenix.Core.Validation.Matcher.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Matcher.Core;

public class ContainsValidationMatcherTest : AbstractNUnitSetUp
{
    private readonly ContainsValidationMatcher _matcher = new();

    [Test]
    public void TestValidateSuccess()
    {
        _matcher.Validate("field", "This is a test", ["is a"], Context);
        _matcher.Validate("field", "This is a test", ["This"], Context);
        _matcher.Validate("field", "This is a test", ["test"], Context);
        _matcher.Validate("field", "This is a 0815test", ["0815"], Context);
        _matcher.Validate("field", "This is a test", new List<string> { " " }, Context);
    }

    [Test]
    public void TestValidateError()
    {
        AssertException("field", "This is a test", new List<string> { "0815" });
        AssertException("field", null, new List<string> { "control" });
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

            if (value != null)
            {
                ClassicAssert.IsTrue(e.GetMessage().Contains(value));
            }
        }
    }
}
