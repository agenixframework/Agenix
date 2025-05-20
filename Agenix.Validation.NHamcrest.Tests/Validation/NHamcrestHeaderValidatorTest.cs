using Agenix.Api.Exceptions;
using Agenix.Api.Validation.Context;
using Agenix.Validation.NHamcrest.Validation;
using Is = NHamcrest.Is;

namespace Agenix.Validation.NHamcrest.Tests.Validation;

public class NHamcrestHeaderValidatorTest : AbstractNUnitSetUp
{
    private readonly NHamcrestHeaderValidator _validator = new();
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