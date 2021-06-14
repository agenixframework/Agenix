using System.Collections.Generic;
using FleetPay.Core.Exceptions;
using FleetPay.Core.Functions.Core;
using NUnit.Framework;

namespace FleetPay.Core.NUnitTestProject.Functions
{
    /// <summary>
    ///     Test the UpperCaseFunction function
    /// </summary>
    public class UpperCaseFunctionTest : AbstractNUnitSetUp
    {
        private readonly UpperCaseFunction _upperCaseFunction = new();

        [Test]
        public void TestUpperCaseFunction()
        {
            Assert.AreEqual(_upperCaseFunction.Execute(new List<string> {"1000"}, Context), "1000");
            Assert.AreEqual(_upperCaseFunction.Execute(new List<string> {"hallo TestFramework!"}, Context),
                "HALLO TESTFRAMEWORK!");
            Assert.AreEqual(_upperCaseFunction.Execute(new List<string> {"Today is: 09.02.2012"}, Context),
                "TODAY IS: 09.02.2012");
        }

        [Test]
        public void TestNoParameters()
        {
            Assert.Throws<InvalidFunctionUsageException>(() =>
                _upperCaseFunction.Execute(new List<string>(), Context)
            );
        }
    }
}