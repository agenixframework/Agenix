using Agenix.Api.Functions;
using Agenix.Core.Functions;
using Agenix.Validation.Json.Functions;
using NUnit.Framework;

namespace Agenix.Validation.Json.Tests.Functions;

public class JsonFunctionsTest : AbstractNUnitSetUp
{
    [Test]
    public void TestJsonPath()
    {
        Assert.That(JsonFunctions.JsonPath("{\"text\": \"Some Text\"}", "$.text", Context), Is.EqualTo("Some Text"));
    }

    [Test]
    public void TestFunctionUtils()
    {
        Context.FunctionRegistry = new DefaultFunctionRegistry();
        Assert.That(
            FunctionUtils.ResolveFunction("core:jsonPath('{\"message\": \"Hello Agenix!\"}', '$.message')", Context),
            Is.EqualTo("Hello Agenix!"));
    }
}
