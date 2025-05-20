using Agenix.Api.Exceptions;
using Agenix.Api.Validation.Context;
using Agenix.Core.Message;
using Agenix.Core.Validation;
using NUnit.Framework;

namespace Agenix.Core.NUnitTestProject.Validation;

public class DefaultMessageHeaderValidatorTest : AbstractNUnitSetUp
{
    private readonly HeaderValidationContext _validationContext = new();
    private readonly DefaultMessageHeaderValidator _validator = new();

    [Test]
    public void TestValidateNoMessageHeaders()
    {
        var receivedMessage = new DefaultMessage("Hello World!");
        var controlMessage = new DefaultMessage("Hello World!");

        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
    }

    [Test]
    public void TestValidateMessageHeaders()
    {
        var receivedMessage = new DefaultMessage("Hello World!")
            .SetHeader("foo", "foo_test")
            .SetHeader("additional", "additional")
            .SetHeader("bar", "bar_test");
        var controlMessage = new DefaultMessage("Hello World!")
            .SetHeader("foo", "foo_test")
            .SetHeader("bar", "bar_test");

        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
    }

    [Test]
    public void TestValidateMessageHeadersIgnoreCase()
    {
        try
        {
            var receivedMessage = new DefaultMessage("Hello World!")
                .SetHeader("X-Foo", "foo_test")
                .SetHeader("X-Additional", "additional")
                .SetHeader("X-Bar", "bar_test");
            var controlMessage = new DefaultMessage("Hello World!")
                .SetHeader("x-foo", "foo_test")
                .SetHeader("x-bar", "bar_test");

            _validationContext.HeaderNameIgnoreCase = true;
            _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
        }
        finally
        {
            _validationContext.HeaderNameIgnoreCase = false;
        }
    }

    [Test]
    public void TestValidateMessageHeadersIgnoreCaseError()
    {
        var receivedMessage = new DefaultMessage("Hello World!")
            .SetHeader("X-Foo", "foo_test")
            .SetHeader("X-Additional", "additional")
            .SetHeader("X-Bar", "bar_test");
        var controlMessage = new DefaultMessage("Hello World!")
            .SetHeader("x-foo", "foo_test")
            .SetHeader("x-bar", "bar_test");

        Assert.Throws<ValidationException>(() =>
            _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext));
    }

    [Test]
    public void TestValidateMessageHeadersVariableSupport()
    {
        var receivedMessage = new DefaultMessage("Hello World!")
            .SetHeader("foo", "foo_test")
            .SetHeader("additional", "additional")
            .SetHeader("bar", "bar_test");
        var controlMessage = new DefaultMessage("Hello World!")
            .SetHeader("foo", "core:Concat('foo', '_test')")
            .SetHeader("bar", "${bar}");

        Context.SetVariable("bar", "bar_test");

        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
    }

    [Test]
    public void TestValidateMessageHeadersMatcherSupport()
    {
        var receivedMessage = new DefaultMessage("Hello World!")
            .SetHeader("foo", "foo_test")
            .SetHeader("additional", "additional")
            .SetHeader("bar", "bar_test");
        var controlMessage = new DefaultMessage("Hello World!")
            .SetHeader("foo", "@StartsWith('foo')@")
            .SetHeader("bar", "@EndsWith('_test')@");

        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
    }
    
    [Test]
    public void TestValidateError()
    {
        var receivedMessage = new DefaultMessage("Hello World!")
            .SetHeader("foo", "other_value")
            .SetHeader("bar", "bar_test");
        var controlMessage = new DefaultMessage("Hello World!")
            .SetHeader("foo", "foo_test")
            .SetHeader("bar", "bar_test");

        Assert.Throws<ValidationException>(() =>
            _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext));
    }

    [Test]
    public void TestValidateErrorMissingHeader()
    {
        var receivedMessage = new DefaultMessage("Hello World!")
            .SetHeader("bar", "bar_test");
        var controlMessage = new DefaultMessage("Hello World!")
            .SetHeader("foo", "foo_test")
            .SetHeader("bar", "bar_test");

        Assert.Throws<ValidationException>(() =>
            _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext));
    }
}