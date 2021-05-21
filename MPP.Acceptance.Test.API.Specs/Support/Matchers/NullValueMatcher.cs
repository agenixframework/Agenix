using System.Collections.Generic;
using MPP.Core.Exceptions;
using MPP.Core.Validation.Matcher;
using NUnit.Framework;
using TestContext = MPP.Core.TestContext;

namespace MPP.Acceptance.Test.API.Specs.Support.Matchers
{
    public class NullValueMatcher : IValidationMatcher
    {
        public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
        {
            switch (controlParameters[0])
            {
                case "NullValue()":
                    Assert.IsNull(value);
                    break;
                case "NotNullValue()":
                    Assert.IsNotNull(value);
                    break;
                default:
                    throw new NoSuchValidationMatcherException("Unknown validation matcher: " +
                                                               controlParameters[0]);
            }
        }
    }
}