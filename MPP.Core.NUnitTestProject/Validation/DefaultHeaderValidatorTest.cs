using System.Collections.Generic;
using MPP.Core.Exceptions;
using MPP.Core.Validation;
using NUnit.Framework;

namespace MPP.Core.NUnitTestProject.Validation
{
    public class DefaultHeaderValidatorTest : AbstractNUnitSetUp
    {
        private readonly DefaultHeaderValidator _validator = new();

        [Test]
        public void TestValidateHeader()
        {
            _validator.ValidateHeader("foo", "foo", "foo", Context);
            _validator.ValidateHeader("foo", null, "", Context);
            _validator.ValidateHeader("foo", null, null, Context);
            _validator.ValidateHeader("foo", new List<string> {"foo", "bar"}, new List<string> {"foo", "bar"}, Context);
        }

        [Test]
        public void TestValidateHeaderVariableSupport()
        {
            Context.SetVariable("control", "bar");

            _validator.ValidateHeader("foo", "bar", "${control}", Context);
        }

        [Test]
        public void TestValidateHeaderValidationMatcherSupport()
        {
            _validator.ValidateHeader("foo", "bar", "@Ignore@", Context);
            _validator.ValidateHeader("foo", "bar", "@StringLength(3)@", Context);
        }

        [Test]
        public void TestValidateHeaderError()
        {
            Assert.Throws<ValidationException>(() => _validator.ValidateHeader("foo", "foo", "wrong", Context));
            Assert.Throws<ValidationException>(() => _validator.ValidateHeader("foo", null, "wrong", Context));
            Assert.Throws<ValidationException>(() => _validator.ValidateHeader("foo", "foo", null, Context));
        }
    }
}