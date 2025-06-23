using System.Collections.Generic;
using Agenix.Api.Exceptions;
using Agenix.Api.Validation.Context;
using Agenix.Core.Validation;
using NUnit.Framework;

namespace Agenix.Core.Tests.Validation;

/// <summary>
///     Contains unit tests for the DefaultHeaderValidator class.
/// </summary>
public class DefaultHeaderValidatorTest : AbstractNUnitSetUp
{
    private readonly HeaderValidationContext _validationContext = new();
    private readonly DefaultHeaderValidator _validator = new();

    [Test]
    public void TestValidateHeader()
    {
        _validator.ValidateHeader("foo", "foo", "foo", Context, _validationContext);
        _validator.ValidateHeader("foo", null, "", Context, _validationContext);
        _validator.ValidateHeader("foo", null, null, Context, _validationContext);
        _validator.ValidateHeader("foo", new List<string> { "foo", "bar" }, new List<string> { "foo", "bar" }, Context,
            _validationContext);
        _validator.ValidateHeader("foo", new[] { "foo", "bar" }, new[] { "foo", "bar" }, Context,
            _validationContext);
        _validator.ValidateHeader("foo", new Dictionary<object, object> { { "foo", "bar" } },
            new Dictionary<object, object> { { "foo", "bar" } }, Context,
            _validationContext);
    }

    [Test]
    public void TestValidateHeaderVariableSupport()
    {
        Context.SetVariable("control", "bar");

        _validator.ValidateHeader("foo", "bar", "${control}", Context, _validationContext);
    }

    [Test]
    public void TestValidateHeaderValidationMatcherSupport()
    {
        _validator.ValidateHeader("foo", "bar", "@Ignore@", Context, _validationContext);
        _validator.ValidateHeader("foo", "bar", "@StringLength(3)@", Context, _validationContext);
    }

    [Test]
    public void TestValidateHeaderError()
    {
        Assert.Throws<ValidationException>(() =>
            _validator.ValidateHeader("foo", "foo", "wrong", Context, _validationContext));
        Assert.Throws<ValidationException>(() =>
            _validator.ValidateHeader("foo", null, "wrong", Context, _validationContext));
        Assert.Throws<ValidationException>(() =>
            _validator.ValidateHeader("foo", "foo", null, Context, _validationContext));
        Assert.Throws<ValidationException>(() =>
            _validator.ValidateHeader("foo", new List<string> { "foo", "bar" }, new List<string> { "foo", "wrong" },
                Context, _validationContext));
        Assert.Throws<ValidationException>(() =>
            _validator.ValidateHeader("foo", new[] { "foo", "bar" }, new[] { "foo", "wrong" },
                Context, _validationContext));
        Assert.Throws<ValidationException>(() =>
            _validator.ValidateHeader("foo", new Dictionary<object, object> { { "foo", "bar" } },
                new Dictionary<object, object> { { "foo", "wrong" } },
                Context, _validationContext));
    }
}
