using System.Collections.Generic;
using MPP.Core.Exceptions;
using MPP.Core.Functions.Core;
using NUnit.Framework;

namespace MPP.Core.NUnitTestProject.Functions
{
    public class DecodeBase64FunctionTest : AbstractNUnitSetUp
    {
        private readonly DecodeBase64Function _function = new();

        [Test]
        public void TestFunction()
        {
            Assert.AreEqual(_function.Execute(new List<string> {"Zm9v"}, Context), "foo");
        }

        [Test]
        public void TestNoParameterUsage()
        {
            Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute(new List<string>(), Context));
        }
    }
}