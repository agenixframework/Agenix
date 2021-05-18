using System.Collections.Generic;
using MPP.Core.Exceptions;
using MPP.Core.Functions.Core;
using NUnit.Framework;

namespace MPP.Core.NUnitTestProject.Functions
{
    public class JsonPathFunctionTest : AbstractNUnitSetUp
    {
        private readonly JsonPathFunction _function = new();

        private readonly string _jsonSource = @"{ 'person': { 'name': 'Sheldon', 'age': '29' } }";

        [Test]
        public void TestExecuteJsonPath()
        {
            var parameters = new List<string> {_jsonSource, "$.person.name"};
            Assert.AreEqual(_function.Execute(parameters, Context), "Sheldon");
        }

        [Test]
        public void TestExecuteJsonPathWithKeySet()
        {
            var parameters = new List<string> {_jsonSource, "$.person.KeySet()"};
            Assert.AreEqual(_function.Execute(parameters, Context), "name, age");
        }

        [Test]
        public void TestExecuteJsonPathWithValues()
        {
            var parameters = new List<string> {_jsonSource, "$.person.Values()"};
            Assert.AreEqual(_function.Execute(parameters, Context), "Sheldon, 29");
        }

        [Test]
        public void TestExecuteJsonPathWithSize()
        {
            var parameters = new List<string> {_jsonSource, "$.person.Size()"};
            Assert.AreEqual(_function.Execute(parameters, Context), "2");
        }

        [Test]
        public void TestExecuteJsonPathWithToString()
        {
            var parameters = new List<string> {_jsonSource, "$.person.ToString()"};
            Assert.AreEqual("{\"name\":\"Sheldon\",\"age\":\"29\"}", _function.Execute(parameters, Context));
        }

        [Test]
        public void TestExecuteJsonPathUnknown()
        {
            var parameters = new List<string> {_jsonSource, "$.person.unknown"};
            Assert.Throws<CoreSystemException>(() => _function.Execute(parameters, Context));
        }
    }
}