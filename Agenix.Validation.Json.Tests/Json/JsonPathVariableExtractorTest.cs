using Agenix.Core.Message;
using Agenix.Validation.Json.Validation;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Validation.Json.Tests.Json;

public class JsonPathVariableExtractorTest : AbstractNUnitSetUp
{
    private readonly string _jsonPayload =
        "{\"text\":\"Hello World!\", \"person\":{\"name\":\"John\",\"surname\":\"Doe\"}, \"index\":5, \"numbers\": [10,20,30,40], \"id\":\"x123456789x\"}";

    [Test]
    public void TestExtractVariables()
    {
        var variableExtractor = new JsonPathVariableExtractor.Builder()
            .Expression("$.KeySet()", "keySet")
            .Expression("$['index']", "index")
            .Expression("$.numbers", "numbers")
            .Expression("$.numbers.Size()", "numbersSize")
            .Expression("$.person", "person")
            .Expression("$.person.name", "personName")
            .Expression("$.ToString()", "toString")
            .Expression("$.Values()", "values")
            .Expression("$.Size()", "size")
            .Expression("$.*", "all")
            .Expression("$", "root")
            .Build();

        variableExtractor.ExtractVariables(new DefaultMessage(_jsonPayload), Context);

        ClassicAssert.IsNotNull(Context.GetVariable("keySet"));
        Assert.That(Context.GetVariable("keySet"), Is.EqualTo("text, person, index, numbers, id"));


        ClassicAssert.IsNotNull(Context.GetVariable("index"));
        Assert.That(Context.GetVariable("index"), Is.EqualTo("5"));


        ClassicAssert.IsNotNull(Context.GetVariable("numbers"));
        Assert.That(Context.GetVariable("numbers"), Is.EqualTo("[10,20,30,40]"));


        ClassicAssert.IsNotNull(Context.GetVariable("numbersSize"));
        Assert.That(Context.GetVariable("numbersSize"), Is.EqualTo("4"));

        ClassicAssert.IsNotNull(Context.GetVariable("person"));
        Assert.That(Context.GetVariable("person"), Is.EqualTo("{\"name\":\"John\",\"surname\":\"Doe\"}"));

        ClassicAssert.IsNotNull(Context.GetVariable("personName"));
        Assert.That(Context.GetVariable("personName"), Is.EqualTo("John"));

        ClassicAssert.IsNotNull(Context.GetVariable("toString"));
        Assert.That(
            Context.GetVariable("toString"),
            Is.EqualTo("{\"text\":\"Hello World!\",\"person\":{\"name\":\"John\",\"surname\":\"Doe\"},\"index\":5,\"numbers\":[10,20,30,40],\"id\":\"x123456789x\"}"));

        ClassicAssert.IsNotNull(Context.GetVariable("values"));
        Assert.That(Context.GetVariable("values"),
            Is.EqualTo("Hello World!, {\"name\":\"John\",\"surname\":\"Doe\"}, 5, [10,20,30,40], x123456789x"));

        ClassicAssert.IsNotNull(Context.GetVariable("size"));
        Assert.That(Context.GetVariable("size"), Is.EqualTo("5"));

        ClassicAssert.IsNotNull(Context.GetVariable("all"));
        Assert.That(Context.GetVariable("all"),
            Is.EqualTo("Hello World!, {\"name\":\"John\",\"surname\":\"Doe\"}, 5, [10,20,30,40], x123456789x"));

        ClassicAssert.IsNotNull(Context.GetVariable("root"));
        Assert.That(
            Context.GetVariable("root"),
            Is.EqualTo("{\"text\":\"Hello World!\",\"person\":{\"name\":\"John\",\"surname\":\"Doe\"},\"index\":5,\"numbers\":[10,20,30,40],\"id\":\"x123456789x\"}"));
    }
}
