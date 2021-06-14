using System.Collections.Generic;
using FleetPay.Core.Exceptions;
using FleetPay.Core.Functions.Core;
using NUnit.Framework;

namespace FleetPay.Core.NUnitTestProject.Functions
{
    public class EncodeBase64FunctionTest : AbstractNUnitSetUp
    {
        private readonly EncodeBase64Function _function = new();

        [Test]
        public void TestFunction()
        {
            Assert.AreEqual(_function.Execute(new List<string> {"foo"}, Context), "Zm9v");
        }

        [Test]
        public void TestNoParameterUsage()
        {
            Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute(new List<string>(), Context));
        }
    }
}