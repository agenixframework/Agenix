using System.Collections.Generic;
using FleetPay.Core.Exceptions;
using FleetPay.Core.Functions.Core;
using NUnit.Framework;

namespace FleetPay.Core.NUnitTestProject.Functions
{
    public class RandomStringFunctionTest : AbstractNUnitSetUp
    {
        private readonly RandomStringFunction _function = new();

        [Test]
        public void TestRandomStringFunction()
        {
            var parameters = new List<string> {"3"};

            Assert.AreEqual(_function.Execute(parameters, Context).Length, 3);

            parameters = new List<string> {"3", RandomStringFunction.Uppercase};

            Assert.AreEqual(_function.Execute(parameters, Context).Length, 3);

            parameters = new List<string> {"3", RandomStringFunction.Lowercase};

            Assert.AreEqual(_function.Execute(parameters, Context).Length, 3);

            parameters = new List<string> {"3", RandomStringFunction.Mixed};

            Assert.AreEqual(_function.Execute(parameters, Context).Length, 3);

            parameters = new List<string> {"3", "UNKNOWN"};

            Assert.AreEqual(_function.Execute(parameters, Context).Length, 3);
        }

        [Test]
        public void TestWithNumbers()
        {
            var parameters = new List<string> {"10", RandomStringFunction.Uppercase, "true"};

            Assert.AreEqual(_function.Execute(parameters, Context).Length, 10);

            parameters = new List<string> {"10", RandomStringFunction.Lowercase, "true"};

            Assert.AreEqual(_function.Execute(parameters, Context).Length, 10);

            parameters = new List<string> {"10", RandomStringFunction.Mixed, "true"};

            Assert.AreEqual(_function.Execute(parameters, Context).Length, 10);

            parameters = new List<string> {"10", "UNKNOWN", "true"};

            Assert.AreEqual(_function.Execute(parameters, Context).Length, 10);
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