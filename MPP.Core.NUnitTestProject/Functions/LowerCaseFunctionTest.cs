using System.Collections.Generic;
using FleetPay.Core.Exceptions;
using FleetPay.Core.Functions.Core;
using NUnit.Framework;

namespace FleetPay.Core.NUnitTestProject.Functions
{
    public class LowerCaseFunctionTest : AbstractNUnitSetUp
    {
        private readonly LowerCaseFunction _function = new();

        [Test]
        public void TestLowerCaseFunction()
        {
            Assert.AreEqual(_function.Execute(new List<string> {"1000"}, Context), "1000");
            Assert.AreEqual(_function.Execute(new List<string> {"hallo TestFramework!"}, Context),
                "hallo testframework!");
            Assert.AreEqual(_function.Execute(new List<string> {"Today is: 09.02.2012"}, Context),
                "today is: 09.02.2012");
        }

        [Test]
        public void TestNoParameters()
        {
            Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute(new List<string>(), Context));
        }
    }
}