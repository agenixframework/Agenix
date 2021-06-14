using System.Collections.Generic;
using FleetPay.Core.Exceptions;
using FleetPay.Core.Validation.Matcher.Core;
using NUnit.Framework;

namespace FleetPay.Core.NUnitTestProject.Matcher.Core
{
    public class TrimAllWhitespacesValidationMatcherTest : AbstractNUnitSetUp
    {
        private readonly TrimAllWhitespacesValidationMatcher _matcher = new();

        [Test]
        public void TestValidateSuccess()
        {
            _matcher.Validate("field", "This is a value", new List<string> {"Thisisavalue"}, Context);
            _matcher.Validate("field", " This is a value", new List<string> {"Thisisavalue"}, Context);
            _matcher.Validate("field", "  This is a value  ", new List<string> {"Thisisavalue"}, Context);
            _matcher.Validate("field", "  This is a value  ", new List<string> {"This is a value   "}, Context);
        }
        
        [Test]
        public void TestValidateError()
        {
            Assert.Throws<ValidationException>(() =>
                _matcher.Validate("field", "This is a value", new List<string> { "This is a wrong value" }, Context)
            );
        }
    }
}