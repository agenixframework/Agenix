using Agenix.Core.NUnitTestProject;
using Agenix.Core.Validation.Json.Dsl;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Validation.Json
{
    public class JsonPathVariableExtractorTest : AbstractNUnitSetUp
    {
        private readonly string _jsonPayload =
            "{\"text\":\"Hello World!\", \"person\":{\"name\":\"John\",\"surname\":\"Doe\"}, \"index\":5, \"numbers\": [10,20,30,40], \"id\":\"x123456789x\"}";

        [Test]
        public void TestExtractVariables()
        {
            JsonSupport.Json().JsonPath().Extract()
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
                .Build()
                .ExtractVariables(_jsonPayload, Context);

            ClassicAssert.IsNotNull(Context.GetVariable("keySet"));
            ClassicAssert.AreEqual("text, person, index, numbers, id", Context.GetVariable("keySet"));


            ClassicAssert.IsNotNull(Context.GetVariable("index"));
            ClassicAssert.AreEqual("5", Context.GetVariable("index"));


            ClassicAssert.IsNotNull(Context.GetVariable("numbers"));
            ClassicAssert.AreEqual("[10,20,30,40]", Context.GetVariable("numbers"));


            ClassicAssert.IsNotNull(Context.GetVariable("numbersSize"));
            ClassicAssert.AreEqual("4", Context.GetVariable("numbersSize"));

            ClassicAssert.IsNotNull(Context.GetVariable("person"));
            ClassicAssert.AreEqual("{\"name\":\"John\",\"surname\":\"Doe\"}", Context.GetVariable("person"));

            ClassicAssert.IsNotNull(Context.GetVariable("personName"));
            ClassicAssert.AreEqual("John", Context.GetVariable("personName"));

            ClassicAssert.IsNotNull(Context.GetVariable("toString"));
            ClassicAssert.AreEqual(
                "{\"text\":\"Hello World!\",\"person\":{\"name\":\"John\",\"surname\":\"Doe\"},\"index\":5,\"numbers\":[10,20,30,40],\"id\":\"x123456789x\"}",
                Context.GetVariable("toString"));

            ClassicAssert.IsNotNull(Context.GetVariable("values"));
            ClassicAssert.AreEqual("Hello World!, {\"name\":\"John\",\"surname\":\"Doe\"}, 5, [10,20,30,40], x123456789x",
                Context.GetVariable("values"));

            ClassicAssert.IsNotNull(Context.GetVariable("size"));
            ClassicAssert.AreEqual("5", Context.GetVariable("size"));

            ClassicAssert.IsNotNull(Context.GetVariable("all"));
            ClassicAssert.AreEqual("Hello World!, {\"name\":\"John\",\"surname\":\"Doe\"}, 5, [10,20,30,40], x123456789x",
                Context.GetVariable("all"));

            ClassicAssert.IsNotNull(Context.GetVariable("root"));
            ClassicAssert.AreEqual(
                "{\"text\":\"Hello World!\",\"person\":{\"name\":\"John\",\"surname\":\"Doe\"},\"index\":5,\"numbers\":[10,20,30,40],\"id\":\"x123456789x\"}",
                Context.GetVariable("root"));
        }
    }
}