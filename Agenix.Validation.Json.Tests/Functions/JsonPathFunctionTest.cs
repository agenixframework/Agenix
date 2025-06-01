using Agenix.Api.Exceptions;
using Agenix.Validation.Json.Functions.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Validation.Json.Tests.Functions;

public class JsonPathFunctionTest : AbstractNUnitSetUp
{
    private readonly JsonPathFunction _function = new();

    private readonly string _jsonSource = @"{ 'person': { 'name': 'Sheldon', 'age': '29' } }";

    [Test]
    public void TestExecuteJsonPath()
    {
        var parameters = new List<string> { _jsonSource, "$.person.name" };
        ClassicAssert.AreEqual(_function.Execute(parameters, Context), "Sheldon");
    }

    [Test]
    public void TestExecuteJsonPathWithKeySet()
    {
        var parameters = new List<string> { _jsonSource, "$.person.KeySet()" };
        ClassicAssert.AreEqual(_function.Execute(parameters, Context), "name, age");
    }

    [Test]
    public void TestExecuteJsonPathWithValues()
    {
        var parameters = new List<string> { _jsonSource, "$.person.Values()" };
        ClassicAssert.AreEqual(_function.Execute(parameters, Context), "Sheldon, 29");
    }

    [Test]
    public void TestExecuteJsonPathWithSize()
    {
        var parameters = new List<string> { _jsonSource, "$.person.Size()" };
        ClassicAssert.AreEqual(_function.Execute(parameters, Context), "2");
    }

    [Test]
    public void TestExecuteJsonPathWithToString()
    {
        var parameters = new List<string> { _jsonSource, "$.person.ToString()" };
        ClassicAssert.AreEqual("{\"name\":\"Sheldon\",\"age\":\"29\"}", _function.Execute(parameters, Context));
    }

    [Test]
    public void TestExecuteJsonPathUnknown()
    {
        var parameters = new List<string> { _jsonSource, "$.person.unknown" };
        Assert.Throws<AgenixSystemException>(() => _function.Execute(parameters, Context));
    }
}
