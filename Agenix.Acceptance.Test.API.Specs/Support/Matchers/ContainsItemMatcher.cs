using System;
using System.Collections.Generic;
using System.Linq;
using FleetPay.Core.Validation.Matcher;
using NUnit.Framework;
using TestContext = FleetPay.Core.TestContext;

namespace FleetPay.Acceptance.Test.API.Specs.Support.Matchers
{
    public class ContainsItemMatcher : IValidationMatcher
    {
        public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
        {
            var actualValues = value.Split(',', StringSplitOptions.TrimEntries).ToList();
            Assert.That(actualValues, Contains.Item(controlParameters[0]));
        }
    }
}