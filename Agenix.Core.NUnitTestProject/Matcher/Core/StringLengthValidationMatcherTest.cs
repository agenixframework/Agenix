using System.Collections.Generic;
using Agenix.Core.Exceptions;
using Agenix.Core.Validation.Matcher.Core;
using NUnit.Framework;

namespace Agenix.Core.NUnitTestProject.Matcher.Core;

public class StringLengthValidationMatcherTest : AbstractNUnitSetUp
{
    private readonly StringLengthValidationMatcher _matcher = new();

    [Test]
    public void TestValidateSuccess()
    {
        _matcher.Validate("field", "value", new List<string> { "5" }, Context);
    }

    [Test]
    public void TestValidateError()
    {
        Assert.Throws<ValidationException>(() =>
            _matcher.Validate("field", "value", new List<string> { "4" }, Context)
        );
    }

    [Test]
    public void TestValidateInvalidArgument()
    {
        Assert.Throws<ValidationException>(() =>
            _matcher.Validate("field", "value", new List<string> { "foo" }, Context)
        );
    }
}