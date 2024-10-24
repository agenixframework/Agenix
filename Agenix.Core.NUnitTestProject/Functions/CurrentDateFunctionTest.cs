using System;
using System.Collections.Generic;
using Agenix.Core.Functions;
using Agenix.Core.Functions.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Functions
{
    public class CurrentDateFunctionTest : AbstractNUnitSetUp
    {
        private readonly CurrentDateFunction _currentDateFunction = new();

        [Test]
        public void TestCurrentDateFunction()
        {
            ClassicAssert.AreEqual(
                _currentDateFunction.Execute(FunctionParameterHelper.GetParameterList("'yyyy-MM-dd'"), Context),
                DateTime.Now.ToString("yyyy-MM-dd"));
        }

        [Test]
        public void TestNoParameters()
        {
            ClassicAssert.AreEqual(_currentDateFunction.Execute([], Context),
                DateTime.Now.ToString("dd.MM.yyyy"));
        }
    }
}