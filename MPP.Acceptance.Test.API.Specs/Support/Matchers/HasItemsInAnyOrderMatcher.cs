using System;
using System.Collections.Generic;
using System.Linq;
using FleetPay.Core.Validation.Matcher;
using NUnit.Framework;
using TestContext = FleetPay.Core.TestContext;

namespace FleetPay.Acceptance.Test.API.Specs.Support.Matchers
{
    /// <summary>
    ///     Asserts that expected and actual are equivalent, containing the same objects but the match may be in any order.
    /// </summary>
    public class HasItemsInAnyOrderMatcher : IValidationMatcher
    {
        public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
        {
            var actualValues = value.Split(',', StringSplitOptions.TrimEntries).ToList();
            actualValues = actualValues.Select(s => s.Replace("[", "").Replace("]","").Replace("\"","")).ToList();
            CollectionAssert.AreEquivalent(controlParameters, actualValues);
        }
    }
}