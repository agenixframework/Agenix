using System;
using System.Collections.Generic;
using MPP.Core.Exceptions;
using MPP.Core.Functions.Core;
using NUnit.Framework;

namespace MPP.Core.NUnitTestProject.Functions
{
    public class RandomNumberFunctionTest : AbstractNUnitSetUp
    {
        private readonly RandomNumberFunction _function = new();

        [Test]
        public void TestRandomStringFunction()
        {
            var parameters = new List<string> {"3"};

            Assert.Less(int.Parse(_function.Execute(parameters, Context)), 1000);

            parameters = new List<string> {"3", "false"};
            var generated = _function.Execute(parameters, Context);

            Assert.LessOrEqual(generated.Length, 3);
            Assert.Greater(generated.Length, 0);
        }

        [Test]
        public void TestLeadingZeroNumbers()
        {
            var generated = RandomNumberFunction.CheckLeadingZeros("0001", true);
            Console.WriteLine(generated);
            Assert.Greater(int.Parse(generated.Substring(0, 1)), 0);

            generated = RandomNumberFunction.CheckLeadingZeros("0009", true);
            Assert.AreEqual(generated.Length, 4);

            generated = RandomNumberFunction.CheckLeadingZeros("00000", true);
            Assert.AreEqual(generated.Length, 5);
            Assert.Greater(int.Parse(generated.Substring(0, 1)), 0);
            Assert.IsTrue(generated.EndsWith("0000"));

            generated = RandomNumberFunction.CheckLeadingZeros("009809", true);
            Assert.AreEqual(generated.Length, 6);
            Assert.Greater(int.Parse(generated.Substring(0, 1)), 0);
            Assert.IsTrue(generated.EndsWith("09809"));

            generated = RandomNumberFunction.CheckLeadingZeros("01209", true);
            Assert.AreEqual(generated.Length, 5);
            Assert.Greater(int.Parse(generated.Substring(0, 1)), 0);
            Assert.IsTrue(generated.EndsWith("1209"));

            generated = RandomNumberFunction.CheckLeadingZeros("1209", true);
            Assert.AreEqual(generated.Length, 4);
            Assert.AreEqual(generated, "1209");

            generated = RandomNumberFunction.CheckLeadingZeros("00000", false);
            Assert.AreEqual(generated.Length, 1);
            Assert.AreEqual(generated, "0");

            generated = RandomNumberFunction.CheckLeadingZeros("0009", false);
            Assert.AreEqual(generated.Length, 1);
            Assert.AreEqual(generated, "9");

            generated = RandomNumberFunction.CheckLeadingZeros("01209", false);
            Assert.AreEqual(generated.Length, 4);
            Assert.AreEqual(generated, "1209");

            generated = RandomNumberFunction.CheckLeadingZeros("1209", false);
            Assert.AreEqual(generated.Length, 4);
            Assert.AreEqual(generated, "1209");
        }

        [Test]
        public void TestWrongParameterUsage()
        {
            Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute(new List<string> {"-1"}, Context));
        }

        [Test]
        public void TestNoParameterUsage()
        {
            Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute(new List<string>(), Context));
        }

        [Test]
        public void TestTooManyParameters()
        {
            var parameters = new List<string> {"3", RandomStringFunction.Uppercase, "true", "too much"};
            Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute(parameters, Context));
        }
    }
}