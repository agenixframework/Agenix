using System.Collections.Generic;
using FleetPay.Core.Exceptions;
using FleetPay.Core.Validation.Matcher.Core;
using NUnit.Framework;

namespace FleetPay.Core.NUnitTestProject.Matcher.Core
{
    public class StartsWithValidationMatcherTest : AbstractNUnitSetUp
    {
        private readonly StartsWithValidationMatcher _matcher = new();

        [Test]
        public void TestValidateSuccess()
        {
            _matcher.Validate("field", "This is a test", new List<string> {""}, Context);
            _matcher.Validate("field", "This is a test", new List<string> {"T"}, Context);
            _matcher.Validate("field", "This is a test", new List<string> {"This"}, Context);
            _matcher.Validate("field", "This is a test", new List<string> {"This is"}, Context);
        }

        [Test]
        public void TestValidateError()
        {
            AssertException("field", "This is a test", new List<string> {"his"});
            AssertException("field", "This is a test", new List<string> {"test"});
        }

        private void AssertException(string fieldName, string value, List<string> control)
        {
            try
            {
                _matcher.Validate(fieldName, value, control, Context);
                Assert.Fail("Expected exception not thrown!");
            }
            catch (ValidationException e)
            {
                Assert.IsTrue(e.GetMessage().Contains(fieldName));
                Assert.IsTrue(e.GetMessage().Contains(control[0]));
                Assert.IsTrue(e.GetMessage().Contains(value));
            }
        }
    }
}