using Agenix.Core.Exceptions;
using Agenix.Core.Message;
using Agenix.Core.Validation;
using Agenix.Core.Validation.Context;
using NHamcrest;

namespace Agenix.Validation.NHamcrest.Tests.Validation;

public class DefaultMessageHeaderValidatorTest : AbstractNUnitSetUp
{
    private readonly DefaultMessageHeaderValidator _validator = new();
    private readonly HeaderValidationContext _validationContext = new();

    [Test]
    public void TestValidateMessageHeadersHamcrestMatcherSupport()
    {
        var receivedMessage = new DefaultMessage("Hello World!")
            .SetHeader("foo", "foo_test")
            .SetHeader("additional", "additional")
            .SetHeader("bar", "bar_test");
        var controlMessage = new DefaultMessage("Hello World!")
            .SetHeader("foo", Starts.With("foo"))
            .SetHeader("bar", Ends.With("_test"));

        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
    }
    
    [Test]
    public void TestValidateHamcrestMatcherError()
    {
        var receivedMessage = new DefaultMessage("Hello World!")
            .SetHeader("foo", "foo_test")
            .SetHeader("bar", "bar_test");
        var controlMessage = new DefaultMessage("Hello World!")
            .SetHeader("foo", Starts.With("bar"))
            .SetHeader("bar", Ends.With("_test"));

        Assert.Throws<ValidationException>(() =>
            _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext));
    }
}