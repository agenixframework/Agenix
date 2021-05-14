using System;
using System.Collections.Generic;
using MPP.Core.Functions;
using MPP.Core.Functions.Core;
using NUnit.Framework;

namespace MPP.Core.NUnitTestProject.Functions
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