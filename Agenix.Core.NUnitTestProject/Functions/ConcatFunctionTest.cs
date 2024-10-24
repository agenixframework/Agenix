using System.Collections.Generic;
using Agenix.Core.Functions.Core;
using Agenix.Core.NUnitTestProject;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Functions
{
    public class ConcatFunctionTest : AbstractNUnitSetUp
    {
        private readonly ConcatFunction _function = new();

        [Test]
        public void TestConcatFunction()
        {
            var parameterList = new List<string> { "Hallo ", "TestFramework", "!" };


            ClassicAssert.AreEqual(_function.Execute(parameterList, Context), "Hallo TestFramework!");
        }
    }
}