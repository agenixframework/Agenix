using System.Collections.Generic;
using Agenix.Api.Exceptions;
using Agenix.Core.Validation.Matcher.Core;
using NUnit.Framework;

namespace Agenix.Core.Tests.Matcher.Core;

public class IgnoreNewLineValidationMatcherTest : AbstractNUnitSetUp
{
    private readonly IgnoreNewLineValidationMatcher _matcher = new();

    [Test]
    public void TestValidateSuccess()
    {
        _matcher.Validate("field", "value", new List<string> { "value" }, Context);
        _matcher.Validate("field", "value1 \nvalue2 \nvalue3!", new List<string> { "value1 value2 value3!" },
            Context);
        _matcher.Validate("field", "\nvalue1 \nvalue2 \nvalue3!\n", new List<string> { "value1 value2 value3!" },
            Context);
        _matcher.Validate("field", "value1 \r\nvalue2 \r\nvalue3!\r\n", new List<string> { "value1 value2 value3!" },
            Context);
        _matcher.Validate("field", "\r\nvalue1 \r\nvalue2 \r\nvalue3!", new List<string> { "value1 value2 value3!" },
            Context);
        _matcher.Validate("field", "value1 \n\n\nvalue2 \n\nvalue3!", new List<string> { "value1 value2 value3!" },
            Context);
        _matcher.Validate("field", "value1 \r\n\r\n\r\nvalue2 \r\n\r\nvalue3!",
            new List<string> { "value1 value2 value3!" }, Context);
        _matcher.Validate("field", "value1 \n\n\nvalue2 \n\nvalue3!", new List<string> { "value1 value2 value3!" },
            Context);
    }

    [Test]
    public void TestValidateError()
    {
        Assert.Throws<ValidationException>(() =>
            _matcher.Validate("field", "value1 \nvalue2 \nvalue3!", new List<string> { "value1! value2! value3!" },
                Context)
        );
    }
}
