using System;
using System.Collections.Generic;
using FleetPay.Core.Functions;
using FleetPay.Core.Functions.Core;
using NUnit.Framework;

namespace FleetPay.Core.NUnitTestProject.Functions
{
    public class CurrentDateFunctionTest : AbstractNUnitSetUp
    {
        private readonly CurrentDateFunction _currentDateFunction = new();

        [Test]
        public void TestCurrentDateFunction()
        {
            Assert.AreEqual(
                _currentDateFunction.Execute(FunctionParameterHelper.GetParameterList("'yyyy-MM-dd'"), Context),
                DateTime.Now.ToString("yyyy-MM-dd"));
        }

        [Test]
        public void TestNoParameters()
        {
            Assert.AreEqual(_currentDateFunction.Execute(new List<string>(), Context),
                DateTime.Now.ToString("dd.MM.yyyy"));
        }
    }
}