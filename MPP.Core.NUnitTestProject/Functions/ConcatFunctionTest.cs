using System.Collections.Generic;
using MPP.Core.Functions.Core;
using NUnit.Framework;

namespace MPP.Core.NUnitTestProject.Functions
{
    public class ConcatFunctionTest : AbstractNUnitSetUp
    {
        private readonly ConcatFunction _function = new();

        [Test]
        public void TestConcatFunction()
        {
            var parameterList = new List<string> {"Hallo ", "TestFramework", "!"};


            Assert.AreEqual(_function.Execute(parameterList, Context), "Hallo TestFramework!");
        }
    }
}