using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPP.Core.Exceptions;
using MPP.Core.Validation.Matcher.Core;
using NUnit.Framework;

namespace MPP.Core.NUnitTestProject.Matcher.Core
{
    public class TrimValidationMatcherTest : AbstractNUnitSetUp
    {
        private readonly TrimValidationMatcher _matcher = new();

        [Test]
        public void TestValidateSuccess()
        {
            _matcher.Validate("field", "value", new List<string> {"value"}, Context);
            _matcher.Validate("field", "value", new List<string> {  "value " }, Context);
            _matcher.Validate("field", "   value   ", new List<string> { "value" }, Context);
            _matcher.Validate("field", "   value   ", new List<string> { "value   " }, Context);
        }

        [Test]
        public void TestValidateError()
        {
            Assert.Throws<ValidationException>(() =>
                _matcher.Validate("field", " value ", new List<string> { "wrong" }, Context)
            );
        }
    }
}
