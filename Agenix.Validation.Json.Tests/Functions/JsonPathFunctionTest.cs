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
        Assert.That(_function.Execute(parameters, Context), Is.EqualTo("Sheldon"));
    }

    [Test]
    public void TestExecuteJsonPathWithKeySet()
    {
        var parameters = new List<string> { _jsonSource, "$.person.KeySet()" };
        Assert.That(_function.Execute(parameters, Context), Is.EqualTo("name, age"));
    }

    [Test]
    public void TestExecuteJsonPathWithValues()
    {
        var parameters = new List<string> { _jsonSource, "$.person.Values()" };
        Assert.That(_function.Execute(parameters, Context), Is.EqualTo("Sheldon, 29"));
    }

    [Test]
    public void TestExecuteJsonPathWithSize()
    {
        var parameters = new List<string> { _jsonSource, "$.person.Size()" };
        Assert.That(_function.Execute(parameters, Context), Is.EqualTo("2"));
    }

    [Test]
    public void TestExecuteJsonPathWithToString()
    {
        var parameters = new List<string> { _jsonSource, "$.person.ToString()" };
        Assert.That(_function.Execute(parameters, Context), Is.EqualTo("{\"name\":\"Sheldon\",\"age\":\"29\"}"));
    }

    [Test]
    public void TestExecuteJsonPathUnknown()
    {
        var parameters = new List<string> { _jsonSource, "$.person.unknown" };
        Assert.Throws<AgenixSystemException>(() => _function.Execute(parameters, Context));
    }
}
