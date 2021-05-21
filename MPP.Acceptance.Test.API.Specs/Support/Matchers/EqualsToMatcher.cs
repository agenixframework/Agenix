using System.Collections.Generic;
using MPP.Core.Validation.Matcher;
using NUnit.Framework;
using TestContext = MPP.Core.TestContext;

namespace MPP.Acceptance.Test.API.Specs.Support.Matchers
{
    public class EqualsToMatcher : IValidationMatcher
    {
        public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
        {
            Assert.That(value, Is.EqualTo(controlParameters[0]));
        }
    }
}