using System.Collections.Generic;
using FleetPay.Core.Validation.Matcher;
using NUnit.Framework;
using TestContext = FleetPay.Core.TestContext;

namespace FleetPay.Acceptance.Test.API.Specs.Support.Matchers
{
    public class EqualsToMatcher : IValidationMatcher
    {
        public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
        {
            Assert.That(value, Is.EqualTo(controlParameters[0]));
        }
    }
}