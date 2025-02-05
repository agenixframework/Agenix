using System.Collections.Generic;
using Agenix.Core.Exceptions;
using Agenix.Core.Validation.Context;
using Agenix.Core.Validation.Matcher;
using NUnit.Framework;
using Is = NHamcrest.Is;

namespace Agenix.Core.NUnitTestProject.Validation;

public class HamcrestHeaderValidatorTest : AbstractNUnitSetUp
{
    private readonly HamcrestHeaderValidator _validator = new();
    private readonly HeaderValidationContext _validationContext = new();
    
    public static IEnumerable<TestCaseData> SuccessData
    {
        get
        {
            yield return new TestCaseData("foo", "foo");
            yield return new TestCaseData("foo", Is.EqualTo("foo"));
        }
    }
    
    public static IEnumerable<TestCaseData> ErrorData
    {
        get
        {
            yield return new TestCaseData("foo", "wrong");
            yield return new TestCaseData("foo", Is.EqualTo("wrong"));
        }
    }
    
    [TestCaseSource(nameof(SuccessData))]
    public void TestValidateHeaderSuccess(object receivedValue, object controlValue)
    {
        _validator.ValidateHeader("foo", receivedValue, controlValue, Context, _validationContext);
    }
    
    [TestCaseSource(nameof(ErrorData))]
    public void TestValidateHeaderError(object receivedValue, object controlValue)
    {
        Assert.Throws<ValidationException>(() =>
        {
            _validator.ValidateHeader("foo", receivedValue, controlValue, Context, _validationContext);
        });
    }
}