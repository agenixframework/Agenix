using System;
using System.Collections.Generic;
using System.Linq;
using FleetPay.Core.Validation.Matcher;
using NUnit.Framework;
using TestContext = FleetPay.Core.TestContext;

namespace FleetPay.Acceptance.Test.API.Specs.Support.Matchers
{
    /// <summary>
    /// Asserts that expected and actual are exactly equal.  The collections must have the same count,
    /// and contain the exact same objects in the same order.
    /// </summary>
    public class HasItemsMatcher : IValidationMatcher
    {
        public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
        {
            var actualValues = value.Split(',', StringSplitOptions.TrimEntries).ToList();
            CollectionAssert.AreEqual(controlParameters, actualValues);
        }
    }
}