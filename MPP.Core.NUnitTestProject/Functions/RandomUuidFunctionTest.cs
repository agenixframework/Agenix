using System.Collections.Generic;
using FleetPay.Core.Functions.Core;
using NUnit.Framework;

namespace FleetPay.Core.NUnitTestProject.Functions
{
    public class RandomUuidFunctionTest : AbstractNUnitSetUp
    {
        private readonly RandomUuidFunction _function = new();

        [Test]
        public void TestRandomUuidFunction()
        {
            Assert.IsNotNull(_function.Execute(new List<string>(), Context));
        }
    }
}