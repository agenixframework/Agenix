using System.Collections.Generic;
using Agenix.Core.Exceptions;
using Agenix.Core.Validation.Matcher.Core;
using NUnit.Framework;

namespace Agenix.Core.NUnitTestProject.Matcher.Core;

public class TrimValidationMatcherTest : AbstractNUnitSetUp
{
    private readonly TrimValidationMatcher _matcher = new();

    [Test]
    public void TestValidateSuccess()
    {
        _matcher.Validate("field", "value", new List<string> { "value" }, Context);
        _matcher.Validate("field", "value", new List<string> { "value " }, Context);
        _matcher.Validate("field", "   value   ", new List<string> { "value" }, Context);
        _matcher.Validate("field", "   value   ", new List<string> { "value   " }, Context);
    }

    [Test]
    public void TestValidateError()
    {
        Assert.Throws<ValidationException>(() =>
            _matcher.Validate("field", " value ", new List<string> { "wrong" }, Context)
        );
    }
}