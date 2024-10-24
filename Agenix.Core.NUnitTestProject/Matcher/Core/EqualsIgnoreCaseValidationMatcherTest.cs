using System.Collections.Generic;
using Agenix.Core.Exceptions;
using Agenix.Core.Validation.Matcher.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Matcher.Core
{
    public class EqualsIgnoreCaseValidationMatcherTest : AbstractNUnitSetUp
    {
        private readonly EqualsIgnoreCaseValidationMatcher _matcher = new();

        [Test]
        public void TestValidateSuccess()
        {
            _matcher.Validate("field", "VALUE", new List<string> { "value" }, Context);
            _matcher.Validate("field", "VALUE", new List<string> { "VALUE" }, Context);
            _matcher.Validate("field", "value", new List<string> { "VALUE" }, Context);
            _matcher.Validate("field", "value", new List<string> { "value" }, Context);
            _matcher.Validate("field", "$%& value 123", new List<string> { "$%& VALUE 123" }, Context);
            _matcher.Validate("field", "/() VALUE @&§", new List<string> { "/() VALUE @&§" }, Context);
        }

        [Test]
        public void TestValidateError()
        {
            AssertException("field", "VALUE", new List<string> { "VAIUE" });
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
                ClassicAssert.IsTrue(e.GetMessage().Contains(fieldName));
                ClassicAssert.IsTrue(e.GetMessage().Contains(value));
                ClassicAssert.IsTrue(e.GetMessage().Contains(control[0]));
            }
        }
    }
}